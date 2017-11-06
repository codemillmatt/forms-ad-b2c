The whole point of [Azure AD B2C](https://msou.co/6l) is to both have a means of providing authentication between a front end client and a back end resource, and to obtain an access token which can be used for authorization purposes.

In this article I'm going to cover some foundational steps before implementing any authentication. Namely I want to talk about how to invoke any web call with Xamarin. After that I'll get into how to setup the basics of the demo app for the solution.

But first, an overview of what the demo app is comprised of.

## The Demo App

For the case of this [blog series](https://msou.co/6k), I'm going to build up a Xamarin.Forms app that both displays and records restaurant reviews. 

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_800/v1509730731/PNG_image-3AF62C84331D-1_lj1hud.png)

The backend will be a .NET core Web API which will be responsible for persisting and returning data to the Xamarin.Forms app.

We also want to be sure each and every user signs up for our app, and then is logged in, in order to see the reviews.

(I know that's probably not the best business model - but for the purpose of these demos it's a _wonderful_ model!)

## But What's This Post About?

Before I get into actually implementing the sign-up and sign-in authentication with Azure AD B2C, I want to take a step back and talk about how to both setup a basic .NET Core Web API and invoke/consume it from a Xamarin.Forms app.

Something that I get asked about a lot when I speak at conferences is, how can Xamarin - in the core project - consume web services? So I really want to devote an entire post to talking about that before moving on to the actual authentication.

I'm going to break this post down into 2 parts - a tl;dr; where you can find out exactly how to call web services, and then a longer part where I'll go through how I set the solution that I'll be using throughout the rest of the series on Azure AD B2C.

## The tl;dr; Calling the Web Service

First off, all of the code for this post can be found in [GitHub here](https://msou.co/6q), so go ahead clone it and have some fun!

Xamarin unleashes the power of the .NET framework for use in both Android and iOS (and Mac, UWP, Tizen, etc.) - so that means we can use the .NET `System.Net.Http.HttpClient` class to invoke and consume web services. We can use `HttpClient` from within the platform projects, but of course it makes more sense to use it from within a core project that's shared across the platforms.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_800/v1509730731/PNG_image-F40AACD262B3-1_kci8q4.png)

If you're not already familiar with `HttpClient` it provides 2 mechanisms to perform web calls.

The first is, what I'll call the "quick mathod".

I call it quick because it generally only involve a single class. There are several methods of `HttpClient` that when passed an address - will go out and get a resource. So, for instance, if we wanted to hit an endpoint via the HTTP Get verb that returned a JSON string, we could use something like the following:

```language-csharp
var baseAddr = new Uri("http://localhost:5000");
var client = new HttpClient{ BaseAddress = baseAddr; }

var returnedJson = await client.GetStringAsync("/api/reviews");

// Do some work with the JSON in returnedJson
```

So in this example - we're able to query `http://localhost:5000/api/reviews` via a GET and then handle a string it returns (which could be any string - but we'll assume JSON in this case). Then we could use Newtonsoft.Json to handle it. (The `HttpClient` also has other functions such as `GetStreamAsync, DeleteAsync, PostAsync, PutAsync`).

However, if we wanted to be more explicit as to what we're up to (and that's probably not a bad thing) - like when setting the headers in the request - then we would have to use 2 other objects - `HttpRequestMessage` and `HttpResponseMessage`.

So, the same request as above would look like the following then:

``` language-csharp
var req = new HttpRequestMessage(HttpMethod.Get, "/api/reviews");

var resp = await client.SendAsync(req);
resp.EnsureSuccessStatusCode();

var allReviews = await resp.Content.ReadAsStringAsync();
```

Here the `HttpRequestMessage` allows us to explicitly set the HTTP verb to use. The `HttpResponseMessage` has a function that makes sure a successful HTTP status code is returned, otherwise it raises an exception.

Using `System.Net.Http` is the way to go when communicating to the web with Xamarin.

Now... I want to give a rundown of how the projects are setup for the rest of the Azure AD B2C blog series.

## Setting up the .NET Core Web API

_One thing to note - at least in this sample - I don't have much exception handling or anything along those lines setup. My main goal is to demonstrate how to invoke a web call and get the foundation of the projects setup... fair warning :)._

I'm not a web developer ...so [ASP.NET Core](https://msou.co/6n) is (was) a complete mystery to me. (So this is my way of saying ... if I get something wrong here - let me know!)

But I want to get it down in writing so everybody can follow along. Of course, what better way to follow along than to grab the code from [GitHub](https://msou.co/6q)

### Creating the project

I used [VS Code on a Mac to create the Web API](https://msou.co/6m) - which means I am also using the .NET CLI to create the projects.

In order to create a new Web API project, navigate to a folder where you want the project to live, open up the terminal, and issue the following:

```language-bash
dotnet new webapi -n "NAME OF YOUR WEB API PROJECT"
dotnet restore
dotnet run

```

The first command will create the Web API project will all of the necessary files. 

The second command will restore all the NuGet packages.

And after issuing the `dotnet run` command a web server will start up and you will be able to browse to `http://localhost:5000/api/values` and should see some JSON returned.

### Creating the Reviews Controller

The next step is to create a controller that will return the reviews of various restaurants to the Xamarin.Forms app.

First thing first - we need to create a class that will model the review. So create a folder called `Models` and then a class called `Review.cs`.

Name the class `Review`, pop it into an appropriate namespace, and put in a single property - name it `Text`.

Next, create the actual Reviews controller. Create a new file under the Controllers directory. Name it `ReviewsController.cs` and it should look like the following:

```language-csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace backend.Controllers
{
    [Route("/api/[Controller]")]
    public class ReviewsController : Controller
    {
        [HttpGet]
        public IEnumerable<Review> GetAllReviews()
        {
            var allReviews = new List<Review> {
                new Review { Text = "Yum" },
                new Review { Text = "Meh" }
            };

            return allReviews;
        }
    }
}
```

The key here is to make sure the class inherits from `Controller` and then the `Route` attribute - which tells the runtime the URL which this controller gets invoked.

That's it ... issue `dotnet run` in the terminal again, and then browse to `http://localhost:5000/api/reviews` and you should see a JSON array come back with 2 records - one that says "Yum" and one that says "Meh".

Cool.

## The Xamarin.Forms Project

Finally ... at the Xamarin.Forms project and invoking web services (well, that is if you skipped the tl;dr; from above). (The [GitHub](https://msou.co/6q) code...)

Easy part, create a Xamarin.Forms solution. Semi, easy part - convert it to a .netstandard core project.

As of right now - the way I go about doing that is adding a .NET Standard library to the project, then add all of the files into that library - add the references from the platform projects ... make sure everything compiles - you get the picture.

Add a NuGet package to all the projects `Install-Package Refractored.MvvmHelpers -Version 1.3.0` (I love this package, it adds many classes that make MVVM development _much_ easier.)

After that, create a new folder `Models`, then add a `Review.cs` file that is going to look exactly like the model class you created above for the ASP.NET Web API. (Now, we could go into code sharing between the 2 solutions, but for now, we'll keep them separate).

Then, create a `ContentPage`, call it `ReviewListPage.xaml` and to it, add `ListView`. (I'll get to wiring everything up in a bit.)

Finally, the good stuff - create a `ViewModels` folder and a new class called `ReviewListViewModel.cs`. Make sure it inherits from `BaseViewModel` (which comes from the MVVMHelpers NuGet) ... and create a new `Command` call it `RefreshCommand` and implement the following:

```language-csharp
public ObservableRangeCollection<Review> AllReviews { get; set; } = new ObservableRangeCollection<Review>();

Command _refreshCommand;
public Command RefreshCommand => _refreshCommand ??
(_refreshCommand = new Command(async () =>
{
    var baseAddr = new Uri("http://localhost:5000");
    var client = new HttpClient { BaseAddress = baseAddr };

    var reviews = await client.GetStringAsync("/api/values");

    var allReviews = JsonConvert.DeserializeObject<List<Review>>(reviews);

    AllReviews.AddRange(allReviews);
}));
```

Of course - there'a property in there as well, `AllReviews`.

Finally, we'll need to hook up the view model to the view and do the bindings.

The long story here is the `ListView.ItemSource` should be bound to the view model's `AllReviews` property and `TextCell` including within should be bound to the `Text` property of the `Review` class which composes the `AllReviews` list.

Within the constructor, call the `RefreschCommand.Execute` function, so it loads data immediately - and that's all there is to it.

## Summary

We went through a lot here, and most of it was related to project setup.

But the big, big point of all this is that you can invoke web services fairly easily by using `System.Net.Http.HttpClient`.

And there are 2 ways you can get data ... and quick and easy way by using functions directly off of `HttpClient` and a way that provides more functionality by using `HttpRequest` and `HttpResponse` in combinatino with `HttpClient`.

In the next post, I'll take a look at getting the Xamarin.Forms app to use Azure AD B2C to authenticate with Azure, and then use that authentication token to retrieve data from a "locked down" ASP.NET Web API.