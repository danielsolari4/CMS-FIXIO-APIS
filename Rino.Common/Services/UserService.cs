using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Rino.Common.Authentication
{
    public interface IUserService
    {
        string GetLoggedUsername();
        LoggedUser GetLoggedUser();
        int GetLoggedUserId();
        IUserSession GetCurrentUser();
    }
    public class UserService : IUserService, ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        public UserService(IHttpContextAccessor httpContextAccessor, IMemoryCache memoryCache)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryCache = memoryCache;
        }

        public List<string> GetInvalidatedTokenIds()
        {
            var invalidatedTokensId = _memoryCache.Get<List<string>>("invalidated_tokens_id");
            if (invalidatedTokensId == null)
                return new List<string>();
            else
                return invalidatedTokensId;
        }

        public IUserSession GetCurrentUser()
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                return new UserSession();
            }

            IUserSession currentUser = new UserSession
            {
                IsAuthenticated = _httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated ?? false,
                UserName = _httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "No user"
            };

            return currentUser;
        }


        public string GetLoggedUsername()
            => _httpContextAccessor?.HttpContext?.User?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value ?? "";

        public int GetLoggedUserId()
        {
            var loggedUser = GetLoggedUser();
            if (loggedUser == null) return 0;

            return loggedUser.Id;
        }

        public LoggedUser GetLoggedUser()
        {
            if (_httpContextAccessor.HttpContext.User == null ||
                _httpContextAccessor.HttpContext.User.Claims == null ||
                _httpContextAccessor.HttpContext.User.Claims.Where(x => x.Type == ClaimTypes.Email)?.FirstOrDefault() == null) return null;

            var user = _httpContextAccessor.HttpContext.User;
            if (user == null || user.Claims.ToList().Count < 1) return null;
            var roles = user.Claims.Where(x => x.Type == ClaimTypes.Role)?.ToList();
            //var rolesId = user.Claims.Where(x => x.Type == "RolesId").FirstOrDefault()?.Value;

            return new LoggedUser()
            {
                Id = Convert.ToInt32(user.FindFirst(ClaimTypes.NameIdentifier)?.Value),
                Email = user.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value,
                Roles = roles.Count > 0 ? roles.Select(x => x.Value).ToList() : new List<string>(),
                //RolesInt = JsonConvert.DeserializeObject<List<int>>(rolesId),
            };
        }
    }
}
