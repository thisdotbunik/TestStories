using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace TestStories.API.Services
{
    public static class ServiceUtility
    {
        public static string CurrentUserEmail (this ControllerBase controller) => controller.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        public static string CurrentUserRole (this ControllerBase controller) => controller.User.FindFirst(ClaimTypes.Role)?.Value;
    }
}
