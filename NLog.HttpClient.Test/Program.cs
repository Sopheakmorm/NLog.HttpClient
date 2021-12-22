using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog.HttpClient.Abstract;
using NLog.Targets;

namespace NLog.HttpClient.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var log = LogManager.GetLogger("test");
            while (true)
            {
                log.Debug(Console.ReadLine());
            }
        }
    }
}


namespace MyNamespace
{

    /// <summary>
    /// NLog message target for HttpClient.
    /// </summary>
    [Target("HttpClientImplementation")]
    public class NLogHttpClientConcrete : HttpClientAbstract
    {
        private static string Url = "https://localhost:5001/api/logs";
        private static string Auth = "your webservice auth key";
        private static System.Net.Http.HttpClient _client = new System.Net.Http.HttpClient();
        protected override async Task SendObjectAsync(object data)
        {
            var reqMsg = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            reqMsg.Headers.Add("Authorization", "Bearer " + Auth);
            await _client.SendAsync(reqMsg);
        }

        protected override async Task SendCollectionAsync(IEnumerable<object> data)
        {
            var reqMsg = new HttpRequestMessage(HttpMethod.Post, Url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };
            reqMsg.Headers.Add("Authorization", "Bearer " + Auth);
            await _client.SendAsync(reqMsg);
        }
    }
}