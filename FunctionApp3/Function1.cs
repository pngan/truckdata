using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace FunctionApp3
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([ServiceBusTrigger("truckdata", AccessRights.Manage,
                Connection = "SBConnection")]
            string queueItem, TraceWriter log)
        {
            log.Info($"C# ServiceBus queue trigger function about to process message: {queueItem}");

            var truckData = JsonConvert.DeserializeObject<TruckData>(queueItem);

            CloudStorageAccount cloudStorageAccount =
                CloudStorageAccount.Parse
                    ("Azure storage account connection string redacted");
            CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
            CloudTable cloudTable = tableClient.GetTableReference(truckData.CustomerId);

            TableOperation tableOperation = TableOperation.Insert(truckData);

            cloudTable.Execute(tableOperation);

            log.Info($"C# ServiceBus queue trigger function completed processing message: {queueItem}");

            // The name of the database and container we will create
        }


        public class TruckData : TableEntity
        {
            public TruckData(string customerId, string truckId, int longitude, int latitude, int temperature, int pressure, int speed, string driversMessage, DateTimeOffset timeStamp)
            {
                CustomerId = customerId;
                TruckId = truckId;
                Longitude = longitude;
                Latitude = latitude;
                Temperature = temperature;
                Pressure = pressure;
                Speed = speed;
                DriversMessage = driversMessage;

                PartitionKey = truckId;
                RowKey = string.Format("{0:D19}", DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks);
                Timestamp = timeStamp;
            }
            public string CustomerId { get; set; }
            public string TruckId { get; set; }
            public int Longitude { get; set; }
            public int Latitude { get; set; }
            public int Temperature { get; set; }
            public int Pressure { get; set; }
            public int Speed { get; set; }
            public string DriversMessage { get; set; }
        }

    }
}
