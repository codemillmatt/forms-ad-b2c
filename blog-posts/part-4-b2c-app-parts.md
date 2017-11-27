In the first 3 parts of this series on using Azure Active Directory B2C to provide authentication and authorization to Xamarin mobile apps, we took a look at what exactly [Azure AD B2C](https://msou.co/6z) is, [how to create a tenant](https://msou.co/60), and then how to [invoke a Web API from a Xamarin app](https://msou.co/61).

In this post and the next I'm going to get to the good stuff ... using Azure AD B2C to provide authentication and authorization to a Xamarin mobile app so it can hit a Web API.

In other words... I'm going to show how to make sure the user logging is who they say they are, and then once authenticated, use that to gain authorization to the backing Web API.

The best way to understand the Azure AD B2C authentication and authorization process, and how it's used in mobile apps, is to understand the constituent parts of a the Azure AD B2C entity, as hosted in __THE CLOUD__.

And that's what this post is about...

## Azure AD B2C Application Parts

_As a quick aside, everything I'm going to talk about in this post is about Azure AD B2C, and lucky for us Azure AD B2C has this thing called an Application within it, which can result in some confusion, because everything else we create is also called an application. So... everytime you read word application, I'm talking about the one that belongs to Azure AD B2C. I'll clarify if I mean mobile app or Web API app. Cool? Cool._

There are 4 constituent parts of Azure AD B2C which are essential for authorizing a mobile to access a resource that I want to talk about.

* Tenant
* Policies
* Applications
* Scopes

#### Tenant

This is the overall container - think of it as the Directory for your user objects.

At the top level, it hosts all the applications, users, user attributes, provider configurations (integration to Facebook or Twitter, etc.), and policies.

You can read more about the Tenant in [first](https://msou.co/6s) and [second](https://msou.co/6t) posts in this series.

I won't spend much more time going over Tenants than what's already in those 2 posts... 

But... I want to point out that eventually, when users sign-up for our application, they will be asked to enter information about themselves. We can control which information is available to be asked for via the User Attributes node, as seen here. 

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_800/v1509998434/Screen_Shot_2017-11-06_at_1.51.50_PM_ywdcuf.png)

The Tenant doesn't need to utilize all the user attributes, and we can create custom user attributes within the Tenant as well.

#### Policies

Policies are kind of like endpoints that your mobile app calls to interact with the Azure AD B2C application.

These policies define the experiences that your users will go through to sign up or sign in for your application, and hence performing authentication.

By that I mean, if you want your user to sign in or sign up for your app - you would make sure to somehow invoke a "Sign-up or sign-in policy". 

> One thing to note - it is strongly suggested to use the __Sign-up or sign-in policy__ instead of the distinct __Sign-up policy__ and __Sign-in policy__ that exist within the Tenant. It'll make life much easier to use this combo policy...

For our purposes, there are 3 different types of policies, and their purpose should be evident by their names.

* Sign-up and sign-in policies
* Profile editing policies
* Password reset policies

So here's what's cool about these policies... when you configure one, you get to make several decisions about their behavior! (Because we're developers and we like control!)

1. Which identity providers to allow. (So you could have a sign-in and sign-up policy that only allows Twitter authentication.)
1. For sign-up, you can decide which of the user attributes you want to have collected.
1. You can also then decide which _Claims_ or of __all the user attributes defined in the Tenant__ which should be returned to the mobile app. So it's possible to collect more info during sign-up than the app will have access to in every day usage. And the opposite is also true - you can return more attributes to the app than was requested during the sign-up process. _These user attributes being returned to the mobile app will be done so within a token as a claim. That's important, which is why this is in bold._
1. Whether to turn multi-factor authentication on or off.

There will also be settings to tweak for how long the token lifetime should be and so on.

And it's worth noting that when the mobile app starts the authentication workflow, it will be happening through a webview. So the policies give you the a limited amount of leeway to customize the look and feel of the page that's displayed to fit your branding.

Policies are a top-level object in an Azure AD B2C Tenant - which means they can be shared amongst applications.

#### Applications

So, what in the world does the term application mean in the context of an Azure AD B2C Tenant?

An application exists at the top level in a Tenant and this is the thing that models your mobile app and WebAPI app within the Azure AD B2C Tenant.

Here you provide specifics on what to do when after authorization is requested for a Web API (in other words, where Azure AD B2C should send the access token to).

You also obtain various constants - that you use to communicate with the Azure AD B2C application.

In other words, it's mainly a configuration container for your apps.

The real interesting part though is that it allows you to define something called Scopes, which function as permissions...

#### Scopes

Unlike everything else I talked about up to this point, scopes are not defined at the Tenant level, rather they are a part of the application definition.

A scope is something the mobile application requests from the Azure AD B2C application as part of the authorization process. And it is a permission to the Azure AD B2C application. _But it's not a user-level permission._

The way I think about is like a party ... say there's a scope defined named: "raging-party". At this point in the workflow, the user of the app has already been authenticated...

The mobile app knocks on the door: 

_"Dearest Azure AD B2C application. I humbly request that this user ... which __YOU - Azure AD B2C Service__ have already authenticated for me previously using something like Twitter, and guarantee is who they say they are ... be granted an authorization token with this scope, which goes by the name of raging-party. In other words ... please, please, please can I come in to the party?"_

The Azure AD B2C application - seeing that the scope the mobile app is requesting is legit, will generate an authorization token for that scope.

Essentially the Azure AD B2C application is saying:

_"Hey authenticated user! You kinda talk funny, but we've been waiting for you - join the party - it's hot in here!!"_

If the scope is not defined, the Azure AD B2C application says:

_"Hold on poser - I don't care if you are who you say you are - this party is for ticket holders only."_

Once granted entrance into the party, the Azure AD B2C application returns an authorization token as a welcome gift. The partygoer can then use the authorization token to access the WebAPIs in the same Azure AD B2C application.

But hold the phone!! 

Any mobile application that requests the correct scope (along with the certain secret keys that allow you to invoke the Azure AD B2C application in the first place), as long as the scope is defined in the application - NO MATTER WHO THE USER IS, AS LONG AS THEY'RE AUTHENTICATED - will get full access to the backing Web API. _(Don't freak out here, this can be restricted, but some additional work needs to be done on your part & I'll get to that.)_

So although you can think of scopes as permissions to the Azure AD B2C application - they are only permissions to get in the front door.

In other words - any user of your mobile app who gets into the front door of the party can get into the DJ booth and change the music.

And nobody want to listen to my [favorite song](https://msou.co/62) on repeat the whole night.

The next post in this series, where I cover the code of authorizing a mobile app to access a resource will show a quick technique of locking the DJ booth's door.

## Summary

Alright - in this post we took a look at the constituent parts of what makes authentication and authorization a mobile app using an Azure AD B2C possible.

Tenants are the overall container for everything - including users, user attributes, policies, and applications.

User attributes collect ... attributes ... of the people who will be using your application - and once auth'd will be returned as claims in a token.

Policies are the things your mobile app interacts within in order to gain auth ... so provide things like signing in and signing up.

Applications model your mobile and WebAPI.

Finally, scopes are the keys to the front door of the party.

In the next post - we'll look into obtaining those keys, calling a secured WebAPI from a mobile app ... and locking down the DJ booth!