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
        // All of the below must be setup according to values in your AD B2C tenant and application
        static readonly string tenant = "TheReviewer.OnMicrosoft.com";
        static readonly string clientId = "978f6a35-db30-44fd-8544-b7cc40466adc";
        static readonly string[] applicationScopes = new string[] { $"https://{tenant}/backend/rvw.read.only" };
        static readonly string authorityBaseUrl = $"https://login.microsoftonline.com/tfp/{tenant}";

        static readonly string signInUpPolicy = "B2C_1_GenericSignUpAndIn";

        static readonly string signInUpAuthority = $"{authorityBaseUrl}/{signInUpPolicy}";

        readonly PublicClientApplication msaClient;

        public IdentityService()
        {
            msaClient = new PublicClientApplication(clientId);
            msaClient.ValidateAuthority = false;

            // This needs to match what was entered in the portal under the Native Client redirect Url
            msaClient.RedirectUri = $"msal{clientId}://auth";
        }

        UIParent parent;
        public UIParent UIParent { get => parent; set => parent = value; }

        public async Task<AuthenticationResult> Login()
        {
            AuthenticationResult result = null;

            if (Device.RuntimePlatform == Device.Android && UIParent == null)
                return result;

            // First check if the token happens to be cached - grab silently
            result = await GetCachedSignInToken();

            if (result != null)
                return result;

            // Token not in cache - call adb2c to acquire it
            result = await msaClient.AcquireTokenAsync(applicationScopes, GetUserByPolicy(msaClient.Users, signInUpPolicy),
                                                       UIBehavior.ForceLogin, null, null, signInUpAuthority, UIParent);


            return result;
        }

        public async Task<AuthenticationResult> GetCachedSignInToken()
        {
            try
            {
                return await msaClient.AcquireTokenSilentAsync(applicationScopes, GetUserByPolicy(msaClient.Users, signInUpPolicy),
                                                               signInUpAuthority, false);
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
