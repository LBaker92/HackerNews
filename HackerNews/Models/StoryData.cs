using System.Text.Json.Serialization;

namespace HackerNews.Models
{
  public class StoryData
  {
    [JsonPropertyName("stories")]
    public List<Story> Stories { get; set; } = new List<Story>();

    [JsonPropertyName("errors")]
    public List<string> Errors { get; set; } = new List<string>();

    [JsonPropertyName("totalStories")]
    public int TotalStories { get; set; }
  }
}