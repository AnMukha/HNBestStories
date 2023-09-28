namespace HNBestStories
{
    public class Options
    {
        public int NumberOfParallelRequests { get; set; } = 10;
        public string APIUrl { get; set; } = "https://hacker-news.firebaseio.com/v0/";
        public int IdsCacheExpirationSecounds { get; set; } = 5;
        public int StoriesCacheExpirationSecounds { get; set; } = 60;
    }
}
