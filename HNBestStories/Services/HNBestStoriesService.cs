using HNBestStories.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HNBestStories.Services
{
    public class HNBestStoriesService
    {        
        private readonly IMemoryCache _cache;
        private readonly IStoriesFetcher _storiesFetcher;
        private readonly HttpClient _httpClient;
        private readonly IOptions<AppOptions> _options;
        private readonly ILogger<HNBestStoriesService> _logger;

        private const string IdsCacheKey = "ids_key";
        private const string BestStoriesAddress = "beststories.json";

        private readonly SemaphoreSlim _idsReadSemaphore = new(1, 1);
        private readonly SemaphoreSlim _storiesReadSemaphore = new(1, 1);

        public HNBestStoriesService(HttpClient httpClient, 
                                    IMemoryCache cache, 
                                    IStoriesFetcher storiesFetcher, 
                                    IOptions<AppOptions> options,
                                    ILogger<HNBestStoriesService> logger)
        {
            _options = options;
            _cache = cache;
            _storiesFetcher = storiesFetcher;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.APIUrl);
            _logger = logger;
        }

        public async Task<List<StoryResponseDto>> GetBestStories(int number)
        {
            _logger.LogInformation("Received request for {number} best stories.", number);
            var allStoriesIds = await GetStoriesIds() ?? throw new Exception("Failed to retrieve story IDs.");
            _logger.LogDebug("Rreceived story IDs.");            
            var result = await GetStories(allStoriesIds[..number]);
            _logger.LogInformation("Rreceived {Count} stories from HN API.", result.Count);            
            return result;
        }

        private async Task<int[]?> GetStoriesIds()
        {
            if (_cache.TryGetValue(IdsCacheKey, out int[]? idsFromCache))
            {
                _logger.LogDebug("Successfully retrieved story IDs from cache.");
                return idsFromCache;                
            }

            await _idsReadSemaphore.WaitAsync();
            try
            {
                if (_cache.TryGetValue(IdsCacheKey, out int[]? idsAppeared))
                {
                    _logger.LogDebug("Successfully fetched story IDs from cache following thread unlock.");
                    return idsAppeared;
                }

                var idsResponse = await _httpClient.GetStringAsync(BestStoriesAddress);
                _logger.LogDebug("Successfully fetched IDs from HN API.");
                var ids = JsonSerializer.Deserialize<IEnumerable<int>>(idsResponse)?.ToArray();
                _cache.Set(IdsCacheKey, ids, TimeSpan.FromSeconds(_options.Value.IdsCacheExpirationSecounds));
                _logger.LogDebug("IDs was deserialized and updated in the cache.");
                return ids;
            }
            finally
            {
                _idsReadSemaphore.Release();
            }
        }

        private async Task<List<StoryResponseDto>> GetStories(int[] requestedStoriesIds)
        {
            var readedFromCache = GetStoriesFromCache(requestedStoriesIds);
            if (readedFromCache.Count == requestedStoriesIds.Length)
            {
                _logger.LogDebug("Successfully retrieved {Length} stories from cache.", requestedStoriesIds.Length);
                return readedFromCache.Select(s => s.story).ToList();
            }

            await _storiesReadSemaphore.WaitAsync();
            try
            {
                var readedAfterWait = GetStoriesFromCache(requestedStoriesIds);
                if (readedAfterWait.Count == requestedStoriesIds.Length)
                {
                    _logger.LogDebug("Successfully retrieved {Length} stories from cache following thread unlock.", requestedStoriesIds.Length);
                    return readedAfterWait.Select(s => s.story).ToList();
                }

                var idsToFetch = requestedStoriesIds.Except(readedAfterWait.Select(s => s.id)).ToArray();
                var fetchedStories = await _storiesFetcher.FetchStories(idsToFetch);
                _logger.LogDebug("Successfully fetched {Length} stories from HN API.", fetchedStories.Count);

                var fechedToResponse = fetchedStories.Select(s => (s.id, story: Mapper.MapStoryToResponseDto(s.story))).ToList();
                fechedToResponse.ForEach(s => _cache.Set(s.id, s.story, TimeSpan.FromSeconds(_options.Value.StoriesCacheExpirationSecounds)));
                _logger.LogDebug("{Count} stories was deserialized and updated in the cache.", fechedToResponse.Count);

                return readedAfterWait.Concat(fechedToResponse).Select(s => s.story).ToList();
            }
            finally
            {
                _storiesReadSemaphore.Release();
            }          
        }

        private List<(int id, StoryResponseDto story)> GetStoriesFromCache(IEnumerable<int> storiesId)
        {            
            return storiesId.Select(id => (id, story: _cache.Get(id) as StoryResponseDto)).Where(s => s.story is not null).ToList()!;
        }
    }
}
