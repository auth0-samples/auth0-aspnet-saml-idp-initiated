# Auth0 + ASP.NET MVC4 Plus SAML IDP Initiated
This is an example of an app that tests state and therefore needs a special redirect for SAML connections

# Pre-requirements
In order to run the example you need to have Visual Studio 2013/2015 installed.

Install package Auth0.ASPNET via Package Manager or running the following command:

```Powershell
Install-Package Auth0-ASPNET
```

# Create SAMLP Enterprise Connection
[Example for configuring a SAMLP Enterprise Connection](https://auth0.com/docs/protocols/saml/saml-configuration/auth0-as-service-provider)
[Example if you need Auth0 to be the SAML IDP as well](https://auth0.com/docs/protocols/saml/saml-configuration/auth0-as-identity-and-service-provider)

Don't forget to set the IDP-Initiated section of your SAML connection to route to your client for this app, as well as setting `redirect_uri=http://app.local:4987/PartnerSso.ashx` in the query params.

# Setup Local environment
You also need to set the ClientSecret and ClientId of your Auth0 app in the `web.config` file. To do that just find the following lines and modify accordingly:
```CSharp
<add key="auth0:ClientId" value="YOUR-CLIENT-ID" />
<add key="auth0:ClientSecret" value="YOUR-CLIENT-SECRET" />
<add key="auth0:Domain" value="YOUR-TENANT.auth0.com" />
<add key="auth0:Saml" value="YOUR-TENANT-SAML-NAME" />
```

You must add this entry to your `C:\Windows\System32\drivers\etc\hosts` file: `127.0.0.1 app.local`. If you skip this step, you will end up failing to SSO for your SAML users because Auth0 will require consent.

# Configure your Auth0 Client
Don't forget to add `http://app.local:4987/LoginCallback.ashx` and `http://app.local:4987/PartnerSso.ashx` as **Allowed Callback URLs** 
Don't forget to add `http://app.local:4987/` as **Allowed Logout URLs** in your Advanced Tenant Settings. [Instructions](https://auth0.com/docs/logout#set-the-allowed-logout-urls-at-the-tenant-level)

# Run the application
After that just press **F5** to run the application. It will start running in port **4987**. Navigate to [http://localhost:4987/](http://localhost:4987/).

If you setup your SAML IDP in Auth0 as well, to test IDP Initiated go to `https://YOUR-IDP-TENANT.auth0.com/samlp/SAML-ADDON-CLIENTID`

**Note:** You can change CallbackURL in `Views/Shared/_Layout.cshtml` on the line 53.