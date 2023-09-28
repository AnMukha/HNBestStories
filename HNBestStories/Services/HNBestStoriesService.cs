using HNBestStories.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HNBestStories.Services
{
    public class HNBestStoriesService
    {        
        private readonly IMemoryCache _cache;
        private readonly StoriesFetcher _storiesFetcher;
        private readonly HttpClient _httpClient;
        private IOptions<Options> _options;

        private const string IdsCacheKey = "ids_key";
        private const string BestStoriesAddress = "beststories.json";

        private readonly SemaphoreSlim _idsReadSemaphore = new(1, 1);
        private readonly SemaphoreSlim _storiesReadSemaphore = new(1, 1);

        public HNBestStoriesService(HttpClient httpClient, IMemoryCache cache, StoriesFetcher storiesFetcher, IOptions<Options> options)
        {
            _options = options;
            _cache = cache;
            _storiesFetcher = storiesFetcher;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.APIUrl);            
        }

        public async Task<IEnumerable<IdAndStoryPair>> GetBestStories(int number)
        {
            var allStoriesIds = await GetStoriesIds() ?? throw new Exception();
            var requestedStoriesIds = allStoriesIds.Take(number).ToArray();
            return await GetStories(requestedStoriesIds);            
        }

        private async Task<IEnumerable<int>?> GetStoriesIds()
        {
            if (_cache.TryGetValue(IdsCacheKey, out IEnumerable<int>? idsFromCache)) return idsFromCache;

            await _idsReadSemaphore.WaitAsync();
            try
            {                                
                if (_cache.TryGetValue(IdsCacheKey, out IEnumerable<int>? idsAppeared)) return idsAppeared;

                var idsResponse = await _httpClient.GetStringAsync(BestStoriesAddress);
                var ids = JsonSerializer.Deserialize<IEnumerable<int>>(idsResponse)?.ToArray();
                _cache.Set(IdsCacheKey, ids, TimeSpan.FromSeconds(_options.Value.IdsCacheExpirationSecounds));
                return ids;
            }
            finally
            {
                _idsReadSemaphore.Release();
            }
        }

        private async Task<IEnumerable<IdAndStoryPair>> GetStories(int[] requestedStoriesIds)
        {                        

            var storiesToFill = requestedStoriesIds.Select(id => new IdAndStoryPair(id, null));
            var filledFromCache = FillStoriesFromCache(storiesToFill);
            if (filledFromCache.All(s => s.Story is not null)) return filledFromCache;

            await _storiesReadSemaphore.WaitAsync();
            try
            {
                var filledAfterWait = FillStoriesFromCache(filledFromCache);
                if (filledAfterWait.All(s => s.Story is not null)) return filledAfterWait;                

                var fetchedStories = await _storiesFetcher.FetchStories(filledAfterWait.Where(s => s.Story is null).Select(s => s.Id));                
                var storiesById = fetchedStories.ToDictionary(s => s.id, s => Mapper.MapStoryToResponseDto(s.story));
                storiesById.Select(s => _cache.Set(s.Key, s.Value, TimeSpan.FromSeconds(_options.Value.StoriesCacheExpirationSecounds))).ToArray();
                
                return filledAfterWait.Select(s => new IdAndStoryPair(s.Id, s.Story ?? storiesById[s.Id]));
            }
            finally 
            {
                _storiesReadSemaphore.Release();
            }          
        }

        private IdAndStoryPair[] FillStoriesFromCache(IEnumerable<IdAndStoryPair> storiesById)
        {
            return storiesById.Select(s => new IdAndStoryPair(s.Id, _cache.Get(s.Id) as StoryInResponseDto)).ToArray();
        }
    }
}
