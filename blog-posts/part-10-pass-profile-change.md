When you allow users to create accounts from within your app, eventually they are going to want to change their passwords, or edit some of the information they provided at sign-up.

And I wouldn't be writing this post if [Azure AD B2C](https://msou.co/bak) didn't allow your users to change passwords and their information!

I mentioned way back in the first post on how everything is organized within Azure AD B2C that policies control the workflow the user experiences as they work their way through the authentication and authorization process. Resetting passwords and updating profile information is no different - they are both dictated by [policies](https://msou.co/bar).

So without further ado, in this post we're going to look at how to create and setup the [policies](https://msou.co/bar) within the portal and then call them with the MSAL library from your Xamarin-based project.

All the code for this post can be found on [GitHub](msou.co/baw).

## Resetting Passwords

First up, let's talk about resetting passwords.

The ability to reset passwords only apply to _Local Accounts_ within Azure AD B2C. In other words, you can only reset your password if you signed up using an email address and password. You're not able to reset a social provider's password through this mechanism.

### Resetting a Password Screen Flow

When a user requests to reset their password - they are brought through a series of screens which look like the following:

__First they are asked to verify their email address.__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516368900/Simulator_Screen_Shot_-_iPhone_8_-_2018-01-18_at_10.01.00_s9aho9.png)

__In order to verify the email address a code is sent to the email address that was entered.__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000/v1516368966/Screen_Shot_2018-01-18_at_10.01.48_AM_byghmb.png)

__Of course, the user must enter that code - or have another one sent.__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516369022/Simulator_Screen_Shot_-_iPhone_8_-_2018-01-18_at_10.01.22_i8wvki.png)

__Once a valid code has been entered, they can now either change their email address or tap continue to change their password.__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516369080/Simulator_Screen_Shot_-_iPhone_8_-_2018-01-18_at_10.02.21_eilne5.png)

__Finally, the new / confirm password screen is displayed.__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516369124/Simulator_Screen_Shot_-_iPhone_8_-_2018-01-18_at_10.02.36_bync8p.png)

### Creating the Reset Pasword Policy

All of the user interaction with Azure AD B2C is dictated through [policies](https://msou.co/bar) setup within the Tenant in the Azure portal. You create a policy by logging into your Tenant, then selecting the _Password reset policies_ from the left hand menu options, and then selecting _add_ in the resulting blade.

This [set of documentation](https://msou.co/bap) will walk you through the process.

A couple interesting things to note when creating or editing password reset policies.

The first is that, again, only the local account identity provider is allowed to be selected from the _Identity Providers_ option. Makes sense - that's the only account type where AD holds onto the password.

The second is the _Application claims_ blade specifies what information you want sent back to the client when a password is successfully reset. This means that not only is the password reset policy changing a password, but you can use the info it sends back inside the app by inspecting its claims. And it also sends back an authorization token that can be used to get at resources, such as an Azure Function, which requires authorization to be invoked!

The _Multifactor authentication_ blade is as it sounds like - a simple switch to require MFA in order to change the password.

Then finally, within _Page UI customization_ you can set the custom HTML for both the Forgot Password page and the Error page that's displayed during the workflow. (See this [article](https://msou.co/bat) and [this one](https://msou.co/95) for more info on what's all involved in setting up custom UIs.)

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

The question here is - how to handle the [token](https://msou.co/bau) / result from the password reset `AcquireTokenAsync` call in the Xamarin.Forms app? Where to store it? Which one from which policy to use? Do we still use the one from the sign-in or the new one from the reset password?

When the `AcquireTokenAsync` method returns - it will have put an additional `IUser` into the `Users` collection within the `PublicClientApplication` object that's used to communicate up to the Azure AD B2C application. This new user will be associated with the policy that was just invoked.

Assuming the user has previously signed in, there will be an `IUser` object associated with the _Sign-up and sign-in_ policy and another `IUser` associated with the _Reset password_ policy.

The `AuthenticationResult` object from the reset password operation will have an appropriate token that can later be used to access any resources that require them.

Getting a token out of the `PublicClientApplication.Users` collection is as simple as passing the correct _policy-user_ whose token you want to the `AcquireTokenSilentAsync` function.

In other words - as long as the scope is correct to access the resource you want - you can use the token from any user in the `PublicClientApplication's` collection - including the token returned immediately from `AcquireTokenAsync`.

Confusing? In a future post I'm going to outline the best practices of using the MSAL library to store and retrieve tokens. 

For now, the demo app I'm accompanying this post immediately uses the token in the `AuthenticationResult` object that's returned from the latest `AcquireTokenAsync` call - and if you shut the app down - it forgets that you were even logged in! :) This is because I want to always force the user to sign-in or change a password in order to get at the data in the web service. Pure for demonstration purposes.

> Pro tip ... when debugging anything that deals with Azure AD B2C - use a real device instead of the simulator ... it will make life much easier in the long run!

## Editing User Information

The information a user can edit is specified through ... wait for it ... profile editing policies! (Go figure.)

If you remember back to one of the first posts in this series, there are a number of user attributes that can be collected and stored in the backing Active Directory. These are set within the _User attributes_ section of the Tenant.

The user is able to modify any of those attributes except for system based ones (such as object ID) that you specify within the edit policy.

So let's find out all about these edit policies!

### Editing the User Information Screen Flow

First here's a look at the screenflow the user will experience when editing their profile:

__Choose the identity provider__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516374155/screenshot-1516371304803_jrdizg.jpg)

Unlike changing a password where it only applies to local accounts, editing a profile can be done through any of the identity providers setup. So the first step is to choose which identity provider you want to log in with.

__Multifactor authentication__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516374190/screenshot-1516371321823_vxhoqy.jpg)

Assuming you have MFA enabled.

__Verify the MFA__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516374233/screenshot-1516371369254_yfd7ba.jpg)

Again, assuming you have MFA enabled, once you get the code you'll have to enter it.

__Update the profile info__

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1516374268/screenshot-1516371387783_y8yc8h.jpg)

Finally, once you pass all the verification hoops, the user can change the information you that specified as changeable in the edit policy.

Now let's look at that edit policy!

### Creating the Profile Editing Policy

Much like the _Password reset policies_, all interaction for editing user information is dictated through _Profile editing policies_ which are created within the Tenant in the Azure portal.

You create a _Profile editing policy_ by logging into the Tenant in the portal and selecting _Profile editing policies_ from the left under the _Policies_ heading. From the new blade that appears, click the plus button.

This [set of documentation](https://msou.co/baq) will walk you through the process.

A couple of interesting things to note while creating (or later editing) the policy.

First, you can select as many of the identity providers as you have configured in the Tenant to allow the user to sign-in with.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_600/v1516374329/Screen_Shot_2018-01-19_at_8.34.48_AM_yjn8yb.png)

The profile attributes specifies which of the _User attributes_ can be edited. (You don't have to specify all of them - and you can specify some of them that you don't include in the _Sign-up or sign-in_ policy.)

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_600/v1516374365/Screen_Shot_2018-01-19_at_8.37.54_AM_bayrhq.png)

Application claims lets you specify which of the user attributes you want returned as claims to you app. And Page UI customization lets you pick custom HTML pages to display during the various work flow stages the user goes through while updating their profile info.

### Updating the MSL -> B2C Client Code

The code from the Xamarin app to invoke the edit profile policy is pretty straight forward ... in fact it looks a lot like the code from the reset password section!

We're still going to use the MSAL client. We're still going to invoke `AcquireTokenAsync`. Still going to get back an `AuthenticationResult` object. Still can use the token within that object to access any resources that need tokens from our Azure AD B2C application.

```language-csharp
AuthenticationResult result = 
    await msaClient.AcquireTokenAsync(ADB2C_Constants.ApplicationScopes,
                        GetUserByPolicy(msaClient.Users,
                            ADB2C_Constants.EditProfilePolicy),
                        UIBehavior.SelectAccount,
                        null,
                        null,
                        ADB2C_Constants.EditProfileAuthority,
                        UIParent);
```

Some differences here... `GetUserByPolicy` scans the `Users` collection in the `msaClient` object to see if there already is an `IUser` in there that is from this policy. In other words, if our app has already called the edit profile policy, there will be a user in that collection and we can pass that up.

The `UIBehavior.SelectAccount` is telling MSAL how it wants the UI to be displayed. 

And then finally, the `ADB2C_Constants.EditProfileAuthority` is a URL that points to the edit profile policy and is of the format: `https://login.microsoftonline.com/tfp/{YOUR-TENANT-NAME-HERE}/{YOUR-EDIT-PROFILE-POLICY-NAME-HERE}`

### Updating the Client UI

The question of updating the client UI - or what to do with the token after the edit policy is invoked is the same as it was in the reset password section.

After the `AcquireTokenAsync` returns, the MSAL will place another `IUser` object into the `PublicClientApplication.Users` collection. This time it will be an `IUser` for the edit profile policy.

For the purposes of the demo app, I'm using the latest [access token](https://msou.co/bau) (or whatever is being returned from the edit policy version of `AcquireTokenAsync`) to invoke the [web service](https://msou.co/8y). I'm not worrying about which policy was invoked, be it a sign-up/in or password reset, or profile edit. In fact, every time the demo app starts, I'm clearing out any cached users the MSAL may have stored, that way there's always a clean slate to work off of.

In a future post I'll go over the best practices of which `IUser` to use when, and which token to use when.

## Summary

In this article we went over the different policies that you use in order to allow users to change their passwords and to update information stored in their user profile.

The initial setup is all done through the portal by configuring policies - which dictate the workflow users take in order to interact with Azure AD B2C.

Then the app still uses the MSAL library - and still invokes the `AcquireTokenAsync` method to invoke those policies. The object returned from that method has an access token in it which can be used to get at any service which is setup to require the Azure AD B2C tokens from your [Tenant application](https://msou.co/8x).

In the next post we'll take a look at securing resources even more. Currently when you have an authorization token, that's all you need to perform any operation the resource offers. The next post will show you how to inspect who the user is and to make sure that not everybody can do everything within the backing server or resource application.