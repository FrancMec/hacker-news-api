namespace hacker_news_wrapper
{
    using Implementation;
    using Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtension
    {
        public static void AddServiceProvider(this IServiceCollection services)
        {

            services.AddHttpClient("hackerNews", (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
            });
            services.AddScoped<IHackerNewsService, HackerNewsService>();
        }
    }
}
