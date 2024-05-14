// <copyright file="BaseQueueService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>

namespace Microsoft.Purview.DataEstateHealth.DHDataAccess.Queue
{
    using global::Azure.Storage.Queues;
    using global::Azure.Storage.Queues.Models;
    using Microsoft.Azure.Purview.DataEstateHealth.Configurations;
    using Microsoft.Azure.Purview.DataEstateHealth.Loggers;
    using Microsoft.Extensions.Options;
    using Microsoft.Purview.ActiveGlossary.Scheduler.Setup.Secret;
    using System;
    using System.Threading.Tasks;

    public class BaseQueueService
    {
        protected readonly IDataEstateHealthRequestLogger logger;
        protected readonly QueueConfiguration configuration;
        protected readonly DHCosmosDBContextAzureCredentialManager credentialManager;

        protected string QueueName => this.configuration.QueueName;

        protected string QueueServiceUri => this.configuration.QueueServiceUri;

        public BaseQueueService(
            IOptions<QueueConfiguration> configuration,
            DHCosmosDBContextAzureCredentialManager credentialManager,
            IDataEstateHealthRequestLogger logger)
        {
            this.logger = logger;
            this.configuration = configuration.Value;
            this.credentialManager = credentialManager;
        }

        public async Task SendMessageAsync(string message)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(SendMessageAsync)}"))
            {
                var client = this.CreateQueueClient();
                await client.SendMessageAsync(message).ConfigureAwait(false);
            }
        }

        public async Task<QueueMessage[]> ReceiveMessagesAsync(int? maxMessages = null, TimeSpan? visibilityTimeout = null)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(ReceiveMessagesAsync)}"))
            {
                var client = this.CreateQueueClient();
                return await client.ReceiveMessagesAsync(maxMessages, visibilityTimeout).ConfigureAwait(false);
            }
        }

        public async Task DeleteMessage(string messageId, string popReceipt)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(DeleteMessage)}"))
            {
                await this.CreateQueueClient().DeleteMessageAsync(messageId, popReceipt).ConfigureAwait(false);
            }
        }

        public async Task UpdateMessage(string messageId, string popReceipt, string messageContent)
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(UpdateMessage)}"))
            {
                await this.CreateQueueClient().UpdateMessageAsync(messageId, popReceipt, messageContent).ConfigureAwait(false);
            }
        }

        public int GetApproximateMessagesCount()
        {
            using (this.logger.LogElapsed($"{this.GetType().Name}#{nameof(GetApproximateMessagesCount)}"))
            {
                return this.CreateQueueClient().GetProperties().Value.ApproximateMessagesCount;
            }
        }

        protected QueueClient CreateQueueClient()
        {
            var queueUri = new Uri($"{this.QueueServiceUri}/{this.QueueName}");
            return new QueueClient(queueUri, this.credentialManager.Credential);
        }
    }
}
