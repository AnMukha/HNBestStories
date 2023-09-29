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
            if (n < 0)
            {
                return Results.BadRequest("Invalid value for n. It should be greater than or equal to 0.");
            }
            var bestStories = await bestStoriesService.GetBestStories(n);
            return Results.Ok(bestStories.OrderByDescending(s => s.Score));
        }

    }
}
