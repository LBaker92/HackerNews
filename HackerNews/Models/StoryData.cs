namespace HackerNews.Models
{
  public class StoryData
  {
    public List<Story> Stories { get; set; } = new List<Story>();
    public List<string> Errors { get; set; } = new List<string>();
    public int TotalStories { get; set; }
  }
}