﻿// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using StarterBot.Service.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace StarterBot.Services
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                //.UseKestrel();
                .UseKestrel((ctx, opt) =>
                {
                    var az = new AzureSettings();
                    ctx.Configuration.GetSection("AzureSettings").Bind(az);
                    az.Initialize();
                    opt.Configure()
                        .Endpoint("HTTPS", listenOptions =>
                        {
                            listenOptions.HttpsOptions.SslProtocols = SslProtocols.Tls12;
                        });
                    opt.ListenAnyIP(az.CallSignalingPort, o => o.UseHttps());
                    opt.ListenAnyIP(az.CallSignalingPort + 1);
                });
    }
}
