using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using OfficeMeetingRoomsBookingSystem.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OfficeMeetingRoomsBookingSystem.Filters
{
    public class PageAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;

            if (actionDescriptor == null || actionDescriptor.ControllerTypeInfo.BaseType != typeof(Controller))
            {
                // If it's not a controller action, allow access
                return;
            }

            // For login actions
            var actionName = actionDescriptor.ActionName;
            if (CommonUtil.IsLoginAction(actionName))
            {
                if (context.HttpContext.User.Identity.IsAuthenticated)
                {
                    context.Result = new RedirectResult("/Home/MeetingRoomsMap");
                    return;
                }

                return;
            }

            // For non-login actions
            if (!context.HttpContext.User.Identity.IsAuthenticated)
            {
                context.Result = new RedirectResult("/Home/LoginPage");
                return;
            }

            // If authenticated and not a login action, allow access
            return;
        }
    }
}
