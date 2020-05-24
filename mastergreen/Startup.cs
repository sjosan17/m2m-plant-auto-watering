using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetify;
using Microsoft.AspNetCore.SpaServices.Extensions;
using System.IO;
using mastergreen.server.services;

namespace mastergreen
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddDotNetify();
            services.AddSingleton<ISubscriptionService,SubscriptionService>();
            services.AddHostedService<SensorListenerService>();
            
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseWebSockets();
            app.UseDotNetify();
            // Not supported in the future
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new Microsoft.AspNetCore.SpaServices.Webpack.WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    HotModuleReplacementClientOptions = new Dictionary<string, string> { { "reload", "true" } },
                });
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapHub<DotNetifyHub>("/dotnetify"));

            app.Run(async (context) =>
            {
                using (var reader = new StreamReader(File.OpenRead("wwwroot/index.html")))
                    await context.Response.WriteAsync(reader.ReadToEnd());
            });
        }
    
    }
}
