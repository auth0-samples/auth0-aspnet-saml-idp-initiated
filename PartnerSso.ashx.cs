namespace aspnet4_sample1
{
    using Auth;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.SessionState;

    public class PartnerSso : HttpTaskAsyncHandler, IRequiresSessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var error = context.Request.QueryString["error"];
            if (error != null)
            {
                context.Response.Redirect("/error"+context.Request.Url.Query);
                return;
            }

            // if no errors, just redirect back to authorize with SAML connection set
            context.Response.Redirect(AuthHelper.BuildAuthorizeUrl(AuthHelper.Prompt.none, true));
            return;
        }

        public override bool IsReusable
        {
            get { return false; }
        }
    }
}