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
        Size = 1
      };
    }

    [HttpGet]
    public async Task<IActionResult> GetNewestStories(int pageIndex, int pageSize, string? title = "")
    {
      var storyData = new StoryData();

      if (pageIndex < 0 || pageSize < 0)
      {
        storyData.Errors.Add("Page index and page size cannot be negative numbers.");
        return BadRequest(storyData);
      }

      HttpResponseMessage response = await client.GetAsync(newestStoriesBaseUrl);
      try
      {
        response.EnsureSuccessStatusCode();

        IEnumerable<int> storyIds = JsonSerializer.Deserialize<IEnumerable<int>>(await response.Content.ReadAsStringAsync());

        storyData = await BuildStoryDataFromIds(storyIds, pageIndex, pageSize, title);

        if (!IsPageIndexValid(pageIndex, pageSize, storyData.Stories.Count()))
        {
          storyData.Stories = new List<Story>();
          storyData.Errors.Add("Requested page index doesn't exist.");
          return BadRequest(storyData);
        }

        storyData = TransformStoryData(storyData, pageIndex, pageSize, title);

        return Ok(storyData);
      }
      catch (HttpRequestException ex)
      {
        // TO-DO: Add logging
        return StatusCode(StatusCodes.Status500InternalServerError, ex.ToString());
      }
    }

    private async Task<StoryData> BuildStoryDataFromIds(IEnumerable<int> storyIds, int pageIndex, int pageSize, string title)
    {
      var storyData = new StoryData();
   
      foreach (int id in storyIds)
      {
        Story story;
        if (!cache.TryGetValue(id, out story))
        {
          story = await GetStoryFromApi(id);
          cache.Set(id, story, cacheOptions);
        }

        if (IsValidStory(story))
        {
          storyData.Stories.Add(story);
        }
      }

      return storyData;
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

    private bool IsPageIndexValid(int pageIndex, int pageSize, int totalStoriesCount)
    {
      int totalPages = totalStoriesCount / pageSize;
      return pageIndex <= totalPages;
    }

    private StoryData TransformStoryData(StoryData storyData, int pageIndex, int pageSize, string? title = "")
    {
      var transformedStoryData = new StoryData();

      transformedStoryData.Stories = FilterStories(storyData.Stories, title);
      transformedStoryData.TotalStories = transformedStoryData.Stories.Count();
      transformedStoryData.Stories = transformedStoryData.Stories
      .Skip(pageIndex * pageSize)
      .Take(pageSize)
      .ToList();

      return transformedStoryData;
    }

    private List<Story> FilterStories(List<Story> stories, string title)
    {
      if (string.IsNullOrWhiteSpace(title))
      {
        return stories;
      }
      
      return stories = stories
      .Where(story => story.Title.Contains(title, StringComparison.InvariantCultureIgnoreCase))
      .ToList();
    }
  }
}
