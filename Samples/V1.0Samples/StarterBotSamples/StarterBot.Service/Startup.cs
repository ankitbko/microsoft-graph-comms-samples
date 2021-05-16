// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Communications.Common.Telemetry;
using StarterBot.Service.Settings;
using StarterBot.Services.Bot;
using StarterBot.Services.Logging;

namespace StarterBot.Services
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddSingleton<IGraphLogger, GraphLogger>(_ => new GraphLogger("StarterBot", redirectToTrace: true));
            services.AddSingleton<InMemoryObserver, InMemoryObserver>();
            services.Configure<AzureSettings>(Configuration.GetSection(nameof(AzureSettings)));
            services.PostConfigure<AzureSettings>(az => az.Initialize());
            services.AddSingleton<IBotService, BotService>(provider =>
            {
                var bot = new BotService(
                    provider.GetRequiredService<IGraphLogger>(),
                    provider.GetRequiredService<IOptions<AzureSettings>>());
                bot.Initialize();
                return bot;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            //app.UseExceptionHandler();
            app.UseMvc();
        }
    }
}
