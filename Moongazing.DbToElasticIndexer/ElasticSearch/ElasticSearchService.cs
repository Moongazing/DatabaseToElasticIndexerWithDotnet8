using Microsoft.Extensions.Logging;
using Moongazing.DbToElasticIndexer.Index;
using Newtonsoft.Json;
using RestSharp;

namespace Moongazing.DbToElasticIndexer.ElasticSearch;

public class ElasticSearchService : IElasticSearchService
{
    private readonly RestClient _restClient;
    private readonly string _indexName;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly IndexManager _indexManager;

    public ElasticSearchService(string baseUrl,
                                string indexName, 
                                IndexManager indexManager, 
                                ILogger<ElasticSearchService> logger)
    {
        _indexName = indexName ?? throw new ArgumentNullException(nameof(indexName));
        _restClient = new RestClient(baseUrl);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _indexManager = indexManager ?? throw new ArgumentNullException(nameof(indexManager));
    }

    public async Task<bool> SendDataAsync(object data)
    {
        await _indexManager.EnsureIndexExistsAsync();

        var request = new RestRequest($"{_indexName}/_doc", Method.Post);
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(JsonConvert.SerializeObject(data));

        var response = await _restClient.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            _logger.LogError($"Failed to send data to Elasticsearch: {response.ErrorMessage}");
            return false;
        }

        _logger.LogInformation("Data sent successfully to Elasticsearch.");
        return true;
    }
}
