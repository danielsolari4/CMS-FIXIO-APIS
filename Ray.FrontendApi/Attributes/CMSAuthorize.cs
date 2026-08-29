using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Managers;
using Ray.Model.NewContext.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ray.FrontendApi.Attributes
{
    public enum CustomAuthorizationType
    {
        HasAccessToControllerAndAction
    }

    public class HasPermissionRequirement : IAuthorizationRequirement
    {
        public CustomAuthorizationType CustomAuthorizationType { get; set; }
        public HasPermissionRequirement(CustomAuthorizationType customAuthorizationType) => CustomAuthorizationType = customAuthorizationType;
    }

    public class HasPermissionsHandler : AuthorizationHandler<HasPermissionRequirement>
    {
        private readonly UserManager<User> _userManager;
        private readonly IApplicationUserManager _applicationUserManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HasPermissionsHandler(UserManager<User> userManager, IApplicationUserManager applicationUserManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _applicationUserManager = applicationUserManager;
            _httpContextAccessor = httpContextAccessor;
        }
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasPermissionRequirement requirement)
        {
            var action = _httpContextAccessor.HttpContext.Request.RouteValues["action"].ToString();
            var controller = _httpContextAccessor.HttpContext.Request.RouteValues["controller"].ToString();

            if (requirement.CustomAuthorizationType == CustomAuthorizationType.HasAccessToControllerAndAction)
            {
                if (context.User.Identity.IsAuthenticated)
                {
                    var loggedUser = GetLoggedUser(context);
                    var user = _userManager.FindByEmailAsync(loggedUser.Email).Result;
                    if (user == null || !user.IsEnabled || !_applicationUserManager.HasAccessToActionAsync(controller, action, loggedUser.RolesInt).Result)
                    {
                        context.Fail();
                        return Task.CompletedTask;
                    }
                    else
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }

            context.Fail();
            return Task.CompletedTask;
        }

        public LoggedUserDto GetLoggedUser(AuthorizationHandlerContext context)
        {
            if (context.User == null) return null;
            var roles = context.User.Claims.Where(x => x.Type == ClaimTypes.Role)?.ToList();
            var rolesInt = context.User.Claims.Where(x => x.Type == ClaimTypes.UserData)?.FirstOrDefault()?.Value;
            int userId;
            var claimsUserId = context.User.FindAll(ClaimTypes.NameIdentifier).ToList();
            var userIdParsed = int.TryParse(claimsUserId.FirstOrDefault().Value, out userId);
            if (!userIdParsed)
                int.TryParse(claimsUserId[1].Value, out userId);

            return new LoggedUserDto()
            {
                Id = userId,
                Email = context.User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value,
                RolesInt = JsonConvert.DeserializeObject<List<int>>(rolesInt),
                Roles = roles.Count > 0 ? roles.Select(x => x.Value).ToList() : new List<string>()
            };
        }
    }
}
