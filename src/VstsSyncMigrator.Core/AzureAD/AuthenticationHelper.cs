using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace VstsSyncMigrator
{
    internal class AuthenticationHelper
    {
        public static string TokenForUser;

        /// <summary>
        /// Async task to acquire token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> AcquireTokenAsyncForUser()
        {
            return await GetTokenForUser();
        }

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUser()
        {
            if (TokenForUser == null)
            {
                var redirectUri = new Uri("urn:ietf:wg:oauth:2.0:oob");
                AuthenticationContext authenticationContext = new AuthenticationContext(UserModeConstants.AuthString, false);
                AuthenticationResult userAuthnResult = await authenticationContext.AcquireTokenAsync(GlobalConstants.ResourceUrl,
                    UserModeConstants.ClientId, redirectUri, new PlatformParameters(PromptBehavior.RefreshSession));
                TokenForUser = userAuthnResult.AccessToken;
                Trace.WriteLine("\n Welcome " + userAuthnResult.UserInfo.GivenName + " " +
                                  userAuthnResult.UserInfo.FamilyName);
            }
            return TokenForUser;
        }
    }
}