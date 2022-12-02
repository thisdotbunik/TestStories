using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TestStories.API.Services;
using TestStories.DataAccess.Entities;
using TestStories.DataAccess.Enums;

namespace TestStories.API.Filters
{
    public class CustomAuthorizationFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        readonly TestStoriesContext _context;
        /// <inheritdoc />
        public CustomAuthorizationFilter(TestStoriesContext context)
        {
            _context = context;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.HttpContext.User.Claims.ToList().Count == 0)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
                return;
            }
            var email = context.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
            var statusId = 0;
            if (!string.IsNullOrEmpty(email))
            {
                var user = _context.User.Where(x => x.Email == email).FirstOrDefault();
                if(user != null)
                {
                    statusId = user.UserstatusId;
                }
            }
            var isValid = false;
            isValid = statusId == (int)UserStatusEnum.Active;
            if (!isValid)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
