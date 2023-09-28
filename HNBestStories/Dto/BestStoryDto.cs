namespace HNBestStories.Dto
{
    public class BestStoryDto
    {
        public string? Title { get; set; }
        public string? Url { get; set; }
        public string? By { get; set; }
        public int Time { get; set; }
        public int Score { get; set; }
        public IEnumerable<int>? Kids { get; set; }
    }
}

