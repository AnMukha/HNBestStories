using HNBestStories;
using HNBestStories.Endpoints;
using HNBestStories.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<HNBestStoriesService>();
builder.Services.AddSingleton<StoriesFetcher>();

builder.Services.Configure<Options>(builder.Configuration.GetSection("Options"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

BestStoriesEndpoint.AddEndpoint(app);

app.Run();
