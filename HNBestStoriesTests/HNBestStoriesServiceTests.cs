using HNBestStories;
using HNBestStories.Dto;
using HNBestStories.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HNBestStoriesTests
{
    public class HNBestStoriesServiceTests
    {
        private Mock<HttpClient> _httpClientMock;
        private Mock<IMemoryCache> _memoryCacheMock;
        private Mock<IStoriesFetcher> _storiesFetcherMock;
        private Mock<IOptions<AppOptions>> _optionsMock;
        private Mock<ILogger<HNBestStoriesService>> _loggerMock;
        private Mock<StoriesFetcher> _storiesFetcher;

        private HNBestStoriesService _service;

        [SetUp]
        public void SetUp()
        {
            _httpClientMock = new Mock<HttpClient>();
            _memoryCacheMock = new Mock<IMemoryCache>();
            _storiesFetcherMock = new Mock<IStoriesFetcher>();
            _loggerMock = new Mock<ILogger<HNBestStoriesService>>();
            _optionsMock = new Mock<IOptions<AppOptions>>();
            _storiesFetcherMock = new Mock<IStoriesFetcher>();
            _optionsMock.Setup(o => o.Value).Returns(new AppOptions());
            

            _service = new HNBestStoriesService(_httpClientMock.Object, _memoryCacheMock.Object,
                                                _storiesFetcherMock.Object, _optionsMock.Object,
                                                _loggerMock.Object);
        }

        [Test]
        public async Task GetBestStories_ShouldReturnStoryFromCache_WhenStoryInCache()
        {
            // Arrange            
            _memoryCacheMock.Setup(m => m.TryGetValue(It.Is<string>(key => key.GetType() == typeof(string)), out It.Ref<object>.IsAny!))
                .Returns((string key, out object value) =>
                {
                    value = new int[] { 1, 2, 3 };
                    return true;
                });
            _memoryCacheMock.Setup(m => m.TryGetValue(It.Is<int>(key => key == 1), out It.Ref<object>.IsAny!))
                            .Returns((int key, out StoryResponseDto value) =>
                            {
                                value = new StoryResponseDto() { };
                                return true;
                            });
            // Act
            var result = await _service.GetBestStories(1);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
        }
    }
}