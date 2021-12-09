using System.Text.Json;
using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HackerNews.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class StoriesController : ControllerBase
  {
    private static HttpClient client;
    private static IMemoryCache cache;
    private static MemoryCacheEntryOptions cacheOptions;

    private static readonly string newestStoriesBaseUrl = "https://hacker-news.firebaseio.com/v0/topstories.json";
    private static readonly string storyItemBaseUrl = "https://hacker-news.firebaseio.com/v0/item/";

    public StoriesController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
      client = httpClientFactory.CreateClient();
      cache = memoryCache;

      cacheOptions = new MemoryCacheEntryOptions()
      {
        AbsoluteExpiration = DateTime.Now.AddMinutes(10),
        Size = 1024
      };
    }

    [HttpGet]
    public async Task<IActionResult> GetNewestStories(int pageIndex, int pageSize, string? title = "")
    {
      HttpResponseMessage response = await client.GetAsync(newestStoriesBaseUrl);
      try
      {
        response.EnsureSuccessStatusCode();

        IEnumerable<int> storyIds = JsonSerializer.Deserialize<IEnumerable<int>>(await response.Content.ReadAsStringAsync());

        StoryData storyData = await BuildStoryDataFromIds(storyIds, pageIndex, pageSize, title);

        return Ok(storyData);
      }
      catch (HttpRequestException ex)
      {
        // TO-DO: Add logging
        return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
      }
      finally
      {
        response.Dispose();
      }
    }

    private async Task<StoryData> BuildStoryDataFromIds(IEnumerable<int> storyIds, int pageIndex, int pageSize, string title)
    {
      var storyData = new StoryData();

      if (storyIds.Any())
      {        
        foreach (var id in storyIds)
        {
          Story story = null;
          if (!cache.TryGetValue(id, out story))
          {
            story = await GetStoryFromApi(id);

            if (IsValidStory(story))
            {
              cache.Set(id, story, cacheOptions);
            }
          }

          storyData.stories.Add(story);
        }
      }

      if (!string.IsNullOrWhiteSpace(title))
      {
        storyData.stories = FilterStories(storyData.stories, title);
      }

      storyData = TransformStoryData(storyData, pageIndex, pageSize);

      return storyData;
    }

    private StoryData TransformStoryData(StoryData storyData, int pageIndex, int pageSize)
    {
      var transformedStoryData = new StoryData();

      transformedStoryData.totalStories = storyData.stories.Count();

      transformedStoryData.stories = storyData.stories
      .Skip(pageIndex * pageSize)
      .Take(pageSize)
      .ToList();

      return transformedStoryData;
    }

    private async Task<Story> GetStoryFromApi(int id)
    {
      HttpResponseMessage response = await client.GetAsync($"{storyItemBaseUrl}{id}.json");
      response.EnsureSuccessStatusCode();

      Story story = JsonSerializer.Deserialize<Story>(await response.Content.ReadAsStringAsync());

      return story;
    }

    private bool IsValidStory(Story story)
    {
      if (string.IsNullOrWhiteSpace(story.Title) || string.IsNullOrWhiteSpace(story.Url))
      {
        return false;
      }

      return true;
    }

    private List<Story> FilterStories(List<Story> stories, string title)
    {
      return stories = stories
      .Where(story => story.Title.Contains(title, StringComparison.InvariantCultureIgnoreCase))
      .ToList();
    }
  }
}
