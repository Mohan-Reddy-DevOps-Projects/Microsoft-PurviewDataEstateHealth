﻿// <copyright file="ScheduleServiceClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Schedule
{
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Purview.DataEstateHealth.DHModels.Services.Control.Schedule;
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class ScheduleServiceClient : ServiceClient<ScheduleServiceClient>
    {
        private readonly Uri BaseUri;

        private readonly IDataEstateHealthRequestLogger Logger;

        private readonly HttpClient Client;

        public ScheduleServiceClient(HttpClient httpClient, Uri baseUri, IDataEstateHealthRequestLogger logger)
        {
            this.Client = httpClient;
            this.BaseUri = baseUri;
            this.Logger = logger;
        }

        public async Task<DHScheduleUpsertResponse> CreateSchedule(DHScheduleCreateRequestPayload schedule)
        {
            var requestUri = this.CreateRequestUri("/schedules");
            var content = this.CreateRequestContent(schedule);
            var response = await this.Client.PostAsync(requestUri, content).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
            return await this.ParseResponse<DHScheduleUpsertResponse>(response).ConfigureAwait(false);
        }

        public async Task<DHScheduleUpsertResponse> UpdateSchedule(DHScheduleCreateRequestPayload schedule)
        {
            var requestUri = this.CreateRequestUri($"/schedules");
            var content = this.CreateRequestContent(schedule);
            var response = await this.Client.PutAsync(requestUri, content).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
            return await this.ParseResponse<DHScheduleUpsertResponse>(response).ConfigureAwait(false);
        }

        public async Task DeleteSchedule(string scheduleId)
        {
            var requestUri = this.CreateRequestUri($"/schedules");
            var payload = new Dictionary<string, string>()
            {
                { "scheduleId", scheduleId },
                { "category", DHScheduleConstant.Category }
            };
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            request.Content = this.CreateRequestContent(payload);
            var response = await this.Client.SendAsync(request).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
        }

        public async Task TriggerSchedule(DHScheduleTriggerRequestPayload schedule)
        {
            var requestUri = this.CreateRequestUri($"/schedules/trigger");
            var content = this.CreateRequestContent(schedule);
            var response = await this.Client.PostAsync(requestUri, content).ConfigureAwait(false);
            this.HandleResponseStatusCode(response);
        }

        private Uri CreateRequestUri(string pathname)
        {
            var builder = new UriBuilder(this.BaseUri)
            {
                Path = pathname
            };
            return builder.Uri;
        }

        private HttpContent CreateRequestContent(object obj)
        {
            var content = JsonConvert.SerializeObject(obj);
            return new StringContent(content, Encoding.UTF8, "application/json");
        }

        private void HandleResponseStatusCode(HttpResponseMessage response)
        {
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                this.Logger.LogError("Schedule service request failed", ex);
                throw;
            }
        }

        private async Task<T> ParseResponse<T>(HttpResponseMessage response)
        {
            try
            {
                var output = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var result = JsonConvert.DeserializeObject<T>(output);
                return result!;
            }
            catch (JsonSerializationException ex)
            {
                this.Logger.LogError("Fail to deserialize response content", ex);
                throw;
            }
        }
    }
}
