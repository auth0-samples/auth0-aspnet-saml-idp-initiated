namespace aspnet4_sample1
{
    using System;
    using System.Threading.Tasks;
    using Auth0.AuthenticationApi;
    using Auth0.AuthenticationApi.Models;
    using Auth0.AspNet;
    using System.Collections.Generic;
    using System.Configuration;
    using System.IdentityModel.Services;
    using System.Web;
    using Auth;
    using System.Web.SessionState;

    public class LoginCallback : HttpTaskAsyncHandler, IRequiresSessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            AuthenticationApiClient client = new AuthenticationApiClient(
                new Uri(string.Format("https://{0}", ConfigurationManager.AppSettings["auth0:Domain"])));

            // IMPORTANT: First Validate the state!
            var stateParam = context.Request.QueryString["state"];
            var state = AuthHelper.State.Validate(stateParam);
            var code = context.Request.QueryString["code"];

            if (state == null) {
                if (code != null)
                {
                    // Bad State, but good code, may be something nefarious
                    // Force a logout here
                    var baseUrl = context.Request.Url.Scheme + "://" + context.Request.Url.Authority + context.Request.ApplicationPath.TrimEnd('/') + "/";
                    context.Response.Redirect(AuthHelper.Logout(baseUrl));
                    return;
                }
                // Error out if state is no good, or not present
                context.Response.Redirect("/error" + context.Request.Url.Query);
                return;
            }

            var error = context.Request.QueryString["error"];
            if (error != null)
            {
                if (error == "login_required")
                {
                    // Redirect to authorize here and end early
                    // IMPORTANT: force prompt=login here!
                    context.Response.Redirect(AuthHelper.BuildAuthorizeUrl(AuthHelper.Prompt.login, false, state));
                    return;
                } else
                {
                    // Some other error
                    context.Response.Redirect("/error" + context.Request.Url.Query);
                    return;
                }
            }

            var token = await client.GetTokenAsync(new AuthorizationCodeTokenRequest
            {
                ClientId = ConfigurationManager.AppSettings["auth0:ClientId"],
                ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"],
                Code = code,
                RedirectUri = context.Request.Url.ToString()
            });

            var profile = await client.GetUserInfoAsync(token.AccessToken);

            var user = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("name", profile.FullName ?? profile.PreferredUsername ?? profile.Email),
                new KeyValuePair<string, object>("email", profile.Email),
                new KeyValuePair<string, object>("family_name", profile.LastName),
                new KeyValuePair<string, object>("given_name", profile.FirstName),
                new KeyValuePair<string, object>("nickname", profile.NickName),
                new KeyValuePair<string, object>("picture", profile.Picture),
                new KeyValuePair<string, object>("user_id", profile.UserId),
                new KeyValuePair<string, object>("id_token", token.IdToken),
                new KeyValuePair<string, object>("access_token", token.AccessToken),
                new KeyValuePair<string, object>("refresh_token", token.RefreshToken)
            };

            // NOTE: Uncomment the following code in order to include claims from associated identities
            //profile.Identities.ToList().ForEach(i =>
            //{
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".access_token", i.AccessToken));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".provider", i.Provider));
            //    user.Add(new KeyValuePair<string, object>(i.Connection + ".user_id", i.UserId));
            //});

            // NOTE: uncomment this if you send roles
            // user.Add(new KeyValuePair<string, object>(ClaimTypes.Role, profile.ExtraProperties["roles"]));

            // NOTE: this will set a cookie with all the user claims that will be converted 
            //       to a ClaimsPrincipal for each request using the SessionAuthenticationModule HttpModule. 
            //       You can choose your own mechanism to keep the user authenticated (FormsAuthentication, Session, etc.)
            FederatedAuthentication.SessionAuthenticationModule.CreateSessionCookie(user);

            var returnTo = state.ReturnTo == null ? "/" : state.ReturnTo;

            context.Response.Redirect(returnTo);
        }

        public override bool IsReusable
        {
            get { return false; }
        }
    }
}