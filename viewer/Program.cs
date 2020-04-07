using System;
using System.Text;
using System.Diagnostics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types

namespace viewer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();

            traceMessage("Before configuring queue");
            ConfigureQueue();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
                
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

                // Peek at the next message
                CloudQueueMessage peekedMessage = queue.PeekMessage();

                // Display message.
                traceMessage(peekedMessage.AsString);

                CloudQueueMessage message = queue.GetMessage();

                message.SetMessageContent2("Updated contents.", false);
                queue.UpdateMessage(message,
                        TimeSpan.FromSeconds(60.0),  // Make it invisible for another 60 seconds.
                        MessageUpdateFields.Content | MessageUpdateFields.Visibility);
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
