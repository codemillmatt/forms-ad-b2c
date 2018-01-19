using System;
namespace TheReviewer.Core
{
    public class ADB2C_Constants
    {
        public static string Tenant = "TheReviewer.OnMicrosoft.com";
        public static string ClientId = "978f6a35-db30-44fd-8544-b7cc40466adc";
        public static string[] ApplicationScopes = new string[] { $"https://{Tenant}/backend/rvw.read.only" };
        public static string AuthorityBaseUrl = $"https://login.microsoftonline.com/tfp/{Tenant}";

        public static string SignInUpPolicy = "B2C_1_GenericSignUpAndIn";
        public static string SignInUpAuthority = $"{AuthorityBaseUrl}/{SignInUpPolicy}";

        public static string ResetPasswordPolicy = "B2C_1_PasswordChange";
        public static string ResetPasswordAuthority = $"{AuthorityBaseUrl}/{ResetPasswordPolicy}";

        public static string EditProfilePolicy = "B2C_1_EditProfile";
        public static string EditProfileAuthority = $"{AuthorityBaseUrl}/{EditProfilePolicy}";

        public static string MSALRedirectUri = $"msal{ClientId}://auth";
    }
}
