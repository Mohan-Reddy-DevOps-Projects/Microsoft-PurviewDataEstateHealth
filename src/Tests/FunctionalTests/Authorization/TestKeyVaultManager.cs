namespace Microsoft.Azure.Purview.DataEstateHealth.FunctionalTests.Authorization
{
    using global::Azure.Identity;
    using global::Azure.Security.KeyVault.Secrets;

    public class TestKeyVaultManager
    {
        private readonly SecretClient secretClient;

        /// <summary>
        /// Public constructor
        /// </summary>
        public TestKeyVaultManager(string keyVaultUri)
        {
            this.secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
        }

        /// <summary>
        /// Get a secret from key vault using the current user's identity
        /// </summary>
        /// <param name="secretName">Secret name</param>
        /// <returns>Secret value</returns>
        public async Task<string> GetSecretAsync(string secretName)
        {
            var secretBundle = await this.secretClient.GetSecretAsync(secretName).ConfigureAwait(false);
            return secretBundle.Value.Value;
        }
    }
}
