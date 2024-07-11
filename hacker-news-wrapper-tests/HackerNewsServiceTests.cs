
namespace hacker_news_wrapper_tests
{
    using hacker_news_wrapper.Implementation;
    using hacker_news_wrapper.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using Moq.Protected;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;
    using Xunit.Extensions.Ordering;

    public class HackerNewsServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly HackerNewsService _service;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _mockHttpClient;

        public HackerNewsServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockMemoryCache = new Mock<IMemoryCache>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            _mockHttpClientFactory.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(_mockHttpClient);

            _service = new HackerNewsService(_mockHttpClientFactory.Object, _mockMemoryCache.Object);
        }

        [Fact, Order(1)]
        public async Task GetStoriesAsync_ReturnsStories_WhenNotCached()
        {
            // Arrange
            var storyIds = new List<int> { 1, 2 };
            var stories = new List<StoryResponse>
            {
                new StoryResponse { Id = 1, Title = "Story 1", Url = "http://story1.com", Type = "story" }
            };
            var mockResponse = new HackerNewsResponse { StatusCode = StatusCodes.Status200OK, Data = stories };

            _mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<string>())).Returns(Mock.Of<ICacheEntry>);

            var client = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            _mockHttpClientFactory.Setup(factory => factory.CreateClient("hackerNews")).Returns(client);
            SetupHttpResponseMessage("/v0/newstories.json", storyIds);


            foreach (var story in stories)
            {
                SetupHttpResponseMessage($"/v0/item/{story.Id}.json", story);
            }

            // Act
            var result = await _service.GetStoriesAsync(default);

            // Assert
            HackerNewsResponse response = result as HackerNewsResponse;
            var storiesResponse = response.Data as List<StoryResponse>;
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            Assert.Equal(stories.Count, storiesResponse?.Count);
        }

        [Fact, Order(2)]
        public async void GetStoriesAsync_Returns_Stories()
        {
            // Arrange
            var storyIds = new List<int> { 1, 2, 3 };
            var stories = new List<StoryResponse>
        {
            new StoryResponse { Id = 1, Type = "story", Url = "http://test/story/1" },
        };

            SetupHttpResponseMessage("/v0/newstories.json", storyIds);
            foreach (var story in stories)
            {
                SetupHttpResponseMessage($"/v0/item/{story.Id}.json", story);
            }


            // Act
            var result = await _service.GetStoriesAsync(default);
            HackerNewsResponse response = result as HackerNewsResponse;
            var storiesResponse = response.Data as List<StoryResponse>;
            // Assert
            Assert.NotNull(storiesResponse);
            Assert.Equal(1, storiesResponse?.Count);
            Assert.Contains(1, storiesResponse?.Select(x => x.Id).ToList());
        }


        [Fact, Order(3)]
        public async Task GetStoriesAsync_Handles_Exception()
        {
            // Arrange
            SetupHttpResponseMessage("/v0/newstories1.json", StatusCodes.Status500InternalServerError);

            // Act
            var result = await _service.GetStoriesAsync(default);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
            Assert.Null(result.Data);
        }

        [Fact, Order(4)]
        public async Task GetStoryByIdAsync_Returns_Story()
        {
            // Arrange
            var story = new StoryResponse { Id = 1, Type = "story", Url = "http://test/story/1" };

            SetupHttpResponseMessage($"/v0/item/{story.Id}.json", story);


            // Act
            var result = await _service.GetStoryByIdAsync(1, default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact, Order(5)]
        public async Task GetStoryIdsAsync_Returns_StoryIds()
        {
            // Arrange
            var storyIds = new List<int> { 1, 2, 3 };

            SetupHttpResponseMessage("/v0/newstories.json", storyIds);


            // Act
            var result = await _service.GetStoryIdsAsync(default);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Contains(1, result);
            Assert.Contains(2, result);
            Assert.Contains(3, result);
        }
        
        private void SetupHttpResponseMessage<T>(string requestUri, T content)
        {
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.PathAndQuery == requestUri),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(content), System.Text.Encoding.UTF8, "application/json")
                });
        }

    }

}