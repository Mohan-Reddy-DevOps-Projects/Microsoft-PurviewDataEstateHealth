namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Configuration
{
    using Microsoft.Rest;

    public class TestConfiguration : ServiceClientCredentials, IDisposable
    {
        private bool disposedValue;

        /// <summary>
        /// Gets the accountId.
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Gets the accountId.
        /// </summary>
        public string SubscriptionId { get; set; }

        /// <summary>
        /// Gets the accountName.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets the tenantId.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The key vault where the test app certificate secret is set
        /// </summary>
        public string KeyVaultUri { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // noop
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
