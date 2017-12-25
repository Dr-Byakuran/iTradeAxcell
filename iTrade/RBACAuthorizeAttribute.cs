using iTrade.Models;
using CLK.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Web;

namespace iTrade
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class RBACAuthorizeAttribute : CLK.AspNet.Identity.Mvc.RBACAuthorizeAttribute
    {
        // Methods
        protected override PermissionAuthorize GetPermissionAuthorize(HttpContextBase httpContext)
        {
            return new ApplicationPermissionAuthorize(httpContext.GetOwinContext().Get<ApplicationPermissionManager>());
        }
    }
}
