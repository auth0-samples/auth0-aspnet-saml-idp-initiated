using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Services;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;

namespace aspnet4_sample1.Auth
{
    public class AuthHelper
    {
        private const String STATE_KEY = "auth0:states";

        public enum Prompt : int { none, login, consent, undefined };
        public class State
        {
            private static readonly RNGCryptoServiceProvider random = new RNGCryptoServiceProvider();

            static private string GenerateNonce(int length)
            {
                var data = new byte[length];
                random.GetNonZeroBytes(data);
                return Convert.ToBase64String(data);
            }

            private State(string nonce, string returnTo, bool isSaml)
            {
                this.Nonce = nonce;
                this.ReturnTo = IsLocalUrl(returnTo) && returnTo != "/Account/Login" ? returnTo : "/"; // restrict returnTo to relative paths
                this.IsSamlRedirect = isSaml;
            }

            static private bool IsLocalUrl(string url)
            {
                return !String.IsNullOrEmpty(url)
                    && url.StartsWith("/")
                    && !url.StartsWith("//")
                    && !url.StartsWith("/\\");
            }

            static public String Create(bool isSaml, State previousState)
            {
                var context = HttpContext.Current;
                var nonce = GenerateNonce(24);
                State state = null;
                if (isSaml) {
                    // If SAML, returnTo should be set to RelayState
                    // TODO: Validate that this is a relative path!
                    state = new State(nonce, context.Request.QueryString["state"], true);
                } else if (previousState != null)
                {
                    // Let previous state set the return to
                    state = new State(nonce, previousState.ReturnTo, false);
                } else
                {
                    // must be new, store the absolute path as the returnTo
                    state = new State(nonce, context.Request.Url.AbsolutePath, false);
                }

                var states = context.Session[STATE_KEY] as Dictionary<String, State>;
                if (states == null) states = new Dictionary<string, State>();
                states.Add(nonce, state);
                context.Session[STATE_KEY] = states;
                return state.Nonce;
            }

            static public State Validate(String nonce)
            {
                var context = HttpContext.Current;
                var states = context.Session[STATE_KEY] as Dictionary<String,State>;
                if (states != null)
                {
                    State state = null;
                    if (states.TryGetValue(nonce, out state))
                    {
                        states.Remove(nonce);
                        context.Session[STATE_KEY] = states;
                        return state;
                    } else
                    {
                        return null;
                    }
                }

                return null;
            }

            private string Nonce;
            public string ReturnTo { get; }
            public bool IsSamlRedirect { get; }
        }


        static public String BuildAuthorizeUrl(Prompt prompt, bool isSamlRequest, State previousState=null)
        {
            var context = HttpContext.Current;
            var client = new AuthenticationApiClient(
                new Uri(string.Format("https://{0}", ConfigurationManager.AppSettings["auth0:Domain"])));

            var request = context.Request;
            var redirectUri = new UriBuilder(request.Url.Scheme, request.Url.Host, context.Request.Url.IsDefaultPort ? -1 : request.Url.Port, "LoginCallback.ashx");

            var authorizeUrlBuilder = client.BuildAuthorizationUrl()
                .WithClient(ConfigurationManager.AppSettings["auth0:ClientId"])
                .WithRedirectUrl(redirectUri.ToString())
                .WithResponseType(AuthorizationResponseType.Code)
                .WithScope("openid profile")
                // adding this audience will cause Auth0 to use the OIDC-Conformant pipeline
                // you don't need it if your client is flagged as OIDC-Conformant (Advance Settings | OAuth)
                .WithAudience("https://" + @ConfigurationManager.AppSettings["auth0:Domain"] + "/userinfo")
                .WithState(State.Create(isSamlRequest, previousState));

            if (prompt != Prompt.undefined)
            {
                authorizeUrlBuilder.WithValue("prompt", prompt.ToString());
            }

            if (isSamlRequest)
            {
                authorizeUrlBuilder.WithConnection(ConfigurationManager.AppSettings["auth0:SamlConnection"]);
            }

            return authorizeUrlBuilder.Build().ToString();
        }

        static public String Logout(String returnTo)
        {
            var context = HttpContext.Current;
            FederatedAuthentication.SessionAuthenticationModule.SignOut();

            // Redirect to Auth0's logout endpoint.
            // After terminating the user's session, Auth0 will redirect to the 
            // returnTo URL, which you will have to add to the list of allowed logout URLs for the client.
            return string.Format(CultureInfo.InvariantCulture,
                "https://{0}/v2/logout?returnTo={1}",
                ConfigurationManager.AppSettings["auth0:Domain"],
                HttpUtility.UrlEncode(returnTo));
        }

    }
}