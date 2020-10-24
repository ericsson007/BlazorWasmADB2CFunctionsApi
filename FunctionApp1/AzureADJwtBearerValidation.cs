using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FunctionApp1
{
    public class AzureADJwtBearerValidation
    {
        private IConfiguration _configuration;
        private ILogger _log;
        private const string scopeType = @"http://schemas.microsoft.com/identity/claims/scope";
        private ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        private string _wellKnownEndpoint = string.Empty;
        private string _tenantId = string.Empty;
        private string _audience = string.Empty;
        private string _instance = string.Empty;
        private string _userFlow = string.Empty;
        private string _requiredScope = "Read";

        public AzureADJwtBearerValidation(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _log = loggerFactory.CreateLogger<AzureADJwtBearerValidation>();

            _tenantId = _configuration["AzureAd:TenantId"];
            _audience = _configuration["AzureAd:ClientId"];
            _instance = _configuration["AzureAd:Instance"];
            _userFlow = _configuration["AzureAd:UserFlow"];
            _wellKnownEndpoint = $"{_instance}/{_tenantId}/{_userFlow}/v2.0/.well-known/openid-configuration";

            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(_wellKnownEndpoint, new OpenIdConnectConfigurationRetriever());

        }

        public async Task<ClaimsPrincipal> ValidateTokenAsync(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            if (!authorizationHeader.Contains("Bearer"))
            {
                return null;
            }

            var accessToken = authorizationHeader.Substring("Bearer ".Length);

            _log.LogDebug($"Get OIDC well known endpoints {_wellKnownEndpoint}");
            var oidcWellknownEndpoints = await _configurationManager.GetConfigurationAsync();
            _log.LogDebug($"Got the OIDC");

            var tokenValidator = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = true,
                ValidAudience = _audience,
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKeys = oidcWellknownEndpoints.SigningKeys,
                ValidIssuer = oidcWellknownEndpoints.Issuer
            };

            try
            {
                SecurityToken securityToken;
                var _claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);

                if (IsScopeValid(_requiredScope, _claimsPrincipal))
                {
                    return _claimsPrincipal;
                }

                return null;
            }
            catch (Exception ex)
            {
                _log.LogError(ex.ToString());
            }
            return null;
        }

        public string GetPreferredUserName(ClaimsPrincipal claimsPrincipal)
        {
            string preferredUsername = string.Empty;
            var preferred_username = claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "name");
            if (preferred_username != null)
            {
                preferredUsername = preferred_username.Value;
            }

            return preferredUsername;
        }

        private bool IsScopeValid(string scopeName, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
            {
                _log.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            var scopeClaim = claimsPrincipal.HasClaim(x => x.Type == scopeType)
                ? claimsPrincipal.Claims.First(x => x.Type == scopeType).Value
                : string.Empty;

            if (string.IsNullOrEmpty(scopeClaim))
            {
                _log.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            if (!scopeClaim.Equals(scopeName, StringComparison.OrdinalIgnoreCase))
            {
                _log.LogWarning($"Scope invalid {scopeName}");
                return false;
            }

            _log.LogDebug($"Scope valid {scopeName}");
            return true;
        }
    }
}
