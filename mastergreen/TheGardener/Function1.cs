using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using TheGardener;
using Microsoft.Azure.Devices;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace TheGardener
{
    public static class Function1
    {
        public static List<SensorData> Devices { get; private set; }
        public static ServiceClient s_serviceClient = ServiceClient.CreateFromConnectionString("Azure-Iot hub-Shared acced policies-iothubowner-connectionstring-url");
        // Remember to change "AzureWebJobsStorage": "Azure-Storage_account-Access_keys-connection_string-url" in local.settings.json
        [FunctionName("Function1")]
        // Runs every 1 minute.
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            Devices = new List<SensorData>();
            var DeviceData = new DeviceDataProvider();
            var data = DeviceData.GetData(); // Calls on 5 new datapoints from ES
            var devices = data.Documents.GroupBy(e => e.deviceId); // group than per plant
            var avg = data.Documents.Average<SensorData>(a => a.SoilMoisture); //avoid anomlies
            // Each devise will be check for if they have less than 400 in Moisture.
            foreach (var device in devices)
            {

                Devices.Add(device.FirstOrDefault());
                if(avg > 400) //higher numer = more dry
                {
                    var methodInvocation = new CloudToDeviceMethod("fillWater") { ResponseTimeout = TimeSpan.FromSeconds(30) };
                    methodInvocation.SetPayloadJson("0");

                    // Invoke the direct method asynchronously and get the response from the simulated device.
                    var response = s_serviceClient.InvokeDeviceMethodAsync("Plant_1", methodInvocation);
                    Console.WriteLine("Response status: {0}, payload:", response.Status);
                    Counter("Plant_1", 1);
                }
            }
        }
        //Counts to 5 before sending the email function to say that water is empty.
        public static void Counter(string id,int i)
        {
            int count =+ i;
            if (count == 5)
            {
                Execute().Wait();
                count = 0;
            }
        }
        // Send the alert email via sendgrid
        public static async Task Execute()
        {
            string apiKey = ("azure-sengrid-manage-sendgrid dashboard-create-apiKey-url");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("samples@functions.com", "Azure Functions"),
                Subject = "Water Empty",
                PlainTextContent = "Please fill up water in the pott!",
                HtmlContent = "<strong>Water Empty</strong>"
            };
            msg.AddTo(new EmailAddress("testuser@example.com", "Test User"));
            var response = await client.SendEmailAsync(msg);
            Console.WriteLine("Response status: {0}, payload:", response.StatusCode);
        }
    }
}
