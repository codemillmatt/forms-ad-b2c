using System;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace TheReviewer.Core
{
    public partial class App : Application
    {
        public static string Tenant = "TheReviewer.onmicrosoft.com";
        public static string ClientID = "978f6a35-db30-44fd-8544-b7cc40466adc";
        public static string SignUpAndInPolicy = "B2C_1_GenericSignUpAndIn";

        public static string[] Scopes = new string[] { "https://TheReviewer.onmicrosoft.com/backend/rvw.read.only" };
        public static string ApiEndpoint = "http://thereviewer.azurewebsites.net/api/reviews";

        public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
        public static string Authority = $"{AuthorityBase}{SignUpAndInPolicy}";
        //public static string AuthorityEditProfile = $"{AuthorityBase}{PolicyEditProfile}";
        //public static string AuthorityPasswordReset = $"{AuthorityBase}{PolicyResetPassword}";

        public static PublicClientApplication AuthClient = null;

        public static UIParent UiParent = null;

        public App()
        {
            InitializeComponent();

            AuthClient = new PublicClientApplication(ClientID, Authority);
            AuthClient.ValidateAuthority = false;
            AuthClient.RedirectUri = $"msal{ClientID}://auth";

            MainPage = new NavigationPage(new ReviewListPage());
        }
    }
}
