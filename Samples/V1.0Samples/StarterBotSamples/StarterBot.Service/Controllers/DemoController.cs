// <copyright file="DemoController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using StarterBot.Model.Constants;
using StarterBot.Service.Settings;
using StarterBot.Services.Bot;
using StarterBot.Services.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Communications.Common.Telemetry;
using Microsoft.Graph.Communications.Core.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace StarterBot.Services.Controllers
{
    /// <summary>
    /// DemoController serves as the gateway to explore the bot.
    /// From here you can get a list of calls, and functions for each call.
    /// </summary>
    public class DemoController : ControllerBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly IGraphLogger _logger;
        /// <summary>
        /// The bot service
        /// </summary>
        private readonly IBotService _botService;
        /// <summary>
        /// The settings
        /// </summary>
        private readonly AzureSettings _settings;

        private readonly InMemoryObserver _observer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DemoController" /> class.

        /// </summary>
        public DemoController(IBotService botService, IOptions<AzureSettings> settings, IGraphLogger logger, InMemoryObserver observer)
        {
            _logger = logger;
            _botService = botService;
            _settings = settings.Value;
            _observer = observer;
        }

        /// <summary>
        /// The GET calls.
        /// </summary>
        /// <returns>The <see cref="Task" />.</returns>
        [HttpGet]
        [Route(HttpRouteConstants.Calls + "/")]
        public HttpResponseMessage OnGetCalls()
        {
            _logger.Info("Getting calls");

            if (_botService.CallHandlers.IsEmpty)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent);
            }

            var calls = new List<Dictionary<string, string>>();
            foreach (var callHandler in _botService.CallHandlers.Values)
            {
                var call = callHandler.Call;
                var callPath = "/" + HttpRouteConstants.CallRoute.Replace("{callLegId}", call.Id);
                var callUri = new Uri(_settings.CallControlBaseUrl, callPath).AbsoluteUri;
                var values = new Dictionary<string, string>
                {
                    { "legId", call.Id },
                    { "scenarioId", call.ScenarioId.ToString() },
                    { "call", callUri },
                    { "logs", callUri.Replace("/calls/", "/logs/") },
                };
                calls.Add(values);
            }

            var serializer = new CommsSerializer(pretty: true);
            var json = serializer.SerializeObject(calls);
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return response;
        }

        /// <summary>
        /// End the call.
        /// </summary>
        /// <param name="callLegId">Id of the call to end.</param>
        /// <returns>The <see cref="HttpResponseMessage" />.</returns>
        [HttpDelete]
        [Route(HttpRouteConstants.CallRoute)]
        public async Task<HttpResponseMessage> OnEndCallAsync(string callLegId)
        {
            var message = $"Ending call {callLegId}";
            _logger.Info(message);
            
            try
            {
                await _botService.EndCallByCallLegIdAsync(callLegId).ConfigureAwait(false);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                response.Content = new StringContent(e.ToString());
                return response;
            }
        }

        /// <summary>
        /// The GET logs.
        /// </summary>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        [HttpGet]
        [Route(HttpRouteConstants.Logs + "/")]
        public ContentResult OnGetLogs(
            int skip = 0,
            int take = 1000)
        {
            var logs = this._observer.GetLogs(skip, take);

            return Content(logs);
        }

        /// <summary>
        /// The GET logs.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="take">The take.</param>
        /// <returns>
        /// The <see cref="Task" />.
        /// </returns>
        [HttpGet]
        [Route(HttpRouteConstants.Logs + "/{filter}")]
        public ContentResult OnGetLogs(
            string filter,
            int skip = 0,
            int take = 1000)
        {
            var logs = this._observer.GetLogs(filter, skip, take);

            return Content(logs);
        }
    }
}
