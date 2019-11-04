using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Azure.ServiceBus;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Web.Http;

namespace WebApplication1.Controllers
{
    public class AzureQueueSettings
    {
        public AzureQueueSettings(string connectionString, string queueName)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString");

            if (string.IsNullOrEmpty(queueName))
                throw new ArgumentNullException("queueName");

            this.ConnectionString = connectionString;
            this.QueueName = queueName;
        }

        public string ConnectionString { get; }
        public string QueueName { get; }
    }
    public class AzureQueueSender<T> : IAzureQueueSender<T> where T : class
    {
        public AzureQueueSender(AzureQueueSettings settings)
        {
            m_client = new QueueClient(
                settings.ConnectionString, settings.QueueName);
        }

        public async Task SendAsync(T item, Dictionary<string, object> properties)
        {
            var json = JsonConvert.SerializeObject(item);
            var message = new Message(Encoding.UTF8.GetBytes(json));

            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    message.UserProperties.Add(prop.Key, prop.Value);
                }
            }

            await m_client.SendAsync(message);
        }

        private readonly QueueClient m_client;

    }

    public interface IAzureQueueSender<in T>
    {
        Task SendAsync(T item, Dictionary<string, object> properties);
    }

    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TruckDataController : ApiController
    {
        private AzureQueueSettings _settings;
        public TruckDataController()
        {
            _settings = new AzureQueueSettings(
                connectionString: "Service Bus Connection String Redacted",
                queueName: "truckdata");
        }


        [HttpGet]
        public async Task<IHttpActionResult> GetPing()
        {
            return Ok(await Task.FromResult(GetType().Assembly.GetName().Version.ToString()));
        }
        [HttpGet]
        public async Task<IHttpActionResult> GetTruckData(string  customerId, string truckId)
        {
            TableQuery<TruckEntity> query = new TableQuery<TruckEntity>()
                .Where(TableQuery.GenerateFilterCondition(
                    "PartitionKey", QueryComparisons.Equal,
                    truckId))
                .Take(10);

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse
                    ("Azure Storage connection string redacted");
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable cloudTable = tableClient.GetTableReference(customerId);

            var allItems = new List<TruckEntity>();
            TableContinuationToken tableContinuationToken = null;
            do
            {
                var queryResponse = await cloudTable.ExecuteQuerySegmentedAsync<TruckEntity>(query, tableContinuationToken, null, null);
                tableContinuationToken = queryResponse.ContinuationToken;
                allItems.AddRange(queryResponse.Results);
            }
            while (tableContinuationToken != null && allItems.Count < 10);

            return Ok(allItems);
        }
        [HttpPost]
        public async Task<IHttpActionResult> InsertData([FromBody] TruckData truckData)
        {
            IAzureQueueSender<TruckData> sender =
                new AzureQueueSender<TruckData>(_settings);
            Dictionary<string, object> props = new Dictionary<string, object>();
            props["truckId"] = 4565;
            await sender.SendAsync(truckData, props);
            return Ok();
        }
    }

    public class TruckData
    {
        public TruckData(string customerId, string truckId, int longitude, int latitude, int temperature, int pressure, int speed, string driversMessage)
        {
            CustomerId = customerId;
            TruckId = truckId;
            Longitude = longitude;
            Latitude = latitude;
            Temperature = temperature;
            Pressure = pressure;
            Speed = speed;
            DriversMessage = driversMessage;
            TimeStamp = DateTimeOffset.UtcNow;
        }
        public string CustomerId { get; set; }
        public string TruckId { get; set; }
        public int Longitude { get; set; }
        public int Latitude { get; set; }
        public int Temperature { get; set; }
        public int Pressure { get; set; }
        public int Speed { get; set; }
        public string DriversMessage { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }
    public class TruckEntity : TableEntity
    {
        public TruckEntity() {}

        public string CustomerId { get; set; }
        public string TruckId { get; set; }
        public int Longitude { get; set; }
        public int Latitude { get; set; }
        public int Temperature { get; set; }
        public int Pressure { get; set; }
        public int Speed { get; set; }
        public string DriversMessage { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
    }

}
