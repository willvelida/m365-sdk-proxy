using M365ProxyAgent.Configuration;
using M365ProxyAgent.Exceptions;
using M365ProxyAgent.Interfaces;
using M365ProxyAgent.Services;
using Microsoft.Agents.CopilotStudio.Client;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace M365ProxyAgent.Auth
{
    public class AddTokenHandlerS2S(CopilotStudioClientSettings settings, IResilienceService? resilienceService = null) : DelegatingHandler(new HttpClientHandler())
    {
        private static readonly string _keyChainServiceName = "copilot_studio_client_app";
        private static readonly string _keyChainAccountName = "copilot_studio_client";

        private readonly IResilienceService? _resilienceService = resilienceService;
        private IConfidentialClientApplication? _confidentialClientApplication;
        private string[]? _scopes;

        private async Task<AuthenticationResult> AuthenticateAsync(CancellationToken ct = default!)
        {
            ArgumentNullException.ThrowIfNull(settings);

            if (_resilienceService != null)
            {
                return await _resilienceService.AuthenticationPipeline.ExecuteAsync(async cancellationToken =>
                {
                    return await PerformAuthenticationAsync(cancellationToken);
                }, ct);
            }
            else
            {
                return await PerformAuthenticationAsync(ct);
            }
        }

        private async Task<AuthenticationResult> PerformAuthenticationAsync(CancellationToken ct)
        {
            try
            {
                if (_confidentialClientApplication == null)
                {
                    _scopes = [CopilotClient.ScopeFromSettings(settings)];
                    _confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(settings.AppClientId)
                        .WithAuthority(AzureCloudInstance.AzurePublic, settings.TenantId)
                        .WithClientSecret(settings.AppClientSecret)
                        .Build();

                    string currentDir = Path.Combine(AppContext.BaseDirectory, "mcs_client_console");

                    if (!Directory.Exists(currentDir))
                    {
                        Directory.CreateDirectory(currentDir);
                    }

                    StorageCreationPropertiesBuilder storageProperties = new("AppTokenCache", currentDir);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        storageProperties.WithLinuxUnprotectedFile();
                    }
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        storageProperties.WithMacKeyChain(_keyChainServiceName, _keyChainAccountName);
                    }
                    MsalCacheHelper tokenCacheHelper = await MsalCacheHelper.CreateAsync(storageProperties.Build());
                    tokenCacheHelper.RegisterCache(_confidentialClientApplication.AppTokenCache);
                }

                AuthenticationResult authResponse = await _confidentialClientApplication.AcquireTokenForClient(_scopes).ExecuteAsync(ct);
                return authResponse;
            }
            catch (MsalException ex)
            {
                throw new AuthenticationException(
                    $"MSAL authentication failed: {ex.Message}",
                    "ClientCredentials",
                    settings.TenantId,
                    Guid.NewGuid().ToString("D"),
                    ex);
            }
            catch (Exception ex)
            {
                throw new AuthenticationException(
                    $"Authentication failed: {ex.Message}",
                    "ClientCredentials",
                    settings.TenantId,
                    Guid.NewGuid().ToString("D"),
                    ex);
            }
        }

        /// <summary>
        /// Handles sending the request and adding the token to the request.
        /// Implements comprehensive error handling and logging.
        /// </summary>
        /// <param name="request">Request to be sent</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            try
            {
                if (request.Headers.Authorization is null)
                {
                    AuthenticationResult authResponse = await AuthenticateAsync(cancellationToken);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
                }
                return await base.SendAsync(request, cancellationToken);
            }
            catch (AuthenticationException)
            {
                // Re-throw authentication exceptions as they already have proper context
                throw;
            }
            catch (HttpRequestException ex)
            {
                throw new CopilotClientException(
                    $"HTTP request failed: {ex.Message}",
                    "SendRequest",
                    null,
                    Guid.NewGuid().ToString("D"),
                    ex);
            }
            catch (Exception ex)
            {
                throw new CopilotClientException(
                    $"Unexpected error during request processing: {ex.Message}",
                    "SendRequest",
                    null,
                    Guid.NewGuid().ToString("D"),
                    ex);
            }
        }
    }
}
