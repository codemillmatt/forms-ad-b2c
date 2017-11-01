using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;

using Xamarin.Forms;

using MvvmHelpers;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using System.Text;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace TheReviewer.Core
{
    public class ReviewListViewModel : BaseViewModel
    {
        string location = "http://localhost:5000";
        //string location = "http://thereviewer.azurewebsites.net";

        public ReviewListViewModel()
        {
            Title = "All Reviews";
        }

        public async Task<AuthenticationResult> SilentSignIn()
        {
            try
            {
                // ** NOTE: This will not work with iOS 10 & 11 on the simulator, best to use a real device **
                var resp = await App.AuthClient.AcquireTokenSilentAsync(App.Scopes,
                                     GetUserByPolicy(App.AuthClient.Users, App.SignUpAndInPolicy),
                                     App.Authority, false);
                LoggedOut = false;

                return resp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        bool _loggedOut = true;
        public bool LoggedOut
        {
            get => _loggedOut;
            set
            {
                SetProperty(ref _loggedOut, value);
            }
        }

        public ObservableRangeCollection<Review> AllReviews { get; set; } = new ObservableRangeCollection<Review>();

        Command _refreshCommand;
        public Command RefreshCommand => _refreshCommand ??
        (_refreshCommand = new Command(async () =>
        {
            try
            {
                // If running on the simulator, comment out the next 6 lines
                var authResult = await SilentSignIn();
                if (authResult == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Not logged in", "Please log in to get data", "OK");
                    return;
                }

                // Uncomment the below to run on the simulator
                //var authResult = await App.AuthClient.AcquireTokenAsync(App.Scopes,
                //GetUserByPolicy(App.AuthClient.Users, App.SignUpAndInPolicy),
                //App.UiParent);


                var baseAddr = new Uri(location);
                var client = new HttpClient { BaseAddress = baseAddr };

                var reviewUri = new Uri(baseAddr, "api/reviews");
                var request = new HttpRequestMessage(HttpMethod.Get, reviewUri);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var reviewJson = await response.Content.ReadAsStringAsync();

                var allReviews = JsonConvert.DeserializeObject<List<Review>>(reviewJson);

                AllReviews.AddRange(allReviews);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }));

        Command _loginCommand;
        public Command LoginCommand => _loginCommand ??
        (_loginCommand = new Command(async () =>
        {
            try
            {
                // Calling the AD B2C sign in or sign up policy
                var authResponse = await App.AuthClient.AcquireTokenAsync(App.Scopes,
                                      GetUserByPolicy(App.AuthClient.Users, App.SignUpAndInPolicy),
                                      App.UiParent);

                if (!string.IsNullOrEmpty(authResponse.AccessToken))
                    LoggedOut = false;
            }
            catch (Exception ex)
            {
                LoggedOut = true;
                Console.WriteLine(ex);
            }
        }));

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
