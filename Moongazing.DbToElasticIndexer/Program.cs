using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moongazing.DbToElasticIndexer.Data;
using Moongazing.DbToElasticIndexer.ElasticSearch;
using Moongazing.DbToElasticIndexer.Index;

namespace Moongazing.DbToElasticIndexer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();
            var elasticService = serviceProvider.GetRequiredService<IElasticSearchService>();

            Console.WriteLine("Starting data sync...");

            var dataToSync = await FetchDataAsync(serviceProvider);
            foreach (var data in dataToSync)
            {
                await elasticService.SendDataAsync(data);
            }

            Console.WriteLine("Data sync completed.");
        }

        private static ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();


            var services = new ServiceCollection();
            services.AddLogging(config => config.AddConsole())
                    .AddSingleton(configuration)
                    .AddSingleton<AppConfiguration>()
                    .AddDbContext<MyDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DbConnection")));

            var appConfig = new AppConfiguration(configuration);

            services.AddSingleton(new IndexManager(appConfig.ElasticBaseUrl, appConfig.IndexName, services.BuildServiceProvider().GetRequiredService<ILogger<IndexManager>>()));
            services.AddSingleton<IElasticSearchService, ElasticSearchService>(sp =>
            {
                var indexManager = sp.GetRequiredService<IndexManager>();
                var logger = sp.GetRequiredService<ILogger<ElasticSearchService>>();
                return new ElasticSearchService(appConfig.ElasticBaseUrl, appConfig.IndexName, indexManager, logger);
            });

            return services.BuildServiceProvider();
        }

        private static async Task<IEnumerable<object>> FetchDataAsync(ServiceProvider serviceProvider)
        {
            using var dbContext = serviceProvider.GetRequiredService<MyDbContext>();
            return await dbContext.Product.AsNoTracking().ToListAsync();
        }
    }
}
