In the first 3 parts of this series on using Azure Active Directory B2C to provide authentication and authorization to Xamarin mobile apps, we took a look at what exactly [Azure AD B2C](https://codemilltech.com/xamarin-authentication-with-azure-active-directory-b2c/) is, how to create a tenant, and then how to invoke a Web API from a Xamarin app.

In this post and the next I'm going to get to the good stuff ... using Azure AD B2C to provide authentication and authorization to a Xamarin mobile app so it can hit a Web API.

In other words... I'm going to show how to make sure the user logging is who they say they are, and then once authenticated, use that to gain authorization to the backing Web API.

The best way to understand the Azure AD B2C authentication and authorization process, and how it's used in mobile apps, is to understand the constituent parts of a the Azure AD B2C entity, as hosted in __THE CLOUD__.

And that's what this post is about...

## Azure AD B2C Application Parts

_As a quick aside, everything I'm going to talk about in this post is about Azure AD B2C, and lucky for us Azure AD B2C has this thing called an Application within it, to confuse us, because everything else we create is also called an application. So... everytime you read word application, I'm talking about the one that belongs to Azure AD B2C. I'll clarify if I mean mobile app or Web API app. Cool? Cool._

There are 4 constituent parts of Azure AD B2C which are essential for mobile authentication/authorization that I want to talk about.

* Tenant
* Policies
* Applications
* Scopes

#### Tenant
This is the overall container - think of it as the _Active Directory_ for everything that follows. 

At the top level, it hosts all the applications, users, user attributes, provider configuration (integration to Facebook or Twitter, etc.), and policies.

You can read more about the Tenant in [first](https://msou.co/6s) and [second](https://msou.co/6t) posts in this series.

I won't spend any time going over Tenant's than what's already in those 2 posts... 

However I want to point out that eventually, when users sign-up for our application, they will be asked to enter information about themselves. We can control which information is available to be asked for via the User Attributes node, as seen here. 

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_800/v1509998434/Screen_Shot_2017-11-06_at_1.51.50_PM_ywdcuf.png)

The Tenant doesn't need to utilize all the user attributes, and we can create custom user attributes within the Tenant as well.

#### Policies
Policies are kind of like endpoints that your mobile app calls to interact with the Azure AD B2C application.

These policies then provide certain functionality that allow your app (on the device) to "perform authentication".

By that I mean, if you want your user to sign in or sign up for your app - you would make sure to somehow invoke a "Sign-up or sign-in policy". 

> One thing to note - it is strongly suggested to use the __Sign-up or sign-in policy__ instead of the distinct __Sign-up policy__ and __Sign-in policy__. It'll make life much easier to use the combo policy...

For our purposes, there are 3 different types of policies, and their purpose should be evident by their names.

* Sign-up and sign-in policies
* Profile editing policies
* Password reset policies

So here's what's cool about these policies... when you configure one, you have several decisions to make about their behavior.

1. Which identity providers to allow. (So you could have a sign-in and sign-up policy that only allows Twitter authentication.)
1. For sign-up, you can decide which of the user attributes you want to have collected.
1. You can also then decide of those attributes collected - which should be returned to the mobile app. So it's possible to collect more info than the app will have access to. __These user attributes being returned to the mobile app will be done so within a token as a claim. That's important, which is why this is in bold.__
1. Whether to turn multi-factor authentication on or off.

There will also be settings to tweak for how long the token lifetime should be and so on.

And ... and ... when the mobile app starts the authentication workflow, it will be happening through a webview - so the policies give you the opportunity to customize the look and feel of the page that's displayed to fit your branding.

Policies are a top-level object in a Azure AD B2C Tenant - which means they can be shared amongst applications.

#### Applications

So, what in the world are these Azure AD B2C applications anyway?

An application also exists at the top level a Tenant and this is the thing that models your mobile app and WebAPI app within the Azure AD B2C Tenant.

Here you provide specifics on what to do when after authorization is requested for a Web API - or where the callback should go to.

You also obtain various constants - that you use to communicate with the Azure AD B2C application.

In other words, it's mainly a configuration container for your apps.

The real interesting part though is that it allows you to define something called Scopes, which function as permissions...

#### Scopes
Unlike everything else I talked about up to this point, scopes are not defined at the Tenant level, rather they are a part of the application definition.

A scope is a permission to a part of the application.

When making a request to Azure AD B2C - you're going to make that request to the application - specifying which policy you want to invoke. Included within that, you're also going to send along which scopes - or permissions you want to check to see if they user has access to.




