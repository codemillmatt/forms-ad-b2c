When you allow users to create accounts from within your app, eventually they are going to want to change their passwords, or edit some of the information they provided at sign-up.

And I wouldn't be writing this post if Azure AD B2C didn't allow your users to change passwords and their information!

I mentioned way back in the first post on how everything is organized within Azure AD B2C that policies control the workflow the user experiences as they work their way through the authentication and authorization process. Resetting passwords and updating profile information is no different - they are both dictated by policies.

So without further ado, in this post we're going to look at how to create and setup the policies within the portal and then call them with the MSAL library from your Xamarin-based project.

## Resetting Passwords

First up, let's talk about resetting passwords.

The ability to reset passwords only apply to _Local Accounts_ within Azure AD B2C. In other words, you can only reset your password if you signed up using an email address and password. You're not able to reset a social provider's password through this mechanism.

### Resetting a Password Screen Flow

When a user requests to reset their password - they are brought through a series of screens which look like the following:

First they are asked to verify their email address.

In order to verify the email address a code is sent to the email address that was entered.

Of course, the user must enter that code - or have another one sent.

Once a valid code has been entered, they can now either change their email address or tap continue to change their password.

Finally, the new / confirm password screen is displayed.

### Creating the Reset Pasword Policy

All of the user interaction with Azure AD B2C is dictated through policies setup within the Tenant in the Azure portal. You create a policy by logging into your Tenant, then selecting the _Password reset policies_ from the left hand menu options, and then selecting new in the resulting blade.

This [set of documentation](https://msou.co/bap) will walk you through the process.

A couple interesting things to note when creating or editing password reset policies.

The first is that, again, only the local account identity provider is allowed to be selected from the _Identity Providers_ option. Makes sense - that's the only account type where AD holds onto the password.

The second is the _Application claims_ blade specifies what information you want sent back to the client when a password is successfully reset. This means that not only is the password reset policy changing a password, but you can use the info it sends back inside the app by inspecting its claims. And it also sends back an authorization token that can be used to get at resources, such as an Azure Function, which requires authorization to be invoked!

The _Multifactor authentication_ blade is as it sounds like - a simple switch to require MFA in order to change the password.

Then finally, within _Page UI customization_ you can set the custom HTML for both the Forgot Password page and the Error page that's displayed during the workflow. (See this article for more info on what's all involved in setting up custom UIs.)

### Updating the MSAL -> B2C Client Code

Invoking the new policy to reset passwords is easier than it seems on the surface.

The service that we're using to invoke everything on Azure AD B2C is still using the MSAL client. And, in fact, we're still going to invoke the same function, `AcquireTokenAsync`, as we did when initially signing-in into and acquiring the authorization token with Azure AD B2C.

There are a couple of changes - but they're pretty minor. Let's take a look at the snippet of code that's performing the password reset.

```language-csharp
AuthenticationResult result = 
    await msaClient.AcquireTokenAsync(ADB2C_Constants.ApplicationScopes,
                           (IUser)null,
                           UIBehavior.SelectAccount,
                           null,
                           null,
                           ADB2C_Constants.ResetPasswordAuthority,
                           UIParent);
```

The `AcquireTokenAsync` is still returning an `AuthenticationResult` object. That object is going to have everything in it that we'd expect it to have ... an access token, when it expires, the scopes it's authorized for, and within an ID token field, all of the claims.

One difference in the way `AcquireTokenAsync` is invoked is that it is not passed a `IUser` object. It's `UIBehavior` is set to `SelectAccount`. And the URL it's set to call is the URL of the reset password policy.

The URL of the reset password policy is of the following format: `https://login.microsoftonline.com/tfp/{YOUR-TENANT-NAME-HERE}/{YOUR-RESET-PASSWORD-POLICY-NAME-HERE}`

And that's all there is to that! Well, except updating the UI code to make use of the service code that invokes the reset password policy.

### Updating the Client UI

The question here is - how to handle the token / result from the `AcquireTokenAsync` call in the Xamarin.Forms app? Where to store it? Which one from which policy to use?

When the `AcquireTokenAsync` method returns - it will have put an additional `IUser` into the `Users` collection within the `PublicClientApplication` object that's used to communicate up to the Azure AD B2C application. This new user will be associated with the policy that was just invoked.

The `AuthenticationResult` object that it returns will also have the appropriate token that can later be used to access any resources that require tokens.

Getting an appropriate token out of the `PublicClientApplication.Users` collection is as simple as passing the correct _policy-user_ whose token you want to the `AcquireTokenSilentAsync` function.

In other words - as long as the scope is correct to access the resource you want - you can use the token from any user in the `PublicClientApplication's` collection - including the token returned immediately from `AcquireTokenAsync`.

Confusing? In a future post I'm going to outline the best practices of using the MSAL library to store and retrieve tokens. For now, the demo app I'm using always immediately uses the token in the `AuthenticationResult` object - and if you shut the app down - it forgets that you were even logged in! :)

> Pro tip ... when debugging anything that deals with Azure AD B2C - use a real device instead of the simulator ... it will make life much easier in the long run!

## Editing User Information



### Editing the User Information Screen Flow

### Creating the Profile Editing Policy

### Updating the MSL -> B2C Client Code

### Updating the Client UI

## Summary