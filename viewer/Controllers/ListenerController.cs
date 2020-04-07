using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;
using viewer.Hubs;
using viewer.Models;

using System.Text;
using System.Diagnostics;
using System.Threading;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types



namespace viewer.Controllers
{
    public class ListenerController : Controller
    {
        private readonly IHubContext<GridEventsHub> _hubContext;

        public ListenerController(IHubContext<GridEventsHub> gridEventsHubContext)
        {
            this._hubContext = gridEventsHubContext;
            
            traceMessage("Before configuring queue");
            ConfigureQueue();
        }

        [HttpOptions]
        public async Task<IActionResult> Options()
        {
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();
                var webhookRequestCallback = HttpContext.Request.Headers["WebHook-Request-Callback"];
                var webhookRequestRate = HttpContext.Request.Headers["WebHook-Request-Rate"];
                HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
                HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);
            }

            return Ok();
        }

        [HttpGet]
        public string Get()
        {
            return "true";
        }

        private static void ConfigureQueue()
        {
            traceMessage("Connecting to queue");

            try
            {
                // Retrieve storage account from connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=stgaccprivateendpoint;AccountKey=YGQ7LP81FSOZnQm3RNxv6UeAI+O+EEHyNQpTem0Rh/83N/q6T4ZSki4RQbaS8Mv+judJiUs9Z69pIls0BnCZeg==;EndpointSuffix=core.windows.net");

                // Create the queue client.
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                // Retrieve a reference to a container.
                CloudQueue queue = queueClient.GetQueueReference("blobupload");

                traceMessage("Adquired queue");

                // Create the queue if it doesn't already exist
                queue.CreateIfNotExists();

                int currentBackoff = 0;
                int maximumBackoff = 10;

                while (true)
                {
                    var message = queue.GetMessage();
                    if (message != null)
                    {
                        // Reset backoff
                        currentBackoff = 0;

                        // Process the message
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(
                        "Processed message with contents: {0}", message.AsString);
                        // Mark completed
                        queue.DeleteMessage(message);
                    }
                    else
                    {
                        if (currentBackoff < maximumBackoff)
                        {
                            currentBackoff++;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("Backing off for {0} seconds...", currentBackoff);
                        Thread.Sleep(TimeSpan.FromSeconds(currentBackoff));
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                Console.WriteLine(e);
            }

            
        }

        private static void traceMessage(string message) {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }
    }
}
