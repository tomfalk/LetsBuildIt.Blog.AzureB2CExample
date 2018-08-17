using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace LetsBuildIt.Web.API
{
    public static class BearerTokenValidator
    {
        private static readonly IConfigurationManager<OpenIdConnectConfiguration> ConfigurationManager;

        static BearerTokenValidator()
        {
            string domain = Environment.GetEnvironmentVariable("domain");
            string policy = Environment.GetEnvironmentVariable("policy");
            string wellKnownEndpoint = $"https://login.microsoftonline.com/tfp/{domain}/{policy}/v2.0/.well-known/openid-configuration";
            var documentRetriever = new HttpDocumentRetriever { RequireHttps = wellKnownEndpoint.StartsWith("https://") };
            ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownEndpoint,
                new OpenIdConnectConfigurationRetriever(),
                documentRetriever
            );
        }

        public static async Task<ClaimsPrincipal> ValidateAsync(HttpRequest req, ILogger log)
        {
            log.LogInformation("Entering auth...");

            string authorizationHeader = req.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader))
                return null;

            if (!authorizationHeader.StartsWith("Bearer "))
                return null;

            string bearerToken = authorizationHeader.Substring("Bearer ".Length);

            var config = await ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
            var audience = Environment.GetEnvironmentVariable("audience");

            var validationParameter = new TokenValidationParameters()
            {
                RequireSignedTokens = true,
                ValidAudience = audience,
                ValidateAudience = true,
                ValidIssuer = config.Issuer,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys
            };

            ClaimsPrincipal result = null;
            var tries = 0;

            while (result == null && tries <= 1)
            {
                try
                {
                    log.LogInformation("Trying to validate claims...");
                    var handler = new JwtSecurityTokenHandler();
                    result = handler.ValidateToken(bearerToken, validationParameter, out SecurityToken _);
                    if (result != null)
                        log.LogInformation($"Result: {result.ToString()}");
                    else
                        log.LogInformation("Null result");
                }
                catch (SecurityTokenSignatureKeyNotFoundException ex)
                {
                    log.LogInformation($"Security token signature key not found exception: {ex.Message}");
                    log.LogInformation($"Inner exception: {ex.InnerException}");
                    // This exception is thrown if the signature key of the JWT could not be found.
                    // This could be the case when the issuer changed its signing keys, so we trigger a 
                    // refresh and retry validation.
                    ConfigurationManager.RequestRefresh();
                    tries++;
                }
                catch (SecurityTokenException)
                {
                    log.LogInformation("Security token exception");
                    return null;
                }
            }

            return result;
        }
    }
}
