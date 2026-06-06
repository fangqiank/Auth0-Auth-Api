namespace Auth0.Auth.Api.Models
{
    public class Auth0Options
    {
        public const string SectionName = "Authentication";

        /// <summary>
        /// Auth0 租户域名 (例如: your-tenant.auth0.com)
        /// </summary>
        public string Domain { get; init; } = string.Empty;

        /// <summary>
        /// API Identifier/Audience (例如: https://your-api-identifier)
        /// </summary>
        public string Audience { get; init; } = string.Empty;

        /// <summary>
        /// Auth0 Application Client ID
        /// </summary>
        public string ClientId { get; init; } = string.Empty;

        /// <summary>
        /// Auth0 Application Client Secret
        /// </summary>
        public string ClientSecret { get; init; } = string.Empty;

        /// <summary>
        /// Auth0 Authorization URL
        /// </summary>
        public string AuthorizationUrl { get; init; } = string.Empty;

        /// <summary>
        /// Auth0 Token URL
        /// </summary>
        public string TokenUrl { get; init; } = string.Empty;

        /// <summary>
        /// OpenID Configuration Metadata Address
        /// </summary>
        public string MetadataAddress { get; init; } = string.Empty;

        /// <summary>
        /// Token Valid Issuer
        /// </summary>
        public string ValidIssuer { get; init; } = string.Empty;
    }
}
