
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.DynamoDBEvents;
using Microsoft.Extensions.DependencyInjection;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Futbol.Seasons.StatsStream
{
    public class Function
    {
        private readonly ITeamsStatsAggregationService _statsAggregation;

        public Function()
        {
            var serviceProvider = Startup.ConfigureServices().BuildServiceProvider();
            _statsAggregation = serviceProvider.GetService<ITeamsStatsAggregationService>();
        }
        public void FunctionHandler(DynamoDBEvent dynamoEvent, ILambdaContext context)
        {
            context.Logger.LogLine($"Beginning to process {dynamoEvent.Records.Count} records...");

            foreach (var record in dynamoEvent.Records)
            {
                var source = record.EventSourceArn.Split(':')[5].Split('/')[1];

                

                //context.Logger.LogLine($"Event ID: {record.EventID}");
                //context.Logger.LogLine($"Event Name: {record.EventName}");
                //context.Logger.LogLine($"Event Table: {source}");
                
                switch (source)
                {
                    case "Matches":
                        var tf = new TaskFactory(CancellationToken.None, TaskCreationOptions.None,
                            TaskContinuationOptions.None,
                            TaskScheduler.Default);
                        tf.StartNew(() => _statsAggregation.ProcessStreamRecordAsync(record, context))
                            .Unwrap().GetAwaiter().GetResult();

                        break;
                    default:
                        context.Logger.LogLine($"No action for table: {source}");
                        break;
                }
            }
            

            context.Logger.LogLine("Stream processing complete.");
        }
    }
}