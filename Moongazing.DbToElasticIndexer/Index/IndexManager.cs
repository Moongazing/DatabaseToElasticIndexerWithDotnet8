using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moongazing.DbToElasticIndexer.Index;

public class IndexManager
{
    private readonly RestClient _restClient;
    private readonly string _indexName;
    private readonly ILogger<IndexManager> _logger;

    public IndexManager(string baseUrl, string indexName, ILogger<IndexManager> logger)
    {
        _indexName = indexName ?? throw new ArgumentNullException(nameof(indexName));
        _restClient = new RestClient(baseUrl);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task EnsureIndexExistsAsync()
    {
        var checkRequest = new RestRequest($"{_indexName}", Method.Head);
        var checkResponse = await _restClient.ExecuteAsync(checkRequest);

        if (!checkResponse.IsSuccessful)
        {
            var createRequest = new RestRequest($"{_indexName}", Method.Put);
            createRequest.AddHeader("Content-Type", "application/json");

            var indexSettings = new
            {
                settings = new
                {
                    number_of_shards = 1,
                    number_of_replicas = 1
                }
            };
            createRequest.AddJsonBody(JsonConvert.SerializeObject(indexSettings));

            var createResponse = await _restClient.ExecuteAsync(createRequest);
            if (!createResponse.IsSuccessful)
            {
                _logger.LogError($"Failed to create index '{_indexName}': {createResponse.ErrorMessage}");
                throw new Exception($"Failed to create index '{_indexName}': {createResponse.ErrorMessage}");
            }

            _logger.LogInformation($"Index '{_indexName}' created successfully.");
        }
        else
        {
            _logger.LogInformation($"Index '{_indexName}' already exists.");
        }
    }
}
