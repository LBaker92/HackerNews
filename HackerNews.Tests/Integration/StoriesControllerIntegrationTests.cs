using HackerNews.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace HackerNews.Tests.Integration
{
  public class StoriesControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
  {
    private readonly WebApplicationFactory<Program> Factory;

    public StoriesControllerIntegrationTests(WebApplicationFactory<Program> factory) 
    {
      Factory = factory;
    }

    [Theory]
    [InlineData(0, 5, "")]
    [InlineData(0, 25, "")]
    [InlineData(0, 50, "")]
    [InlineData(0, 100, "")]
    [InlineData(0, 5, "T")] // Assumes an article exists that starts with a t.
    [InlineData(0, 5, "A")] // Assumes an article exists that starts with an a.
    [InlineData(1, 5, "")]
    [InlineData(2, 25, "")]
    [InlineData(3, 50, "")]
    [InlineData(4, 50, "")]
    public async Task GetNewestStories_ShouldReturnResults(int pageIndex, int pageSize, string title)
    {
      // Arrange
      var client = Factory.CreateClient();

      // Act
      HttpResponseMessage response;

      if (!string.IsNullOrWhiteSpace(title))
      {
        response = await client.GetAsync($"api/stories?pageIndex={pageIndex}&pageSize={pageSize}&title={title}");
      }
      else
      {
        response = await client.GetAsync($"api/stories?pageIndex={pageIndex}&pageSize={pageSize}");
      }

      response.EnsureSuccessStatusCode();

      var storyData = JsonSerializer.Deserialize<StoryData>(await response.Content.ReadAsStringAsync());

      // Assert
      Assert.IsType<StoryData>(storyData);
      Assert.NotEmpty(storyData.Stories);
      Assert.Empty(storyData.Errors);
      Assert.NotEqual(0, storyData.TotalStories);
    }

    [Theory]
    [InlineData(-1, -1)]
    [InlineData(0, -1)]
    [InlineData(-1, 0)]
    [InlineData(6, 100)] // api returns 500 stories max.
    public async Task GetNewestStories_ShouldReturnBadRequest(int pageIndex, int pageSize)
    {
      // Arrange
      var client = Factory.CreateClient();

      // Act
      HttpResponseMessage response = await client.GetAsync($"api/stories?pageIndex={pageIndex}&pageSize={pageSize}");

      var storyData = JsonSerializer.Deserialize<StoryData>(await response.Content.ReadAsStringAsync());

      // Assert
      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      Assert.IsType<StoryData>(storyData);
      Assert.Empty(storyData.Stories);
      Assert.NotEmpty(storyData.Errors);
      Assert.Single(storyData.Errors);
      Assert.Equal(0, storyData.TotalStories);
    }
  }
}
