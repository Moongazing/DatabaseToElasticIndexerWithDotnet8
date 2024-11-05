using Microsoft.Extensions.Configuration;

namespace Moongazing.DbToElasticIndexer;

public class AppConfiguration
{
    public string ElasticBaseUrl { get; }
    public string IndexName { get; }
    public string DbConnectionString { get; }

    public AppConfiguration(IConfiguration configuration)
    {
        ElasticBaseUrl = configuration["Elasticsearch:BaseUrl"] ?? throw new ArgumentNullException("Elasticsearch BaseUrl is missing");
        IndexName = configuration["Elasticsearch:IndexName"] ?? throw new ArgumentNullException("Elasticsearch IndexName is missing");
        DbConnectionString = configuration.GetConnectionString("DbConnection") ?? throw new ArgumentNullException("Database connection string is missing");
    }
}
