using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using TheReviewer.Core;
using Xamarin.Forms;

[assembly: Dependency(typeof(IdentityService))]
namespace TheReviewer.Core
{
    public class IdentityService : IIdentityService
    {
        readonly PublicClientApplication msaClient;

        public IdentityService()
        {
            msaClient = new PublicClientApplication(ADB2C_Constants.ClientId);
            msaClient.ValidateAuthority = false;

            // This needs to match what was entered in the portal under the Native Client redirect Url
            msaClient.RedirectUri = ADB2C_Constants.MSALRedirectUri;
        }

        UIParent parent;
        public UIParent UIParent { get => parent; set => parent = value; }

        public async Task<AuthenticationResult> Login()
        {
            AuthenticationResult result = null;

            // Running on Android - we need UIParent to be set to the main Activity
            if (UIParent == null && Device.RuntimePlatform == Device.Android)
                return result;

            // First check if the token happens to be cached - grab silently
            result = await GetCachedSignInToken();

            if (result != null)
                return result;

            // Token not in cache - call adb2c to acquire it
            try
            {
                result = await msaClient.AcquireTokenAsync(ADB2C_Constants.ApplicationScopes,
                                                           GetUserByPolicy(msaClient.Users,
                                                                           ADB2C_Constants.SignInUpPolicy),
                                                           UIBehavior.ForceLogin,
                                                           null,
                                                           null,
                                                           ADB2C_Constants.SignInUpAuthority,
                                                           UIParent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public async Task<AuthenticationResult> GetCachedSignInToken()
        {
            try
            {
                // This checks to see if there's already a user in the cache
                return await msaClient.AcquireTokenSilentAsync(ADB2C_Constants.ApplicationScopes,
                                                               GetUserByPolicy(msaClient.Users,
                                                                               ADB2C_Constants.SignInUpPolicy),
                                                               ADB2C_Constants.SignInUpAuthority,
                                                               false);
            }
            catch (MsalUiRequiredException ex)
            {
                // happens if the user hasn't logged in yet & isn't in the cache
                Console.WriteLine(ex);
            }

            return null;
        }

        IUser GetUserByPolicy(IEnumerable<IUser> users, string policy)
        {
            foreach (var user in users)
            {
                string userIdentifier = Base64UrlDecode(user.Identifier.Split('.')[0]);

                if (userIdentifier.EndsWith(policy.ToLower(), StringComparison.OrdinalIgnoreCase)) return user;
            }

            return null;
        }

        string Base64UrlDecode(string s)
        {
            s = s.Replace('-', '+').Replace('_', '/');
            s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
            var byteArray = Convert.FromBase64String(s);
            var decoded = Encoding.UTF8.GetString(byteArray, 0, byteArray.Count());
            return decoded;
        }

        JObject ParseIdToken(string idToken)
        {
            // Get the piece with actual user info
            idToken = idToken.Split('.')[1];
            idToken = Base64UrlDecode(idToken);
            return JObject.Parse(idToken);
        }
    }
}
