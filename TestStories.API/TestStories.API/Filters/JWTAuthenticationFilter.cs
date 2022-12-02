using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Principal;
using System.Threading;
using System.Net;
using TestStories.API.Common.Authentication;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace TestStories.API.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class JWTAuthenticationFilter : Attribute,IAuthorizationFilter
    {
        private StringValues authorizationToken;

        public bool IsUserAuthorized(AuthorizationFilterContext actionContext)
        {
            var authHeader = FetchFromHeader(actionContext); //fetch authorization token from header


            if (authHeader != null)
            {
                var auth = new AuthenticationModule();
                JwtSecurityToken userPayloadToken = auth.GenerateUserClaimFromJwt(authHeader);

                if (userPayloadToken != null)
                {

                    var identity = auth.PopulateUserIdentity(userPayloadToken);
                    string[] roles = { "All" };
                    var genericPrincipal = new GenericPrincipal(identity, roles);
                    Thread.CurrentPrincipal = genericPrincipal;
                    var authenticationIdentity = Thread.CurrentPrincipal.Identity as JWTAuthenticationIdentity;
                    if (authenticationIdentity != null && !String.IsNullOrEmpty(authenticationIdentity.UserName))
                    {
                        authenticationIdentity.UserId = identity.UserId;
                        authenticationIdentity.UserName = identity.UserName;
                    }
                    return true;
                }

            }
            return false;



        }
        public class JWTAuthenticationIdentity : GenericIdentity
        {

            public string UserName { get; set; }
            public string UserId { get; set; }

            public JWTAuthenticationIdentity(string userName)
                : base(userName)
            {
                UserName = userName;
            }


        }

        private string FetchFromHeader(AuthorizationFilterContext actionContext)
        {



            actionContext.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationToken);

            return authorizationToken.FirstOrDefault();
        }

        private static void ShowAuthenticationError(AuthorizationFilterContext filterContext)
        {

            filterContext.Result = new JsonResult("")
            {
                Value = new
                {
                    Status = "Error"
                },
            };
        }

        void IAuthorizationFilter.OnAuthorization(AuthorizationFilterContext context)
        {
            if (!IsUserAuthorized(context))
            {
                ShowAuthenticationError(context);
                return;
            }
            context.Result = new JsonResult("")
            {
                Value = new
                {
                    Status = "OK"
                },
            };

        }
    }
}
