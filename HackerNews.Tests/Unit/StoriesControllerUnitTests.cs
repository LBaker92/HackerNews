using HackerNews.Controllers;
using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace HackerNews.Tests.Controllers
{
  public class StoriesControllerUnitTests
  {
    [Theory]
    [InlineData(-1, -1)]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    public async void GetNewsStories_ShouldReturnBadRequest_WhenNegativePageIndexOrPageSizeEntered(int pageIndex, int pageSize)
    {
      // Arrange 
      var storyData = new StoryData()
      {
        Errors = new List<string>()
        {
          "Page index and page size cannot be negative numbers."
        }
      };

      var mockHttpClientFactory = new Mock<IHttpClientFactory>();
      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

      mockHttpMessageHandler.Protected()
        .Setup<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(new HttpResponseMessage()
        {
          StatusCode = HttpStatusCode.BadRequest,
          Content = new StringContent(JsonSerializer.Serialize(storyData))
        });

      var httpClient = new HttpClient(mockHttpMessageHandler.Object);

      mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      var cache = new MemoryCache(new MemoryCacheOptions());
      var controller = new StoriesController(mockHttpClientFactory.Object, cache);

      // Act
      ObjectResult result = await controller.GetNewestStories(pageIndex, pageSize) as ObjectResult;
      StoryData resultStoryData = (StoryData)result.Value;

      // Assert
      Assert.NotNull(result);
      Assert.Equal(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);

      Assert.NotNull(resultStoryData);
      Assert.Single(resultStoryData.Errors);
      Assert.Equal("Page index and page size cannot be negative numbers.", resultStoryData.Errors[0]);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(11, 1)]
    [InlineData(1, 10)]
    public async void GetNewsStories_ShouldReturnBadRequest_WhenPageIndexDoesNotExist(int pageIndex, int pageSize)
    {
      // Arrange 
      var storyIds = new List<int>()
      {
        111111111,
        222222222,
        333333333,
        444444444,
        555555555
      };
      var stories = new List<Story>()
      {
        new Story()
        {
          Title = "Test",
          Url = "http://test.com"
        },
        new Story()
        {
          Title = "Test2",
          Url = "http://test2.com"
        },
        new Story()
        {
          Title = "Test3",
          Url = "http://test3.com"
        },
        new Story()
        {
          Title = "Test4",
          Url = "http://test4.com"
        },
        new Story()
        {
          Title = "Test5",
          Url = "http://test5.com"
        },
      };
      var storyData = new StoryData()
      {
        Errors = new List<string>()
        {
          "Requested page index doesn't exist."
        }
      };
      var storyIdsResponse = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(JsonSerializer.Serialize(storyIds)),
      };
      var storyResponses = new List<HttpResponseMessage>()
      {
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[0])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[1])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[2])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[3])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[4])),
        },
      };

      var mockHttpClientFactory = new Mock<IHttpClientFactory>();
      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

      mockHttpMessageHandler.Protected()
        .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(storyIdsResponse)
        .ReturnsAsync(storyResponses[0])
        .ReturnsAsync(storyResponses[1])
        .ReturnsAsync(storyResponses[2])
        .ReturnsAsync(storyResponses[3])
        .ReturnsAsync(storyResponses[4])
        .ReturnsAsync(new HttpResponseMessage()
        {
          StatusCode = HttpStatusCode.BadRequest,
          Content = new StringContent(JsonSerializer.Serialize(storyData))
        });

      var httpClient = new HttpClient(mockHttpMessageHandler.Object);

      mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      var cache = new MemoryCache(new MemoryCacheOptions());
      var controller = new StoriesController(mockHttpClientFactory.Object, cache);

      // Act
      ObjectResult result = await controller.GetNewestStories(pageIndex, pageSize) as ObjectResult;
      StoryData resultStoryData = (StoryData)result.Value;

      // Assert
      Assert.NotNull(result);
      Assert.Equal(HttpStatusCode.BadRequest, (HttpStatusCode)result.StatusCode);

      Assert.NotNull(resultStoryData);
      Assert.Single(resultStoryData.Errors);
      Assert.Equal("Requested page index doesn't exist.", resultStoryData.Errors[0]);
    }

    [Fact]
    public async void GetNewsStories_ShouldReturnStoryData_WhenStoriesAreNotCached()
    {
      // Arrange 
      var storyIds = new List<int>()
      {
        111111111,
        222222222,
        333333333,
        444444444,
        555555555
      };
      var stories = new List<Story>()
      {
        new Story()
        {
          Title = "Test",
          Url = "http://test.com"
        },
        new Story()
        {
          Title = "Test2",
          Url = "http://test2.com"
        },
        new Story()
        {
          Title = "Test3",
          Url = "http://test3.com"
        },
        new Story()
        {
          Title = "Test4",
          Url = "http://test4.com"
        },
        new Story()
        {
          Title = "Test5",
          Url = "http://test5.com"
        },
      };
      var storyIdsResponse = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(JsonSerializer.Serialize(storyIds)),
      };
      var storyResponses = new List<HttpResponseMessage>()
      {
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[0])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[1])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[2])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[3])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[4])),
        },
      };

      var mockHttpClientFactory = new Mock<IHttpClientFactory>();
      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

      mockHttpMessageHandler.Protected()
        .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(storyIdsResponse)
        .ReturnsAsync(storyResponses[0])
        .ReturnsAsync(storyResponses[1])
        .ReturnsAsync(storyResponses[2])
        .ReturnsAsync(storyResponses[3])
        .ReturnsAsync(storyResponses[4]);

      var httpClient = new HttpClient(mockHttpMessageHandler.Object);

      mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      var cache = new MemoryCache(new MemoryCacheOptions());
      var controller = new StoriesController(mockHttpClientFactory.Object, cache);

      // Act
      ObjectResult result = await controller.GetNewestStories(0, 5) as ObjectResult;
      StoryData resultStoryData = (StoryData)result.Value;

      // Assert
      Assert.NotNull(result);
      Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);

      Assert.NotNull(resultStoryData);
      Assert.Equal(stories.Count, resultStoryData.Stories.Count);
      Assert.Equal(stories.Count, resultStoryData.TotalStories);

      for (int i = 0; i < stories.Count; ++i)
      {
        Assert.Equal(stories[i].Title, resultStoryData.Stories[i].Title);
        Assert.Equal(stories[i].Url, resultStoryData.Stories[i].Url);
      }
    }

    [Fact]
    public async void GetNewsStories_ShouldReturnStoryData_WhenStoriesAreCached()
    {
      // Arrange 
      var storyIds = new List<int>()
      {
        111111111,
        222222222,
        333333333,
        444444444,
        555555555
      };
      var stories = new List<Story>()
      {
        new Story()
        {
          Title = "Test",
          Url = "http://test.com"
        },
        new Story()
        {
          Title = "Test2",
          Url = "http://test2.com"
        },
        new Story()
        {
          Title = "Test3",
          Url = "http://test3.com"
        },
        new Story()
        {
          Title = "Test4",
          Url = "http://test4.com"
        },
        new Story()
        {
          Title = "Test5",
          Url = "http://test5.com"
        },
      };
      var storyIdsResponse = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(JsonSerializer.Serialize(storyIds)),
      };
      var storyResponses = new List<HttpResponseMessage>()
      {
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[0])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[1])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[2])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[3])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[4])),
        },
      };

      var mockHttpClientFactory = new Mock<IHttpClientFactory>();
      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

      mockHttpMessageHandler.Protected()
        .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(storyIdsResponse)
        .ReturnsAsync(storyResponses[0])
        .ReturnsAsync(storyResponses[1])
        .ReturnsAsync(storyResponses[2])
        .ReturnsAsync(storyResponses[3])
        .ReturnsAsync(storyResponses[4]);

      var httpClient = new HttpClient(mockHttpMessageHandler.Object);

      mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      var cache = new MemoryCache(new MemoryCacheOptions());
      for (int i = 0; i < stories.Count; ++i)
      {
        cache.Set(storyIds[i], stories[i]);
      }

      var controller = new StoriesController(mockHttpClientFactory.Object, cache);

      // Act
      ObjectResult result = await controller.GetNewestStories(0, 5) as ObjectResult;
      StoryData resultStoryData = (StoryData)result.Value;

      // Assert
      Assert.NotNull(result);
      Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);

      Assert.NotNull(resultStoryData);
      Assert.Equal(stories.Count, resultStoryData.Stories.Count);
      Assert.Equal(stories.Count, resultStoryData.TotalStories);

      for (int i = 0; i < stories.Count; ++i)
      {
        Assert.Equal(resultStoryData.Stories[i].Title, stories[i].Title);
        Assert.Equal(resultStoryData.Stories[i].Url, stories[i].Url);
      }
    }

    [Theory]
    [InlineData("T")]
    [InlineData("Te")]
    [InlineData("Tes")]
    [InlineData("Test")]
    public async void GetNewsStories_ShouldReturnStoryData_WhenSearchingTitles(string title)
    {
      // Arrange 
      var storyIds = new List<int>()
      {
        111111111,
        222222222,
        333333333,
        444444444,
        555555555
      };
      var stories = new List<Story>()
      {
        new Story()
        {
          Title = "Test",
          Url = "http://test.com"
        },
        new Story()
        {
          Title = "Test2",
          Url = "http://test2.com"
        },
        new Story()
        {
          Title = "Test3",
          Url = "http://test3.com"
        },
        new Story()
        {
          Title = "Test4",
          Url = "http://test4.com"
        },
        new Story()
        {
          Title = "Test5",
          Url = "http://test5.com"
        },
      };
      var storyIdsResponse = new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(JsonSerializer.Serialize(storyIds)),
      };
      var storyResponses = new List<HttpResponseMessage>()
      {
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[0])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[1])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[2])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[3])),
        },
        new HttpResponseMessage
        {
          StatusCode = HttpStatusCode.OK,
          Content = new StringContent(JsonSerializer.Serialize(stories[4])),
        },
      };

      var mockHttpClientFactory = new Mock<IHttpClientFactory>();
      var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

      mockHttpMessageHandler.Protected()
        .SetupSequence<Task<HttpResponseMessage>>("SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(storyIdsResponse)
        .ReturnsAsync(storyResponses[0])
        .ReturnsAsync(storyResponses[1])
        .ReturnsAsync(storyResponses[2])
        .ReturnsAsync(storyResponses[3])
        .ReturnsAsync(storyResponses[4]);

      var httpClient = new HttpClient(mockHttpMessageHandler.Object);

      mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
        .Returns(httpClient);

      var cache = new MemoryCache(new MemoryCacheOptions());
      for (int i = 0; i < stories.Count; ++i)
      {
        cache.Set(storyIds[i], stories[i]);
      }

      var controller = new StoriesController(mockHttpClientFactory.Object, cache);

      // Act
      ObjectResult result = await controller.GetNewestStories(0, 5, title) as ObjectResult;
      StoryData resultStoryData = (StoryData)result.Value;

      // Assert
      Assert.NotNull(result);
      Assert.Equal(HttpStatusCode.OK, (HttpStatusCode)result.StatusCode);

      Assert.NotNull(resultStoryData);
      Assert.Equal(stories.Count, resultStoryData.Stories.Count);
      Assert.Equal(stories.Count, resultStoryData.TotalStories);

      for (int i = 0; i < stories.Count; ++i)
      {
        Assert.Equal(resultStoryData.Stories[i].Title, stories[i].Title);
        Assert.Equal(resultStoryData.Stories[i].Url, stories[i].Url);
      }
    }
  }
}
