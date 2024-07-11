namespace hacker_news_wrapper.Models.Response
{
    public class HackerNewsResponse
    {
        public int StatusCode { get; set; }
        public object? Data { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
