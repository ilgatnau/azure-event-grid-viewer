using System;
using System.Text;
using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using viewer.Hubs;

using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types

namespace viewer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Awwww yeah!
            services.AddSignalR();


            //traceMessage("Before configuring queue");
            //ConfigureQueue();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Add SignalR hub routes
            app.UseSignalR(routes =>
            {
                routes.MapHub<GridEventsHub>("/hubs/gridevents");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void ConfigureQueue()
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
                queue.DeleteMessage(message);

                //message.SetMessageContent2("Updated contents.", false);
                //queue.UpdateMessage(message,
                //        TimeSpan.FromSeconds(60.0),  // Make it invisible for another 60 seconds.
                //        MessageUpdateFields.Content | MessageUpdateFields.Visibility);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
                Console.WriteLine(e);
            }

            
        }

        private void traceMessage(string message) {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }
    }
}
