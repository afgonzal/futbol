using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;

namespace Futbol.Seasons.DataRepository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly Table _table;
        protected readonly DynamoDBContext _context;
        protected Repository(IConfiguration config, string tableName)
        {
            var dynamoConfig = config.GetSection("DynamoDb");
            var awsConfig = config.GetSection("Aws:Dynamo");
            var runLocal = Convert.ToBoolean(dynamoConfig["LocalMode"]);
            IAmazonDynamoDB client;
            if (!runLocal)
            {
                var credentials = new BasicAWSCredentials(awsConfig["AWSAccessKey"], awsConfig["AWSSecretKey"]);
                client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USEast1);
            }
            else
            {

                var ddbConfig = new AmazonDynamoDBConfig
                {
                    RegionEndpoint = RegionEndpoint.SAEast1,
                    ServiceURL = dynamoConfig["LocalServiceUrl"]
                };
                //takes credentials from environment/configfile
                client = new AmazonDynamoDBClient(ddbConfig);
            }
            _context = new DynamoDBContext(client, new DynamoDBContextConfig { TableNamePrefix = dynamoConfig["TableNamePrefix"] });
            _table = Table.LoadTable(client, tableName);
        }

        public Task<List<TEntity>> QueryById(object id)
        {
            return _context.QueryAsync<TEntity>(id).GetRemainingAsync();
        }

        public Task<List<TEntity>> GetAllAsync()
        {
            return _context.ScanAsync<TEntity>(new List<ScanCondition>()).GetRemainingAsync();
        }


        public Task<TEntity> GetByKeyAsync(object key)
        {
            return _context.LoadAsync<TEntity>(key);
        }

        public Task<TEntity> GetByKeyAsync(object hashKey, object sortKey)
        {
            return _context.LoadAsync<TEntity>(hashKey, sortKey);
        }

        protected virtual async Task<object> GetLastSortKey(string hashKeyFieldName, string hashKeyValue, string sortKeyFieldName)
        {
            var queryConfig = new QueryOperationConfig()
            {
                BackwardSearch = true,
                Limit = 1,
                Filter = new QueryFilter(hashKeyFieldName, QueryOperator.Equal, hashKeyValue)
            };
            var results = await _context.FromQueryAsync<TEntity>(queryConfig).GetNextSetAsync();
            var lastItem = results.FirstOrDefault();
            if (lastItem == null)
                return null;
            var keyProperty = lastItem.GetType().GetProperty(sortKeyFieldName);
            if (keyProperty == null)
                throw new ArgumentException($"Sort key name ({sortKeyFieldName}) is invalid for entity {lastItem.GetType().ToString()}");
            return keyProperty.GetValue(lastItem, null);
        }

        protected Task<List<TEntity>> QueryByKeysAsync(object hashKey, IEnumerable<object> queryKeys, QueryOperator queryOperator)
        {
            var query = _context.QueryAsync<TEntity>(hashKey, queryOperator, queryKeys);
            return query.GetRemainingAsync();
        }

        public virtual Task AddAsync(TEntity entity)
        {
            return _context.SaveAsync(entity);
        }

        public Task UpdateAsync(TEntity entity)
        {
            return _context.SaveAsync(entity);
        }

        public Task DeleteAsync(object hashKey)
        {
            return _context.DeleteAsync<TEntity>(hashKey);
        }

        public Task DeleteAsync(object hashKey, object sortKey)
        {
            return _context.DeleteAsync<TEntity>(hashKey, sortKey);
        }



        public Task<List<TEntity>> QueryBetweenKeysAsync(object hashKey, object sortKeyFrom, object sortKeyTo)
        {
            var result = _context.QueryAsync<TEntity>(hashKey, QueryOperator.Between, new object[] { sortKeyFrom, sortKeyTo });

            return result.GetRemainingAsync();
        }

        public Task BatchAddAsync(IEnumerable<TEntity> entities)
        {
            var batch = _context.CreateBatchWrite<TEntity>();
            batch.AddPutItems(entities);
            return batch.ExecuteAsync();
        }
    }
}