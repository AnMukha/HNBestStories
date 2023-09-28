using HNBestStories.Dto;

namespace HNBestStories.Services
{
    public interface IStoriesFetcher
    {
        public Task<List<(int id, StoryFetchDto story)>> FetchStories(IEnumerable<int> storyIds);
    }
}
