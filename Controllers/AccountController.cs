using aspnet4_sample1.Auth;
using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using System;
using System.Configuration;
using System.Globalization;
using System.IdentityModel.Services;
using System.Web;
using System.Web.Mvc;

namespace aspnet4_sample1.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Login(string returnUrl)
        {
            return new RedirectResult(AuthHelper.BuildAuthorizeUrl(AuthHelper.Prompt.none, false));
        }

        public ActionResult Logout()
        {
            var returnTo = Url.Action("Index", "Home", null, protocol: Request.Url.Scheme);
            return Redirect(AuthHelper.Logout(returnTo));
        }
    }
}