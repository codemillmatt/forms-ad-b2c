Serverless computing is hot, hot, hot! And for good reason - it gives us a way to create focused and stateless microservices that solve exactly one problem without having to create a monolithic application and maintain a server.

In other words, they are perfect in a mobile app scenario! 

# Adding Azure AD B2C Authentication to Azure Functions

Azure's serverless offering is called [Azure Functions](https://msou.co/baj) and one way to invoke them is via [HTTP requests](https://msou.co/bai). Since these functions will be open to the web at large, we'll eventually have a need to require a calling user be authorized in order to invoke them.

And that's what this article is all about ... adding authentication and authorization to Azure Functions via [Azure AD B2C](https://msou.co/bak).

In this article we'll walk through the following:

- The steps necessary to create an Azure Function App that returns a simple JSON array
- Invoking that function via an HTTP request
- Configuring the Azure AD B2C application within the Tenant to provide authentication and authorization to that function.
- Configuring the Azure Function App to use Authentication and Authorization from Azure AD B2C
- Then finally some minor changes to our Xamarin.Forms application to invoke the Function which requires authorization.

And it should be noted that the technique discussed in this article will work for any of the Azure App Service offerings: [Web Apps](https://msou.co/bal), [Mobile Apps](https://msou.co/bam), [API App](https://msou.co/bal)...

First thing first then - we need a Function!

## Creating the Function App

There's going to be 2 parts to creating the Function App ... the first part will be the actual scaffolding of it within the portal, and the second will be the code.

### Scaffolding the App Within the Portal

Go into the portal, click _Create a Resource_ then find _Function Apps_ and hit new.

LOL, right ... that's missing some steps right?

The easiest way is to follow the [great documentation here](https://msou.co/bai). These docs will show you how to create an HTTP Function within the portal - and that's exactly what we want!

### The Function App Code

The function that we'll be working with is simple - it will do the same thing as the [ASP.NET Core Web API did](https://msou.co/61) - return reviews to be displayed in our Xamarin.Forms mobile app.

Right now everything is in _demo-mode_ so the `Review` class on the server looks like the following:

```language-csharp
public class Review
{
    public string Text { get; set; }
}
```

And the function that returns the reviews looks like:

```language-csharp
#load ".\review.csx"
#r "Newtonsoft.Json"

using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("All reviews started.");

    var allReviews = new List<Review>();
    allReviews.Add(new Review { Text = "Good" });
    allReviews.Add(new Review { Text = "Bad" });

    var json = JsonConvert.SerializeObject(allReviews);

    var res = req.CreateResponse(HttpStatusCode.OK, json);

    return res;
}
```

Don't worry if you don't quite understand all of the specifics - the gist of it is the function is returning a JSON representation of 2 `Review` objects, one says __Good__ the other __Bad__.

### How Do I Invoke This?

This gets invoked via an HTTP request. The easiest way to find the URL at which to invoke it is to go into the function definition in the portal:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1513199352/Screen_Shot_2017-12-13_at_3.07.09_PM_bnttp3.png)

And click on the _</>Get function URL_ button in the upper left.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000/v1513199510/Screen_Shot_2017-12-13_at_3.11.25_PM_raq0uu.png)

You can then issue a __GET__ request against it in Postman (or a similar utility) to see the return value.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_800/v1513199664/Screen_Shot_2017-12-13_at_3.14.08_PM_vu4wob.png)

But that's wide open to anybody on the Internet to get at - so let's lock it down so only people within our Azure AD B2C Tenant can access it.

## Configuring the Azure AD B2C Application

In the previous post on [adding authentication to a Xamarin.Forms app](https://msou.co/8b) we walked through the steps necessary to create an Azure AD B2C Application within a Tenant.

We're going to use that same Azure AD B2C Application here, this time adding in our newly created Function App as another client to it.

### Adding the Function App as a Client

The first thing to do is get the overall URL for the Function app. That's going to be on the main blade on of the Function app, under - you guessed it - _URL_.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1513200629/Screen_Shot_2017-12-13_at_3.29.37_PM_zhkiti.png)

Copy that value and then open up the Azure AD B2C Application (remember, it'll be in a different directory, so you'll need to click on your name in the upper left to get a dropdown to appear).

Once the Application is open, toggle the _Include Web App / Web API_ switch to __Yes__. That will bring up a screen with some blank text boxes (in addition to the ones we already filled out for the native/Xamarin client).

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1513200963/Screen_Shot_2017-12-13_at_3.35.29_PM_qkie5e.png)

Make sure the _Allow Implicit Flow_ is set to __Yes__.

Then in the __Reply URL__ field, the value will be of the following format:

```language-csharp
{YOUR-FUNCTION-APP-URL}/.auth/login/aad/callback
```

> Make sure the URL you enter is HTTPS!

This is letting the Azure AD B2C Application know what URL to send authorization tokens to after authenticating users.

That's all there is to enabling the Azure AD B2C Application to communicate with the Azure Function App.

The next thing we need to do is setup the Function App to communicate with the Azure AD B2C App.

## Configuring the Azure Function App for Azure AD B2C Authentication

Back in the Azure portal directory that contains the Function App, open up the App you want to add authentication to, and select the _Platform features_ tab from across the top.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_700/v1513201530/Screen_Shot_2017-12-13_at_3.45.09_PM_fw8lai.png)

Then select _Authentication and Authorization_ underneath the _Networking_ heading.

Initially it will tell you Anonymous Authentication is enabled - change that by changing the switch under _App Service Authentication_ to __On__.

Then a whole new slew of options will become available.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_800/v1513201717/Screen_Shot_2017-12-13_at_3.48.14_PM_re8q1j.png)

First thing, change the value of the _Action to take when the request is not authenticated_ dropdown to __Login with Azure Active Directory__.

This will then force all unauthenticated requests to follow our _Sign-up or sign-in policy_ that we created back the [previous post](https://msou.co/8b) where we initially added authentication to the Xamarin.Forms app.

Next click on _Azure Active Directory (Not Configured)_ button - which will bring up the following screen.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000/v1513201953/Screen_Shot_2017-12-13_at_3.52.18_PM_mr0woq.png)

Change the _Management Mode_ to __Advanced__. Two new fields will then appear, _Client ID_ and _Issuer Url_.

The _Client ID_ field is the Azure AD B2C Application's ID (so you need to go back into there and grab that ID from the Application blade).

The _Issuer Url_ is obtained via the policy we're invoking during the sign-in process.

That's also in the Azure AD B2C Tenant. Select _Sign-up or sign-in policies_. Then select the policy you have tied to the Application's API Access (again from that [previous post](https://msou.co/8b)).

There you will see a URL at the top of the blade.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/bo_2px_solid_rgb:000000,c_scale,h_800/v1513202331/Screen_Shot_2017-12-13_at_3.58.34_PM_kujvg4.png)

Copy that link - it goes into the _Issuer Url_ on the Azure Function side of things.

Once you have the _Client ID_ and the _Issuer Url_ filled-in, you can click __OK__. It should then tell you that Azure Active Directory has been configured. Click __Save__ at the top of that blade, and now the App Function requires authentication before it can be invoked.

Try it by issuing the same GET request in Postman. You should receive a 401 error.

## Xamarin.Forms Changes

There are very few changes to our code necessary!

Since we're still authenticating to the same Azure AD B2C using MSAL, we do not need to change any of the calls to `AcquireTokenSilentAsync` or `AcquireTokenAsync` - those still return exactly what we need - and that's the `AccessToken`.

In fact, the only change to the code is updating the URL of where the `HttpClient` performs its GET operation against. 

The entire code, from calling Azure AD B2C through the Azure Function is shown below:

```language-csharp
var authResult = await App.AuthClient.AcquireTokenAsync(App.Scopes,
    GetUserByPolicy(App.AuthClient.Users, App.SignUpAndInPolicy),
    App.UiParent);

location = "https://thereviewer-func.azurewebsites.net/";

var baseAddr = new Uri(location);
var client = new HttpClient { BaseAddress = baseAddr };

// reviewUri the full address to the function needing to be invoked
var reviewUri = new Uri(baseAddr, "api/allreviews?code=cFyxMO/bL07meVmBbGyxpifzuwHtJyWdj7BhYxWEmI3NpED5nYqEKw=="); 

var request = new HttpRequestMessage(HttpMethod.Get, reviewUri);

// Authorization header is a bearer token with the AccessToken value
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

// Call the function
var response = await client.SendAsync(request);
response.EnsureSuccessStatusCode();

// Read the data out
var reviewJson = await response.Content.ReadAsStringAsync();

var allReviews = JsonConvert.DeserializeObject<List<Review>>(reviewJson);
```

And with that our app should now be displaying the reviews - but this time obtained via the Azure Function!

## Conclusion

A lot of portal configuration again in this article!

The main key here is that we added a _Web App / Web API_ client to the Azure AD B2C application we previously created to model that the Azure Function will be receiving information from the Azure AD B2C Application as well.

The URL entered into the _Reply Url_ field is the Azure Function main URL plus `/.auth/login/aad/callback` signifying where the Azure AD B2C application should send its info.

The Azure Function App can be configured to use B2C by turning off anonymous authentication and then entering the Azure AD B2C Application ID and the sign-in and sign-up policy URL into its configuration.

And then the Xamarin.Forms app needs the least amount of changing! All we need to do there is change the URL it's pointing at, and everything will _just_ work!