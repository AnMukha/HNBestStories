namespace HNBestStories.Dto
{
    public static class Mapper
    {
        public static StoryInResponseDto MapStoryToResponseDto(BestStoryDto story)
        {
            return new StoryInResponseDto()
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
