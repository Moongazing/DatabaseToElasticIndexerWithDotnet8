using System;
using System.Threading.Tasks;

namespace Moongazing.DbToElasticIndexer.ElasticSearch;

public interface IElasticSearchService
{
    Task<bool> SendDataAsync(object data);
}