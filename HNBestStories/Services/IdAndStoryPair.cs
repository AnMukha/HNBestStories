using HNBestStories.Dto;
namespace HNBestStories.Services
{
    public record IdAndStoryPair(int Id, StoryInResponseDto? Story);
}
