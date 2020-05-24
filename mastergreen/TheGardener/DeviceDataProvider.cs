using System;
using System.Collections.Generic;
using System.Text;
using Nest;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace TheGardener
{
    /// <summary>
    /// Same code as in mastergreen to get ElasticSearch data.
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
                .Take(5)
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
