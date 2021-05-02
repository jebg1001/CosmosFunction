using System.Text;
using System.Net;
using System.Configuration;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Cosmos;
using System;
using Microsoft.Extensions.Logging;

namespace CosmosFunctionsApp
{

        public class Espacio
        {
            public string id { get; set; }

            public string espacio { get; set; }

            public string status { get; set; }
        }

        public static class CosmosFunction
        {

            [FunctionName("CosmosFunction")]
            public static async Task Run(
                [EventHubTrigger("twins-event-hub", Connection = "EventHubAppSetting-Twins")] EventData myEventHubMessage, 
                ILogger log)
            {
                CosmosClient cosmosClient= new CosmosClient("https://dt-usmp-db.documents.azure.com:443/","0F3GWamAF8GbBChDZQAeUiMZQsnu1SNQhrpVAoLGothZsyjCvaCPkMofoE607hzyeiHacmqnh0BJOJiMBNT13Q==");
                Microsoft.Azure.Cosmos.Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync("DigitalTwins");
                Container container = await database.CreateContainerIfNotExistsAsync("Espacios", "/espacio");

                JObject message = (JObject)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(myEventHubMessage.Body));
                log.LogInformation("Reading event:" + message.ToString());

                var item= new Espacio();

                foreach (var operation in message["patch"]) {

                    switch (operation["path"].ToString()) {
                        case "/SpotID": item.id= Guid.NewGuid().ToString(); 
                                        item.espacio= operation["value"].ToString(); break;
                        case "/ParkingSpotStatus": item.status="Ocupado"; break;

                        default: break;
                    }
                }
                
                try{
                    var response= await container.CreateItemAsync<Espacio>(item);
                }
                catch (CosmosException cosmosException)
                {
                    log.LogError("Creating item failed with error {0}", cosmosException.ToString());
                }
            }

        }
}