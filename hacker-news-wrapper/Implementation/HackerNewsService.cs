namespace hacker_news_wrapper.Implementation
{
    using hacker_news_wrapper.Constants;
    using Interfaces;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Models.Response;
    using System.Collections.Generic;
    using System.Net.Http.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public class HackerNewsService : IHackerNewsService
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _cache;
        public HackerNewsService(IHttpClientFactory factory, IMemoryCache cache)
        {
            _client = factory.CreateClient("hackerNews");
            _cache = cache;
        }

        /// <summary>
        /// Get stories from hackerNews api.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>HackerNewsResponse</returns>
        public async Task<HackerNewsResponse> GetStoriesAsync(CancellationToken cancellationToken)
        {

            if (!_cache.TryGetValue(ServiceConstants.STORIES_CACHEKEY, out HackerNewsResponse response))
            {
                try
                {
                    response = new HackerNewsResponse();
                    var storyIds = await GetStoryIdsAsync(cancellationToken);

                    var stories = new List<StoryResponse>();

                    if (storyIds?.Any() == true)
                    {
                        var tasks = storyIds.Select(async id =>
                        {
                            var story = await GetStoryByIdAsync(id, cancellationToken);
                            if (story != null && story.Type.Equals("story", StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrWhiteSpace(story.Url))
                            {
                                return story;
                            }
                            return null;
                        });

                        var results = await Task.WhenAll(tasks);
                        stories.AddRange(results.Where(story => story != null));

                        response.StatusCode = StatusCodes.Status200OK;
                        response.Data = stories;
                        CacheResponse(response, ServiceConstants.STORIES_CACHEKEY);

                    }
                    else
                    {
                        // Clear cache when no data.
                        _cache.Dispose();
                        response.StatusCode = StatusCodes.Status204NoContent;
                    }
                }
                catch (Exception ex)
                {
                    response.StatusCode = StatusCodes.Status500InternalServerError;
                    response.ErrorMessage = ex.Message;
                }
            }
            return response;
        }

        /// <summary>
        /// Get story by storyId.
        /// </summary>
        /// <param name="id">The storyId</param>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>Story object.</returns>
        public async Task<StoryResponse> GetStoryByIdAsync(int id, CancellationToken cancellationToken)
        {

            var story = await _client.GetFromJsonAsync<StoryResponse>($"/v0/item/{id}.json", cancellationToken);
            return story;
        }

        /// <summary>
        /// Get new storyIds.
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken.</param>
        /// <returns>list of int ids.</returns>
        public async Task<List<int>> GetStoryIdsAsync(CancellationToken cancellationToken)
        {
            var storyIds = await _client.GetFromJsonAsync<List<int>>($"/v0/newstories.json?orderBy=%22$priority%22&limitToFirst=200", cancellationToken);
            return storyIds;
        }

        private void CacheResponse(HackerNewsResponse response, string key)
        {
            // Cache response when we have data.
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(ServiceConstants.CACHING_DURATION))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(ServiceConstants.CACHING_DURATION))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);
            _cache.Set(key, response, cacheEntryOptions);
        }
    }
}
