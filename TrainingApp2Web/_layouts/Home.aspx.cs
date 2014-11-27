using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace TrainingDemoAppWeb.Pages
{
    public partial class Home : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // The following code gets the client context and Title property by using TokenHelper.
            // To access other properties, you may need to request permissions on the host web.

            Uri sharePointSiteUrlHost = null;

            if (string.IsNullOrEmpty(Page.Request["SPHostUrl"]) == false)
            {
                sharePointSiteUrlHost = new Uri(Page.Request["SPHostUrl"].ToString());
                Session["SPHostUrl"] = sharePointSiteUrlHost.ToString();
            }
            else if (Session["SPHostUrl"] != null)
            {
                sharePointSiteUrlHost = new Uri(Session["SPHostUrl"].ToString());
            }
            Initialize();

            using (var clientContext = TokenHelper.GetClientContextWithAccessToken(sharePointSiteUrlHost.ToString(), Session["AccessTokenHost"].ToString()))
            {
                clientContext.Load(clientContext.Web, web => web.Title);
                clientContext.ExecuteQuery();
                Response.Write(clientContext.Web.Title);
            }
        }

        private void Initialize()
        {
            Uri targetUri = new Uri(Session["SPHostUrl"].ToString());
            if (Session["AccessTokenHost"] == null)
            {
                if (string.IsNullOrEmpty(Page.Request.QueryString["SPAppToken"]) == false || string.IsNullOrEmpty(Page.Request.Form["SPAppToken"]) == false)
                {
                    Session["AccessTokenHost"] = TokenHelper.GetAccessToken(targetUri.ToString(), TokenHelper.GetContextTokenFromRequest(Page.Request), Request.Url.Authority);
                }
                else
                {
                    if (Request.QueryString["code"] != null)
                    {
                        string refreshToken = TokenHelper.GetAccessToken(Request.QueryString["code"], "00000003-0000-0ff1-ce00-000000000000", targetUri.Authority, TokenHelper.GetRealmFromTargetUrl(targetUri), new Uri(Request.Url.GetLeftPart(UriPartial.Path))).RefreshToken;
                        Session["refreshToken"] = refreshToken;
                    }

                    if (Session["refreshToken"] == null)
                    {
                        Response.Redirect(TokenHelper.GetAuthorizationUrl(targetUri.ToString(), "Web.Manage"));
                    }
                    else
                    {
                        string refreshToken = Session["refreshToken"].ToString();
                        Session["AccessTokenHost"] = TokenHelper.GetAccessToken(refreshToken, "00000003-0000-0ff1-ce00-000000000000", targetUri.Authority, TokenHelper.GetRealmFromTargetUrl(targetUri)).AccessToken;
                    }
                }
            }
        }
    }
}