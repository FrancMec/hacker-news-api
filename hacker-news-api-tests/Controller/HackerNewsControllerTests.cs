namespace hacker_news_api_tests.Controller
{
    using hacker_news_api.Controllers;
    using hacker_news_wrapper.Interfaces;
    using hacker_news_wrapper.Models.Response;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Xunit;

    public class HackerNewsControllerTests
    {
        private Mock<IHackerNewsService> _mockHackerNewsService;
        private HackerNewsController _controller;

        public HackerNewsControllerTests()
        {
            _mockHackerNewsService = new Mock<IHackerNewsService>();
            _controller = new HackerNewsController(_mockHackerNewsService.Object);
        }

        [Fact]
        public async Task Get_Returns_Ok_Result_With_Stories()
        {
            // Arrange
            var hackerNewsResponse = new HackerNewsResponse
            {
                Data = new List<StoryResponse>
            {
                new StoryResponse { Title = "Story 1", Url = "http://news/story/1" },
                new StoryResponse { Title = "Sport Story 2", Url = "http://sport/story/2" }
            }
            };

            _mockHackerNewsService.Setup(service => service.GetStoriesAsync(default)).ReturnsAsync(hackerNewsResponse);

            // Act
            var result = await _controller.Get(default);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<HackerNewsResponse>(okResult.Value);
            Assert.Equal(hackerNewsResponse, response);

        }

        [Fact]
        public async Task Get_ReturnsInternalServerError_OnException()
        {
            // Arrange
            var hackerNewsResponse = new HackerNewsResponse
            {
                Data = null,
                StatusCode = StatusCodes.Status500InternalServerError,
            };
            _mockHackerNewsService.Setup(service => service.GetStoriesAsync(It.IsAny<CancellationToken>()))
                                  .ReturnsAsync(hackerNewsResponse);

            // Act
            var result = await _controller.Get(default);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<HackerNewsResponse>(okResult.Value);
            Assert.Equal(StatusCodes.Status500InternalServerError, returnValue.StatusCode);
        }

    }
}
