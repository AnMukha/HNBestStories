using HNBestStories.Dto;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HNBestStories.Services
{
    public class StoriesFetcher
    {

        private readonly HttpClient _httpClient;
        IOptions<Options> _options;
        private static readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };        

        public StoriesFetcher(HttpClient httpClient, IOptions<Options> options)
        {
            _options = options;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.APIUrl);            
        }

        public async Task<List<(int id, BestStoryDto story)>> FetchStories(IEnumerable<int> storyIds)
        {
            var portionSize = _options.Value.NumberOfParallelRequests;
            var idArray = storyIds.ToArray();
            var result = new List<(int, BestStoryDto)>();
            for (var i = 0; i < idArray.Length; i += portionSize)
            {
                var endPos = Math.Min(i + portionSize, idArray.Length);
                result.AddRange(await FetchStoriesInParallel(idArray[i..endPos]));
            }
            return result;
        }

        private async Task<List<(int, BestStoryDto)>> FetchStoriesInParallel(int[] ids)
        {
            var tasksByIds = ids.Select(id => (id, task: _httpClient.GetStringAsync($"item/{id}.json"))).ToArray();
            await Task.WhenAll(tasksByIds.Select(t => t.task));
            var result = new List<(int, BestStoryDto)>();
            foreach (var (id, task) in tasksByIds)
            {
                var storyDto = JsonSerializer.Deserialize<BestStoryDto>(await task, jsonOptions);
                if (storyDto is not null)
                {
                    result.Add((id, storyDto));
                }
            }
            return result;
        }

    }
}
