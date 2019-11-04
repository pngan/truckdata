# The Code Challenge

This document describes a prototype built to satisfy the requirements for the Truck Data Code Challenge. The prototype demonstrates working system that is capable of storing data generated from a fleet of trucks, and to retrieve them for display purposes.

The scenario assumed for this exercise is a fleet of trucks in active operation, and each truck is sending telemetry data to a central data warehouse for storage. The data sent to coretex data warehouse comprises the following values represented as follows:
```
{
    companyId:      int
    truckId:        int
    speed:          int
    latitude:       int
    longitude:      int
    temperature:    int
    pressure:       int
    driversMessage: string
}
```

For ease of implementation, numeric data is represented by integers only, but would be doubles in a real system.

## Web Tier

The demonstration sytem can be accessed at: https://truckstatic.z22.web.core.windows.net/  <span style="color:red">On first use, the table will not update for about 30s until the resources warm up. Refresh the page after 30s to see entered data.</span>

![image](https://user-images.githubusercontent.com/4557674/68081257-e1862280-fe6f-11e9-9660-862420c2d799.jpg)

The web tier is written in Angular, which was chosen because of my familiarity with the framework, but other frameworks would have been fine too, like React.js. 

This UI exists to enter and read the information for the purposes of the coding challenge. In a real system data would be sent from the trucks themselves and stored in a data warehouse for subsequent analysis and reporting.

The top section of the page supports a form into which the the data is entered.

The bottom section of the page shows a data grid of historic values. 

This Submit button triggers a call to the `POST api/truckdata` endpoint hosted in the Web API layer. The page then waits a few seconds and calls a `GET api/truckdata` endpoint to retrieve the 10 most recently stored truck data records, and displays these on the datagrid. A polling design was used for expediency, but in production a technology like SignalR would be more efficient.

In this demonstration system, the web page and associated assets are uploaded to the hosting site using the `ng deploy` command offered by Angular.

## Web API layer

The API consists of two end points:
- `POST api/truckdata` , which accepts a JSON payload of the telemetry data sent from the truck and writes this data into the data wareshouse. In this demonstration system the JSON also contains the CompanyId and TruckId. In a production system these could be provided the claims of a Javascript Web Token (JWT), which is obtained from an external authentication system. The authentication system is out of scope for this demonstration for simplicity. In a production system the API would be secured using JWT Bearer tokens.
- `GET api/truckdata` returns the 10 most recently stored data records for display on the web page.

The HTTP endpoints are served from an Azure App Service hosted on an Azure Service Plan. Azure App Services can be scaled up (uses larger VM's) and scaled out (uses more instances of VM's). This scaling can be triggered automatically based on load thresholds. Scaling allows App Services to cope with large spikes in load. It also allows the services to scale down during quiet periods to reduce costs. App Services also has close integration with Application Insights, a telemetry system that allows DevOps to close monitor request volumes, request latency, request failures, and drill into exceptions - all of which allows automated and manual monitoring of system health. App Services supports certificates to allow https traffic. It supports the use of HTTP/2 to allow faster and more compact transactions. App Services supports the notion of deployment slots, which allows testing on the real system without exposing staging systems to the real world. App Services also supports large scale load testing of endpoints.

## Data Processing Tier

### Azure Functions and Azure Service Bus Queues 

The App Service should not perform the processing itself, but delegate the processing to another component. Although App Services do have automatically scaling, it can be still caught out with sudden spikes in load.

The elastic processing capability in the solution is provided by Azure Functions. As many or as few function are run at the same time, again giving good trade-offs between access to capacity when the load is high, and cost-savings when the load is low. 

The primary mechanism to handle the spikey nature of requests is to use a processing queue. This provides a buffer to allow time for processing resources (Azure functions) to become available. Without a queuing mechanism the system could get overwhelmed and respond by returning Request Timeout responses, or in worst case simply not responding at all. 

Azure Service Bus was used to provide the queuing because it is well integrated into Azure Functions. Items in the Azure Serice Bus queue will trigger a new instance of Azure Functions.

Azure Service Bus Queues also integrate with Application Insights which can monitor queue lengths, and raise alerts on long queue lengths.

## Database Tier

The system uses Azure Table Storage to store the telemetry data. A No SQL approach was selected over SQL because it is quite possible that customers may have specific information unique to them that they would like to store and analysis. A SQL schema would be too inflexible to store varying data. The Azure table storage limit is 2 PetaBytes with ingress rates of 25 Gbps, and so offers ample capacity to ingest high data volumes expected to be transmitted the combined fleet. The data can be stored redundantly over multiple geographic locations to ensure high availability.

## Load Testing

There are several forms of loading testing that production systems should be tested with. These tests should be performed with a system as close as possibile to the production system in terms of computing and storage specification, and loaded with production data. These tests are generally performed against the API.

- Expected load testing : Subject the system with a load close to normal operating volumes - number of trucks and transmission rates. Monitor the system using Application Insights for failure rates and request latency.
- Spike testing: Conjecture abnormal scenarios that might lead to spike traffic patterns. 
- Soak testing: Run the system for long periods of time and watch for long run resource usage that might indicate resource leakages.

App Services has API load tools built into them. These are a very easy way of generated high request rates. Another tool would be to use SOAP UI which can generate rich representative usage patterns. Up until recently I would have recommended using the Visual Studio Load test tool, but this tool will no longer be available after VS 2019.


## Production Technology Choices

This demonstration system was built with limited scope for the sake of brevity. When building a production system, the general architecture of queuing and elastic computing may be retained, there are other technologies that should be explored.

In front of the App Services, the API Managment resource can be used. This offers useful features such as traffic shaping and call volume based billing.

Consider using Event Hubs instead of Service Bus. Event Hubs are lighter weight than Service Bus and capable of higher throughput.

Use CosmosDB instead of Table storage. While Table has high ingress and egress rates, the richness of the queries is limited, but much more expansive in CosmosDB. The drawback of CosmosDB is that is expensive - on par with Azure SQL.

Alternatives to Azure Functions are orchestrated containerized systems such as Azure Service Fabric or Azure Kubernetes - these two are comparable to each other, although Kubernetes has a much wider industry adoption. Azure Functions are more elastic than the other systems but have a higher latency. However because the demonstration system is built around a process queue processing latency is not problem and so would be better suited than the other technologies which are less elastic.




