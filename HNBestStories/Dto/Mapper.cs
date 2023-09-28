namespace HNBestStories.Dto
{
    public static class Mapper
    {
        public static StoryResponseDto MapStoryToResponseDto(StoryFetchDto story)
        {
            return new StoryResponseDto()
            {
                PostedBy = story.By,
                CommentCount = story.Kids?.Count() ?? 0,
                Score = story.Score,
                Uri = story.Url,
                Title = story.Title,
                Time = DateTimeOffset.FromUnixTimeSeconds(story.Time)
            };
        }

    }
}
