using Azure.Core;
using Azure.Identity;


namespace MyCustomApp.API.Extensions
{
    public static class KeyVaultExtension
    {
        public static void AddKeyVault(this IConfigurationBuilder configuration)
        {
            IConfigurationRoot configurationRoot = configuration.Build();
            string text = configurationRoot["KeyVault:Name"]!;
            if (!string.IsNullOrEmpty(text))
            {
                TokenCredential credential = (string.IsNullOrEmpty(configurationRoot?["KeyVault:TenantId"]) ? ((TokenCredential)new DefaultAzureCredential()) : ((TokenCredential)new ClientSecretCredential(configurationRoot["KeyVault:TenantId"], configurationRoot["KeyVault:ClientId"], configurationRoot["KeyVault:ClientSecret"])));
                //configuration.AddAzureKeyVault(new Uri("https://" + text + ".vault.azure.net/"), credential);
            }
        }
    }
}
