Finally - we're at the point in this series where I show how to use Azure AD B2C to authenticate a user and then authorize that user to hit a Web API from a Xamarin.Forms application!

In other words - we're at the post with the official, and long, title of...

# Authenticating and Authorizing a Mobile App to Use a Web API via Azure AD B2C

In the first couple of posts, we learned what [Azure AD B2C is](https://msou.co/6z), how to [create a Tenant](https://msou.co/60) (which I found a bit tricky), then took a quick detour to find out how to [invoke a Web API](https://msou.co/61) from a Xamarin.Forms app - because that's going to be our backing service which will be "protected". Then in the [final post before this one](https://msou.co/7j), we learned what everything is inside of Azure AD B2C... and how it relates together. And the info from that post is going to come in very handy here!

In order to get this all to work, there are 3 parts we have to go through.

1. Configure our Azure AD B2C tenant and application within portal.
1. Modify the WebAPI application to only return data if it receives an authorization token.
1. Modify the Xamarin.Forms app to request the token from Azure AD B2C and then send the authorization token on to the Web API.

Actually, there's a fourth part - and that's to down the beverage of your choice - possibly through a funnel.

I say that because this post is going to be long - and a bit dry. It will be a step-by-step guide on getting everything setup and then finally making an authorization to a backend resource.

A lot of these steps are "set 'em and forget 'em" ... so they're important to go over because ... well ... they're easy to forget.

## Authentication and Authorization Flow

The drawing below illustrates the process the mobile app, Azure AD B2C, and the Web API take in order to provide access to a secured method on the Web API controller.

## Step 1 - Creating a Policy

We need to do some work in the portal. I'm going to go relatively fast through all of this as all of the concepts used here was explained in the [previous post](https://msou.co/7j).

First up - log in to the portal, and select the appropriate directory from the dropdown in the upper left corner under your name:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512490151/Screen_Shot_2017-12-05_at_10.08.21_AM_raaiwg.png)

Once there, select the Azure AD B2C option from the menu on the far left side:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512490254/Screen_Shot_2017-12-05_at_10.10.29_AM_x2icov.png)

First off we need to create a policy for the Azure AD B2C Tenant.

Select __Sign-up or sign-in policies__ from the left-hand menu.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512491809/Screen_Shot_2017-12-05_at_10.36.28_AM_lv7ga7.png)

Then click __Add__ in the blade that comes up.

Here you're going to be able to configure quite a few options for the new policy.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512491917/Screen_Shot_2017-12-05_at_10.38.19_AM_vwj7wo.png)

Give it a name you'll remember ... I chose _Generic Sign-up and Sign-In_.

Then under __Identity Providers__, there will only be one option __Email signup__... select that.

Email signup allows a user to sign up and sign in using their email address and a password of their choosing. (Eventually we'll add in some social authentication.)

The __Sign-up attributes__ declare what fields you want to have collected when a user registers for the app. Choose what you will.

The __Application claims__ determines which of the __Sign-up attributes__ values will be returned to the mobile app _after_ the user signs-in.

Go ahead and click __Create__.

## Step 2 - Setting Up The Azure AD B2C Application

Go back to the overall Azure AD B2C blade ...  then select the __Applications__ menu. Then click __Add__ from the new blade that appears.

Here you're going to be able to give your new Azure AD B2C application a name - and to specify whether it should contain a Web API and Native client. You want to do both.

Then as you select __Yes__ to those options - a whole bunch of new options are going to appear ... so let's take a look at those one at a time.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512490746/Screen_Shot_2017-12-05_at_10.18.44_AM_ciygoj.png)

### Web API Configuration

The __Reply Url__ under the __Web API__ section specifies the web address that you want a "cool / not cool" reply to go to from Azure AD B2C when the Web API asks if it's OK to allow an HTTP request through.

This is going to be the _real_ URL the endpoint which the Xamarin.Forms app will invoke resides at.

As you can see - we can specify more than 1 address - but for the purpose of this demo, only a single one is needed.

Next up is the __App ID URI__ ... it says it's optional. Fill it out - you're going to need it. It doesn't need to resolve to anything - but make it unique.

### Native Client Configuration

Leave the default values as is. We'll fill out the the __Custom Redirect URI__ a bit later.

Click the __Create__ button.

Now when it creates, the Azure AD B2C application property's blade will open. Within there you'll see an __Application ID__ box. Copy that value to the clipboard.

Then go down to the __Custom Redirect URL__ box under the __Native Client__ section and paste it in - in the following format:

`msal{APPLICATION-ID}://auth`

Weird right? Yeah... but you're going to need it in a bit.

### Scopes!

Now we want to add a scope to the Azure AD B2C application.

Hit the __Published scopes (Preview)__ menu option. Then in the new blade enter anything you want - but make it descriptive so you know what it means. Remember, the idea behind scopes is to give _permission_ to the backend resource that's being protected.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1512492882/Screen_Shot_2017-12-05_at_10.54.26_AM_oq2fok.png)

### API Access

Now to enable API Access. Hit the __API Access (Preview)__ menu option right above the scopes one, click __Add__, and then the available API should be your Azure AD B2C name.

You should also see the scope you just created in it as well.

We are done configuring the portal!! 🎉

## Changes to the Web API

What we're going to do is make the WebAPI make use of Azure AD B2C. So we're going to have to do some refactoring to the Web API that was created [before](https://msou.co/61).

### AppSettings

We need to first update the appsettings.json - only to avoid hardcoding constants.

The new portion of the file will look like:

```language-javascript
"Authentication": {
    "AzureAd": {
      "Tenant": "TheReviewer.onmicrosoft.com",
      "ClientId": "978f6a35-db30-44fd-8544-b7cc40466adc",
      "Policy": "B2C_1_GenericSignUpAndIn"
    }
  }
```

The __Tenant__ comes from the main blade of the Azure AD B2C tenant. The __Client ID__ comes from the application's page we just created. And the policy is, of course, the policy we created above.

### Startup.cs

Ok ... now we want to make sure that we configure the WebAPI startup to handle authentication using JWT bearer tokens.

The first part in this is to set the `Configure` function to look like the following:

```language-csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseAuthentication();
    app.UseMvc();
}
```

Notice the use of `app.UseAuthentication()`.

Next up ... we're going do a lot of changes to the `ConfigureServices` function. It's going to end up looking like the following:

```language-csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(options => 
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options => {
        options.RequireHttpsMetadata = false;
        options.Audience = Configuration["Authentication:AzureAd:ClientId"];
        options.Events = new JwtBearerEvents 
        {
            OnAuthenticationFailed = AuthenticationFailed
        };
        var authorityBase = string.Format("https://login.microsoftonline.com/tfp/{0}/",Configuration["Authentication:AzureAd:Tenant"]);
    
        options.Authority = string.Format("{0}{1}/v2.0/",authorityBase,Configuration["Authentication:AzureAd:Policy"]);

    });
    services.AddMvc();
}
```

The big thing here is that we're taking that `IServiceCollection` which gets passed in and then calling `AddAuthentication` on it - telling it we want to use `JwtBearer` tokens.

Then we go ahead and configure those JWT tokens. Notice the `Audience` is the `ClientId` of the Azure AD B2C application from the app settings file. And there's an `Authority` property that is set to a long URL, that even includes the policy name.

### Updating the Controller

Next we need to update the controller to make sure our app sends along an authorization token whenever it tries to call the `GetAllReviews` endpoint.

First thing that needs to be done is import the `Microsoft.AspNetCore.Authorization` namespace.

Next - use the `[Authorize]` attribute right above the function definition.

This way the definition will look something like the following:

```language-csharp
[HttpGet]
[Authorize]
public IEnumerable<Review> GetAllReviews()
{
```

Now we're all setup. If we had the mobile app try to invoke the Web API right now - it would receive a 401 - Unauthorized status code.

Of course - you're going to want to deploy the app to Azure now.

## Updating the Xamarin.Forms App

We're in the homestretch now!!

We're going to use the [Microsoft.Identity.Client](https://msou.co/7k) NuGet package (or MSAL) to take care of communicating to Azure AD B2C (and caching the tokens in respsonse) for us. This removes a lot of work on our end. (The package is in preview - but the team is supporting it, and says you can use it in production environments).

Add that package to the platform projects and to the core Forms project.

### Core Project Basic Setup

For this initial stage, I'm going to brute force my way into getting everything working. That means I'm going to have some hardcoded static strings and static objects. Eventually I'll refactor all of this into a proper Login Service - but for now I only want to get the thing working.

So - for some basic setup - in the `App.cs` file - I'm going to add the following strings:

```language-csharp
public static string Tenant = "TheReviewer.onmicrosoft.com";
public static string ClientID = "978f6a35-db30-44fd-8544-b7cc40466adc";
public static string SignUpAndInPolicy = "B2C_1_GenericSignUpAndIn";

public static string[] Scopes = new string[] { "https://TheReviewer.onmicrosoft.com/backend/rvw.read.only" };
public static string ApiEndpoint = "http://thereviewer.azurewebsites.net/api/reviews";

public static string AuthorityBase = $"https://login.microsoftonline.com/tfp/{Tenant}/";
public static string Authority = $"{AuthorityBase}{SignUpAndInPolicy}";
```

These function as configuration settings. The `Tenant`, `ClientID`, and `SignUpAndInPolicy` values should be evident where they came from.

The `ApiEndpoint` is the resource we want the mobile app to invoke.

Finally - the `Scopes[]` array. This is going to be the __App ID URI__ from the __Web API__ configuration in the portal (it said it was optional - but I said we would eventually need it - and now's the time)! And it's followed by the scope name that we want to invoke.

Then there are 2 object's that we want to create public static variables for:

```language-csharp
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
```

The `PublicClientApplication` object is what facilitates the communication to Azure AD B2C. And in the constructor, you can see how we're using the configuration variables to intialize various properties. (And look at the `RedirectUri` - looks familiar, eh?)

While the `UIParent` is a class which is used by Android ... which I'll get to in a second...

AD B2C shows the login portion of its workflow within a web view. That means when the web view is dismissed, it will need to communicate back to the platform project. And that means ... there's some platform specific work we need to do.

### iOS Specific Steps

Over in iOS land - we're going to have to edit the Info.plist file to add a URL type to define a callback URL that gets invoked when the web view is dismissed.

The added section looks like this:

```language-xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleTypeRole</key>
        <string>Editor</string>
        <key>CFBundleURLName</key>
        <string>com.codemilltech.TheReviewer</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>msal978f6a35-db30-44fd-8544-b7cc40466adc</string>
        </array>
    </dict>
</array>
```
The __CFBundleURLName__ is your app's bundle name. The __CFBundleUrlSchemes__ is obtained from the Azure AD B2C application's blade under the native client section. It's the __Custom Redirect URI__ minus the `://auth` at the end.

Then we need to override the `OpenUrl` function in the `AppDelegate`. It's pretty straight forward and will look like this:

```language-csharp
public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
{
    AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);

    return true;
}
```

The `AuthenticationContinuationHelper` is from the MSAL library, and it's there to help us coordinate the authentication flow.

### Android Specific Steps

In the Android app's `MainActivity`, we need to set that `UIParent` property. That's going to be done in the `OnCreate` function and will look like this:

```language-csharp
App.UiParent = new UIParent(Xamarin.Forms.Forms.Context as Activity);
```

This `App.UiParent` allows the MSAL to show the web view using the current Android activity.

### Sending Requests to Azure AD B2C

Finally ... finally ... FINALLY!!!! We're at the moment of actually invoking the Azure AD B2C to authenticate & then authorize to a service.

