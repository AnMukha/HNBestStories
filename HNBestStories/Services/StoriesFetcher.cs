using HNBestStories.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HNBestStories.Services
{
    public class StoriesFetcher: IStoriesFetcher
    {

        private readonly HttpClient _httpClient;
        private readonly IOptions<AppOptions> _options;
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };        

        public StoriesFetcher(HttpClient httpClient, IOptions<AppOptions> options)
        {
            _options = options;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.APIUrl);            
        }

        public async Task<List<(int id, StoryFetchDto story)>> FetchStories(IEnumerable<int> storyIds)
        {
            var portionSize = _options.Value.NumberOfParallelRequests;
            var idArray = storyIds.ToArray();
            var result = new List<(int id, StoryFetchDto story)>();
            for (var i = 0; i < idArray.Length; i += portionSize)
            {
                var endPos = Math.Min(i + portionSize, idArray.Length);
                result.AddRange(await FetchStoriesInParallel(idArray[i..endPos]));
            }
            return result;
        }

        private async Task<List<(int, StoryFetchDto)>> FetchStoriesInParallel(int[] ids)
        {
            var tasksByIds = ids.Select(id => (id, task: _httpClient.GetStringAsync($"item/{id}.json"))).ToArray();
            await Task.WhenAll(tasksByIds.Select(t => t.task));
            var result = new List<(int, StoryFetchDto)>();
            foreach (var (id, task) in tasksByIds)
            {
                var storyDto = JsonSerializer.Deserialize<StoryFetchDto>(await task, jsonOptions);
                if (storyDto is not null)
                {
                    result.Add((id, storyDto));
                }
            }
            return result;
        }

    }
}
