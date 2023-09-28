using HNBestStories.Services;

namespace HNBestStories.Endpoints
{
    public static class BestStoriesEndpoint
    {
        public static void AddEndpoint(WebApplication application)
        {
            application.MapGet("api/get-best-stories", GetBestStories).WithOpenApi(operation => new(operation)
            {
                Summary = "Get n best stories",
            });
        }

        private static async Task<IResult> GetBestStories(HNBestStoriesService bestStoriesService, int n)
        {
            var bestStories = await bestStoriesService.GetBestStories(n);
            return Results.Ok(bestStories.OrderByDescending(s => s.Score));
        }

    }
}
