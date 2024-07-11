namespace hacker_news_wrapper.Interfaces
{
    using Models.Response;

    public interface IHackerNewsService
    {
        Task<HackerNewsResponse> GetStoriesAsync(CancellationToken cancellationToken);
        Task<List<int>> GetStoryIdsAsync(CancellationToken cancellationToken);
        Task<StoryResponse> GetStoryByIdAsync(int id, CancellationToken cancellationToken);
    }
}
