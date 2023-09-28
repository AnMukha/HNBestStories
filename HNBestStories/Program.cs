using HNBestStories;
using HNBestStories.Endpoints;
using HNBestStories.Middlewares;
using HNBestStories.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<HNBestStoriesService>();
builder.Services.AddSingleton<IStoriesFetcher, StoriesFetcher>();

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("Options"));

var app = builder.Build();

app.ConfigureExceptionHandler(app.Services.GetService<ILogger>()!);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

BestStoriesEndpoint.AddEndpoint(app);

app.Run();
