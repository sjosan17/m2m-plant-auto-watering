using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mastergreen.server.database
{
    /// <summary>
    /// Get ElasticSearch data from the Azure VM server and sorts the data based on time.
    /// </summary>
    public class DeviceDataProvider
    {
        public ISearchResponse<SensorData> GetData()
        {
            string connectionServer = "http://YOURSERVER:9200";
            var elasticConnection = new Uri(connectionServer);
            var client = new ElasticClient(elasticConnection);

            //var request = 
            var data = client
                .Search<SensorData>(f => f
                .Index("azure*")
                .Take(100)
                .Query(e => e
                    .MatchAll())
                .Sort(d => d
                    .Descending(m => m.timestamp)));
            return data;
        }

    }
    public class SensorData
    {
        [Date(Name = "@timestamp")]
        public DateTime timestamp { get; set; }
        public string message { get; set; }
        [Number(Name = "SoilMoisture")]
        public float SoilMoisture { get; set; }
        [Number(Name = "Temperature")]
        public float Temperature { get; set; }
        [Number(Name = "Humidity")]
        public float Humidity { get; set; }
        [Number(Name = "Light")]
        public float Light { get; set; }
        [Text]
        public string deviceId { get; set; }

    }
}
