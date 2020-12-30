using AzureKeyVaultExemplo.Settings;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AzureKeyVaultExemplo.Extensions
{
    public static class ConnectionStringExtension
    {

        public static string KEY_VAULT_API { get; set; }
        public static string CLIENT_ID { get; set; }
        public static string SECRET_KEY { get; set; }

        public static void AddDecriptConnectionString(this IServiceCollection app, IConfiguration configuration)
        {
            try
            {
                AppSettings.ConnectionStrings.DefaultConnection = GetConnectionStringFromKeyVault(configuration).Result;
            }
            catch (Exception ex)
            {
                throw new Exception("Falha ao recuperar a connection string do Key Vault");
            }
        }

        private async static Task<string> GetConnectionStringFromKeyVault(IConfiguration configuration)
        {
            KEY_VAULT_API = configuration.GetValue<string>("KeyVault:KeyVaultApi");
            CLIENT_ID = configuration.GetValue<string>("KeyVault:ClientId");
            SECRET_KEY = configuration.GetValue<string>("KeyVault:SecretKey");

            var keyVaultClient = new KeyVaultClient(GetAccessTokenAsync, new HttpClient());
            var cacheSecret = await keyVaultClient.GetSecretAsync(KEY_VAULT_API, "minha-connection-string");
            string connectionString = cacheSecret.Value;
            return connectionString;
        }

        public async static Task<string> GetAccessTokenAsync(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            ClientCredential credential = new ClientCredential(CLIENT_ID, SECRET_KEY);
            AuthenticationResult result = await context.AcquireTokenAsync(resource, credential);
            if (result == null)
                throw new InvalidOperationException("Falha ao recuperar token");

            return result.AccessToken;
        }
    }
}
