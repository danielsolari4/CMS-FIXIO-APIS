using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using LazyCache;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ray.Dtos;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Exception;

namespace Ray.Managers
{
    public interface IApplicationUserManager : IUserStore<User>,
        IUserLoginStore<User>,
        IUserPasswordStore<User>,
        IUserEmailStore<User>,
        IUserClaimStore<User>,
        IUserRoleStore<User>,
        IUserSecurityStampStore<User>,
        IUserLockoutStore<User>,
        IUserTwoFactorStore<User>,
        IQueryableUserStore<User>
    {
        Task<bool> HasAccessToActionAsync(string controllerName, string actionName, List<int> userActions);
        Task ValidateRolesAsync(List<string> roles);
        List<int> GetControllerActionUserAllowedList(User user);
    }

    public class ApplicationUserManager : IApplicationUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IActionRepository _actionRepository;
        private readonly IAppCache _lazyCache;

        public ApplicationUserManager(IUserRepository userRepository,
            IUserLoginRepository userLoginRepository,
            IRoleRepository roleRepository,
            IActionRepository actionRepository,
            IAppCache lazyCache)
        {
            _userRepository = userRepository;
            _userLoginRepository = userLoginRepository;
            _roleRepository = roleRepository;
            _actionRepository = actionRepository;
            _lazyCache = lazyCache;
        }

        #region IUserStore
        //public Task CreateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Add(user);

        //    return Ok();
        //}
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            _userRepository.Add(user);

            return Task.FromResult(IdentityResult.Success);
        }


        //public Task DeleteAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Delete(user);

        //    return Ok();
        //}
        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            _userRepository.Delete(user);

            return Task.FromResult(IdentityResult.Success);
        }

        //public Task<User> FindByIdAsync(int userId)
        //{
        //    if (userId <= 0)
        //        throw new ArgumentNullException("userId");

        //    return Task.FromResult<User>(_userRepository.Get(u => u.Id == userId)); //u.Discriminator.Equals(UserDiscriminator.Backend)
        //}
        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            var id = Convert.ToInt32(userId);
            if (id <= 0)
                throw new ArgumentNullException("userId");

            return Task.FromResult<User>(_userRepository.Get(u => u.Id == id)); //u.Discriminator.Equals(UserDiscriminator.Backend)
        }


        //public Task<User> FindByNameAsync(string userName)
        //{
        //    if (string.IsNullOrWhiteSpace(userName))
        //        throw new ArgumentNullException("userName");

        //    return Task.FromResult<User>(_userRepository.Get(u => u.UserName.Equals(userName)));//&& u.Discriminator.Equals(UserDiscriminator.Backend)
        //}
        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(normalizedUserName))
                throw new ArgumentNullException("userName");

            return Task.FromResult<User>(_userRepository.Get(u => u.UserName.Equals(normalizedUserName)));
        }


        //public Task UpdateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            _userRepository.Update(user);

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.UserName.ToString());
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.UserName = userName;
            return Ok();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(user.UserName.Trim().ToLower());
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.UserName = normalizedName;
            return Ok();
        }
        #endregion

        #region IUserLoginStore
        //public Task AddLoginAsync(User user, UserLoginInfo login)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (login == null)
        //        throw new EntityException("login");

        //    var l = new UserLogin
        //    {
        //        LoginProvider = login.LoginProvider,
        //        ProviderKey = login.ProviderKey,
        //        User = user
        //    };

        //    user.Logins.Add(l);
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (login == null)
                throw new EntityException("login");

            var l = new UserLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                User = user
            };

            user.UserLogins.Add(l);
            _userRepository.Update(user);

            return Ok();
        }



        //public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");
        //    return Task.FromResult<IList<UserLoginInfo>>(user.Logins.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, null)).ToList());
        //}
        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");
            return Task.FromResult<IList<UserLoginInfo>>(user.UserLogins.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey, user.UserName)).ToList());
        }

        //public Task RemoveLoginAsync(User user, UserLoginInfo login)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (login == null)
        //        throw new EntityException("login");

        //    var l = user.Logins.FirstOrDefault(ul => ul.LoginProvider.Equals(login.LoginProvider) && ul.ProviderKey.Equals(login.ProviderKey));

        //    if (l != null)
        //    {
        //        user.Logins.Remove(l);
        //        _userRepository.Update(user);
        //    }

        //    return Ok();
        //}
        public Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            var l = user.UserLogins.FirstOrDefault(ul => ul.LoginProvider.Equals(loginProvider) && ul.ProviderKey.Equals(providerKey));

            if (l != null)
            {
                user.UserLogins.Remove(l);
                _userRepository.Update(user);
            }

            return Ok();
        }

        //public Task<User> FindAsync(UserLoginInfo login)
        //{
        //    if (login == null)
        //        throw new EntityException("login");

        //    var l = _userLoginRepository.Get(ul => ul.ProviderKey.Equals(login.ProviderKey) && ul.LoginProvider.Equals(login.LoginProvider));

        //    if (l == null)
        //        return Task.FromResult<User>(null);

        //    return Task.FromResult<User>(l.User);
        //}

        public Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            var l = _userLoginRepository.Get(ul => ul.ProviderKey.Equals(providerKey) && ul.LoginProvider.Equals(loginProvider));

            if (l == null)
                return Task.FromResult<User>(null);

            return Task.FromResult<User>(l.User);
        }
        #endregion

        #region IUserPasswordStore
        //public Task<string> GetPasswordHashAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<string>(user.PasswordHash);
        //}
        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<string>(user.PasswordHash);
        }

        //public Task<bool> HasPasswordAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.PasswordHash));
        //}
        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        //public Task SetPasswordHashAsync(User user, string passwordHash)
        //{
        //    user.PasswordHash = passwordHash;
        //    return Ok();
        //}
        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            user.PasswordHash = passwordHash;
            return Ok();
        }



        #endregion

        #region IUserEmailStore
        //public Task<User> FindByEmailAsync(string email)
        //{
        //    if (string.IsNullOrWhiteSpace(email))
        //        throw new ArgumentNullException("email");

        //    var user = _userRepository.Get(u => u.Email.Equals(email) && u.Discriminator.Equals(UserDiscriminator.Backend) && !u.IsDeleted);

        //    return Task.FromResult<User>(user);
        //}
        public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                throw new ArgumentNullException("email");

            var user = _userRepository.Get(u => u.Email.Equals(normalizedEmail) && u.Discriminator.Equals(UserDiscriminator.Backend) && !u.IsDeleted);

            return Task.FromResult<User>(user);
        }


        //public Task<string> GetEmailAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.Email);
        //}
        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult(user.Email);
        }


        //public Task<bool> GetEmailConfirmedAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.EmailConfirmed);
        //}
        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        //public Task SetEmailAsync(User user, string email)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(email))
        //        throw new ArgumentNullException("email");

        //    user.Email = email;
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentNullException("email");

            user.Email = email;
            _userRepository.Update(user);

            return Ok();
        }


        //public Task SetEmailConfirmedAsync(User user, bool confirmed)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(user.Email))
        //        throw new InvalidOperationException("AUEX_001");

        //    user.EmailConfirmed = confirmed;
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new InvalidOperationException("AUEX_001");

            user.EmailConfirmed = confirmed;
            _userRepository.Update(user);

            return Ok();
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IUserClaimStore
        //public Task AddClaimAsync(User user, Claim claim)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (claim == null)
        //        throw new EntityException("claim");

        //    var c = new UserClaim
        //    {
        //        ClaimType = claim.Type,
        //        ClaimValue = claim.Value,
        //        User = user
        //    };

        //    user.Claims.Add(c);
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task AddClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (claims == null || !claims.Any())
                throw new EntityException("claim");

            foreach (var claim in claims)
            {
                var c = new UserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    User = user
                };

                user.UserClaims.Add(c);
                _userRepository.Update(user);
            }

            return Ok();
        }

        //public Task<IList<Claim>> GetClaimsAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        //}
        public Task<IList<Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<IList<Claim>>(user.UserClaims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        }

        //public Task RemoveClaimAsync(User user, Claim claim)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (claim == null)
        //        throw new EntityException("claim");

        //    var c = user.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

        //    if (c != null)
        //    {
        //        user.Claims.Remove(c);
        //        _userRepository.Update(user);
        //    }

        //    return Ok();
        //}
        public Task RemoveClaimsAsync(User user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (claims == null || !claims.Any())
                throw new EntityException("claim");

            foreach (var claim in claims.ToList())
            {
                var c = user.UserClaims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);

                if (c != null)
                {
                    user.UserClaims.Remove(c);
                    _userRepository.Update(user);
                }
            }
            return Ok();
        }

        public Task ReplaceClaimAsync(User user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task<IList<User>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IUserRoleStore
        public async Task ValidateRolesAsync(List<string> roles)
        {
            if (roles == null || !roles.Any())
                throw new ArgumentNullException("roles");

            var rols = await _roleRepository.GetAll();

            if (!rols.Any(r => roles.Contains(r.Name)))
                throw new ArgumentException("roles");

            await Task.FromResult(0);
        }

        //public async Task AddToRoleAsync(User user, string roleName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(roleName))
        //        throw new ArgumentNullException("roleName");

        //    var r = await _roleRepository.Get(ro => ro.Name.Equals(roleName));
        //    if (r.FirstOrDefault() == null)
        //        throw new ArgumentException("roleName");

        //    user.Roles.Add(r.FirstOrDefault());
        //    _userRepository.Update(user);

        //    await Task.FromResult(0);
        //}
        public async Task AddToRoleAsync(User user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("roleName");

            var r = await _roleRepository.Get(ro => ro.Name.Equals(roleName));
            if (r.FirstOrDefault() == null)
                throw new ArgumentException("roleName");

            user.UserRoles.Add(new UserRole() { Role = r.FirstOrDefault() });
            _userRepository.Update(user);

            await Task.FromResult(0);
        }

        //public Task RemoveFromRoleAsync(User user, string roleName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(roleName))
        //        throw new ArgumentNullException("role");

        //    var role = user.Roles.FirstOrDefault(r => r.Name.Equals(roleName));

        //    if (role == null)
        //        throw new ArgumentException("roleName");

        //    user.Roles.Remove(role);
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        public Task RemoveFromRoleAsync(User user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("role");

            var role = user.UserRoles.FirstOrDefault(r => r.Role.Name.Equals(roleName));

            if (role == null)
                throw new ArgumentException("roleName");

            user.UserRoles.Remove(role);
            _userRepository.Update(user);

            return Ok();
        }

        public Task<bool> IsInRoleAsync(User user, string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentNullException("role");

            return Task.FromResult<bool>(user.UserRoles.Any(r => r.Role.Name.Equals(roleName)));
        }

        public Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<IList<string>>(user.UserRoles.Select(r => r.Role.Name).ToList());
        }

        public Task<IList<User>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var users = _userRepository.GetAll().Where(x => x.UserRoles.Any(s => s.Role.Name == roleName));
            return Task.FromResult<IList<User>>(users.ToList());
        }

        public async Task<bool> HasAccessToActionAsync(string controllerName, string actionName, List<int> userActions)
        {
            if (userActions == null || userActions.Count < 1)
                throw new ArgumentNullException("userActions");

            if (string.IsNullOrWhiteSpace(controllerName))
                throw new ArgumentNullException("controller");

            if (string.IsNullOrWhiteSpace(actionName))
                throw new ArgumentNullException("action");


            var systemActionsCache = await _lazyCache.GetOrAddAsync<List<Ray.Model.NewContext.Entities.Action>>("systemActionsCache", async () =>
            {
                var res = await _actionRepository.GetAll();
                return res.ToList();
            }, TimeSpan.FromHours(2));

            var result = systemActionsCache.FirstOrDefault(x => x.ControllerName == controllerName && x.ActionName == actionName);
            if (result == null || !userActions.Contains(result.Id))
                return false;

            return true;
        }

        public List<int> GetControllerActionUserAllowedList(User user)
        {
            var availableActions = user.UserRoles.Select(x => x.Role.RoleActions.Select(f => f.ActionId)).ToList();
            return availableActions.SelectMany(x => x).ToList();

            /*Select actions by name and controllername
            var userAvailableActions = await _actionRepository.Get(x => user
            .UserRoles
            .Select(x => x.Role.RoleActions
                .Select(f => f.ActionId))
            .ToList()
            .FirstOrDefault()
            .Contains(x.Id));
            var groupedControllerActions = userAvailableActions.ToList()
                .GroupBy(x => x.ControllerName, x => new AllowedControllerActionDto { ControllerName = x.ControllerName, ActionName = x.ActionName });

                 //       return groupedControllerActions.SelectMany(c => c);
            */
        }


        #endregion

        #region IUserSecurityStampStore
        //public Task<string> GetSecurityStampAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<string>(user.SecurityStamp);
        //}
        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            if (user == null)
                throw new EntityException("user");

            user.SecurityStamp = stamp;
            return Ok();
        }
        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.SecurityStamp = stamp;
            return Ok();
        }

        #endregion

        #region IUserLockoutStore
        //public Task<int> GetAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.AccessFailedCount);
        //}
        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult(user.AccessFailedCount);
        }

        //public Task<bool> GetLockoutEnabledAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.LockoutEnabled);
        //}
        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult(user.LockoutEnabled);
        }

        //public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(
        //        user.LockoutEndDateUtc.HasValue ?
        //            new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) :
        //            new DateTimeOffset());
        //}
        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            DateTimeOffset? result = user.LockoutEndDateUtc.HasValue ?
                        new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) :
                        new DateTimeOffset();

            return Task.FromResult(result);
        }

        //public Task<int> IncrementAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.AccessFailedCount++;
        //    return Task.FromResult(user.AccessFailedCount);
        //}
        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        //public Task ResetAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.AccessFailedCount = 0;
        //    return Ok();
        //}
        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.AccessFailedCount = 0;
            return Ok();
        }

        //public Task SetLockoutEnabledAsync(User user, bool enabled)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.LockoutEnabled = enabled;
        //    return Ok();
        //}
        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.LockoutEnabled = enabled;
            return Ok();
        }

        //public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.UtcDateTime);
        //    return Ok();
        //}
        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.Value.UtcDateTime);
            return Ok();
        }
        #endregion

        #region IUserTwoFactorStore
        //public Task<bool> GetTwoFactorEnabledAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<bool>(user.TwoFactorEnabled);
        //}
        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            return Task.FromResult<bool>(user.TwoFactorEnabled);
        }

        //public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.TwoFactorEnabled = enabled;
        //    return Ok();
        //}
        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (user == null)
                throw new EntityException("user");

            user.TwoFactorEnabled = enabled;
            return Ok();
        }
        #endregion

        #region IQueryableUserStore
        public IQueryable<User> Users
        {
            get { return _userRepository.GetAll().Where(u => u.Discriminator.Equals(UserDiscriminator.Backend)); }
        }
        #endregion

        public void Dispose()
        {
            // Dispose does nothing since we want Unity to manage the lifecycle of our Unit of Work
        }

        private Task Ok()
        {
            return Task.FromResult(0);
        }
        private Task Ok(IdentityResult result)
        {
            return Task.FromResult(result);
        }















































    }
}
