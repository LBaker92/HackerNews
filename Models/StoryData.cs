namespace HackerNews.Models
{
  public class StoryData
  {
    public List<Story> stories { get; set; } = new List<Story>();
    public int totalStories { get; set; }
  }
}