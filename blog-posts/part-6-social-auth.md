In the sixth chapter of the using mobile apps with Azure AD B2C series - we're going to add in social authentication!

# Social Authentication in Azure AD B2C

If you've been following along with this series you should have a ASP.NET Core Web API app which requires authorization in order to return data and a Xamarin.Forms app using the MSAL library. And if you've really been following along - the Forms app should be able to retrieve data from a protected endpoint in the Web API via logging with a email and password.

But having to create a brand new account linked to an username and password to use an app can be a pain - and actually is a deterent for people to use an app. Having the ability to create an account via an already existing account - such as Twitter or Facebook - lowers the barrier of entry. Not only does it let a third party have to worry about taking care of the username and passwords - it allows the user the ease of not having to create yet another password.

So let's look at the steps necessary to enable social authentication within our existing Azure AD B2C app.

It's actually pretty simple, only 2 parts, and no code changes! All we need to do is configure the third party (here I'll look at Twitter) and then hook that up to the Azure AD B2C portal.

So let's have at it!

## Twitter Configuration

You can think of Azure AD B2C as providing an abstraction layer between your app and the various social networks. It takes care of the actual communication to the third party - such as Twitter. 

In order to make this all work, all we have to do is tell Twitter we have an app we want to integrate with Twitter (and as a side-effect use it as an identity provider), and then hook up some IDs that Twitter gives us to the Azure portal, so those two can talk.

First things first then - we need to tell Twitter we have an app that we want to integrate with it.

Hop on over to (apps.twitter.com)[https://msou.co/7x] and you will see a page that looks something like this:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513018295/social-auth-1_cww9kt.png)

You'll want to click the __Create New App__ button. The following screen will then appear:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513018428/social-auth-2_xsotwe.png)

Here give it a name in the __Name__ field - and it's important to note that whatever name you give it will later be displayed by Twitter when it asks the user to login.

Fill out the __Description__ and __Website__ to some appropriate values.

Then the __Callback URL__ field needs to be of the following format:

```language-csharp
https://login.microsoftonline.com/te/{TENANT NAME}/oauth2/authresp
```

The __Tenant Name__ value can be found back in the Azure portal, under the _Overview_ blade, under _Domain Name_.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1513019222/Screen_Shot_2017-12-11_at_1.04.59_PM_dylohy.png)

After you hit the __Create your Twitter application__ a new page appears with tabs across the top that you can use to configure your app.

The 2 tabs we're initially interested in are _Keys and Access Tokens_ and _Permissions_.

For now, let's look at _Permissions_.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513019846/social-auth-4_aqghan.png)

This is where we're telling Twitter what our app is going to be able to do to the user's account. Because we're only after identity providing - __Read only__ will more than suffice.

That's all there is to provisioning an app for use with Twitter. Now let's make that work with the Azure AD B2C portal.

## Azure AD B2C - Social Portal Configuration

Here is where we perform some additions to integrate the Twitter provisioning we did above into our Azure AD B2C Tenant.

### Adding the Identity Provider

First up, select the __Identity Providers__ option from the left-hand side, and the following window will open.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513020512/social-auth-5_ls3rtn.png)

Notice how there are no _Social identity providers_ listed yet. Go ahead and click __Add__ and you'll see this screen.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513021512/social-auth-6_kpnhq7.png)

You have your pick of providers ... take Twitter. Then select _Set up this identity provider_. There you're going to see 2 boxes, __Client ID__ and __Client Secret__.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513022210/social-auth-7_dri02y.png)

Where do those 2 values come from?

Back in the Twitter portal, under the _Keys and Access Tokens_ tab, there is a __Consumer Key (API Key)__ and __Consumer Secret (API Secret)__.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513022349/social-auth-3_isjegy.png)

Those values map from the Twitter portal to the Azure portal as:

- Consumer Key = Client ID
- Consumer Secret = Client Secret

So then we can fill out the rest of what's needed and save. And then we'll see that there is a value in the _Social identity providers_ section.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513022975/social-auth-8_kgqrro.png)

### Updating the Policy

Associating a Twitter application with the Tenant isn't enough to actually enable the social authentication. We need to update the policy that our mobile app is invoking so it allows the social authentication.

Select __Sign-up or sign-in policies__ from the left-hand side to view the various policies created that allow signing up or signing in (there should only be )one).

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023145/Screen_Shot_2017-12-11_at_2.10.42_PM_mtfvec.png)

Select the one you want to add the Twitter authentication to, and you should see the screen like the one below.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023212/social-auth-9_ejnw3y.png)

Click the __Edit__ button.

A new blade will open that will allow you to update many options of the policy, we want the __Identity providers__.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023256/social-auth-10_fjtbux.png)

Clicking on that will show all of the identity providers configured within the Tenant. Go ahead and select the newly created one for Twitter.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/v1513023358/social-auth-11_ll93li.png)

Save everything on your way out - and that's all there is to enabling Azure AD B2C to use a social service as an identity provider!!

Now let's see it in action.

## Using It

As I mentioned - there are no code changes necessary to use social authentication when using the MSAL with a Xamarin app. Everything *just* works. (And a part of the reason why it just works is because we are running in a webview during the sign-in/up process, so it's able to retrieve the info on the fly.)

So here's what the various screens look like.

### The Sign-In:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023614/social-auth-12_bzke58.png)

Here if the user does not have an account, and they want to sign-in with Twitter, they should still hit the Twitter button. MSAL will create an account for them. The default webview screen isn't very clear here.

### The Twitter Login:

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023854/social-auth-13_gf7tvb.png)

Here the user is presented with a web page from Twitter. It has the name of the app, as was entered on the Twitter Apps site, the company's URL also entered there, and also shows the permissions the app is asking for.

### 2-Factor Auth

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513023951/social-auth-14_gbj9gw.png)

If you have Twitter 2-Factor authentication enabled, you'll get prompted here as well. It's a full sign-in.

### Double Make Sure 

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513024023/social-auth-15_spikel.png)

It's a full sign-in, so Twitter is double making sure you want to give access to the app.

### Collect Additional Info

![](http://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513024100/social-auth-16_w0bt83.png)

Remember when we defined the _Sign-in or Sign-up_ policy, we also defined some user attributes we wanted the application to collect?

Well, you don't have to remember - cuz I helpfully attached a screenshot for you! :)

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513024231/Screen_Shot_2017-12-11_at_2.29.13_PM_r4rn8o.png)

So we told Azure AD B2C to collect a __Display Name__, an __Email Address__, and a __Job Title__.

Well, the __Display Name__ is going to come from Twitter (it's going to be the user's handle). So now Azure AD B2C is prompting the user for the other 2 attributes we said we need.

### All Done!

Now we have all the info and the app allows the user to proceed and download data.

In fact, we can even see that the new user has been added to the backing Active Directory by finding the Active Directory in the _All Resources_ panel of the Azure Portal and selecting it.

Select __Users and Groups__ -> __Users__ and you'll see a full roster of users.

![](https://res.cloudinary.com/code-mill-technologies-inc/image/upload/c_scale,h_600/v1513024483/Screen_Shot_2017-12-11_at_2.33.44_PM_kxgkvx.png)

And sure enough (@codemillmatt)[https://twitter.com/codemillmatt] has been added as a user.

## Conclusion

Adding in social authentication to our Azure AD B2C application isn't difficult at all. All it involves is creating an application out on the various social networks, and then associating that new social app within the Azure AD B2C portal.

Very soon I will have some video walk throughs of creating authentication on all of the other networks that Azure AD B2C supports.

Then in the next post, we'll take a look at some methods that can be used to "lock down" various API endpoints, so not just anybody who has signed-up can access them.