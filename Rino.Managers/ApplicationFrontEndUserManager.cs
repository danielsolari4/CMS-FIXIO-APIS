using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Utils.Exception;

namespace Rino.Managers
{
    //TODO: ver comop implementar este application
    public interface IApplicationFrontEndUserManager //: IUserStore<User>, IUserLoginStore<User>, IUserPasswordStore<User>, IUserEmailStore<User>,
    //                         IUserClaimStore<User>, IUserRoleStore<User>, IUserSecurityStampStore<User>, IUserLockoutStore<User>,
    //                         IUserTwoFactorStore<User>, IQueryableUserStore<User>
    {
        //Task<bool> HasAccessToActionAsync(User user, string controllerName, string actionName);
        //Task ValidateRolesAsync(List<string> roles);
    }

    //TODO: pide agregar CancellationToken cancellationToken a todos los metodos
    public class ApplicationFrontEndUserManager : IApplicationFrontEndUserManager
    {
        //private readonly IUserRepository _userRepository;
        //private readonly IUserLoginRepository _userLoginRepository;
        //private readonly IRoleRepository _roleRepository;

        //public ApplicationFrontEndUserManager(IUserRepository userRepository, IUserLoginRepository userLoginRepository, IRoleRepository roleRepository)
        //{
        //    _userRepository = userRepository;
        //    _userLoginRepository = userLoginRepository;
        //    _roleRepository = roleRepository;
        //}

        //#region IUserStore
        //public Task CreateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Add(user);

        //    return Ok();
        //}

        //public Task DeleteAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Delete(user);

        //    return Ok();
        //}

        //public Task<User> FindByIdAsync(int userId)
        //{
        //    if (userId <= 0)
        //        throw new ArgumentNullException("userId");

        //    return Task.FromResult<User>(_userRepository.GetById(userId));
        //}

        //public Task<User> FindByNameAsync(string userName)
        //{
        //    if (string.IsNullOrWhiteSpace(userName))
        //        throw new ArgumentNullException("userName");

        //    return Task.FromResult<User>(_userRepository.Get(u => u.UserName.Equals(userName)));
        //}

        //public Task UpdateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    _userRepository.Update(user);

        //    return Ok();
        //}
        //#endregion

        //#region IUserLoginStore
        //public Task AddLoginAsync(User user, UserLoginInfo login)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException("user");

        //    if (login == null)
        //        throw new ArgumentNullException("login");

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

        //public Task<User> FindAsync(UserLoginInfo login)
        //{
        //    if (login == null)
        //        throw new ArgumentNullException("login");

        //    var l = _userLoginRepository.Get(ul => ul.ProviderKey.Equals(login.ProviderKey) && ul.LoginProvider.Equals(login.LoginProvider));

        //    if (l == null)
        //        return Task.FromResult<User>(null);

        //    return Task.FromResult<User>(l.User);
        //}

        //public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException("user");

        //    return Task.FromResult<IList<UserLoginInfo>>(user.Logins.Select(ul => new UserLoginInfo(ul.LoginProvider, ul.ProviderKey)).ToList());
        //}

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
        //#endregion

        //#region IUserPasswordStore
        //public Task<string> GetPasswordHashAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<string>(user.PasswordHash);
        //}

        //public Task<bool> HasPasswordAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.PasswordHash));
        //}

        //public Task SetPasswordHashAsync(User user, string passwordHash)
        //{
        //    user.PasswordHash = passwordHash;
        //    return Ok();
        //}
        //#endregion

        //#region IUserEmailStore
        //public Task<User> FindByEmailAsync(string email)
        //{
        //    if (string.IsNullOrWhiteSpace(email))
        //        throw new ArgumentNullException("email");

        //    var user = _userRepository.Get(u => u.Email.Equals(email) && !u.IsDeleted);

        //    return Task.FromResult<User>(user);
        //}

        //public Task<string> GetEmailAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.Email);
        //}

        //public Task<bool> GetEmailConfirmedAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.EmailConfirmed);
        //}

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

        //public Task SetEmailConfirmedAsync(User user, bool confirmed)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(user.Email))
        //        throw new InvalidOperationException("No se puede establecer el estado de confirmación del correo electrónico porque el usuario no tiene uno");//Cannot set the confirmation status of the e-mail because user doesn't have an e-mail.

        //    user.EmailConfirmed = confirmed;
        //    _userRepository.Update(user);

        //    return Ok();
        //}
        //#endregion

        //#region IUserClaimStore
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

        //public Task<IList<Claim>> GetClaimsAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<IList<Claim>>(user.Claims.Select(c => new Claim(c.ClaimType, c.ClaimValue)).ToList());
        //}

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
        //#endregion

        //#region IUserRoleStore
        //public async Task ValidateRolesAsync(List<string> roles)
        //{
        //    if (roles == null || !roles.Any())
        //        throw new EntityException("roles");

        //    var rols = await _roleRepository.GetAll();

        //    if (!rols.Any(r => roles.Contains(r.Name)))
        //        throw new ArgumentException("roles");

        //    await Task.FromResult(0);
        //}

        //public async Task AddToRoleAsync(User user, string roleName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(roleName))
        //        throw new ArgumentNullException("roleName");

        //    var r = await _roleRepository.Get(ro => ro.Name.Equals(roleName));
        //    if (r.FirstOrDefault() == null)
        //        //TODO: ver que tipo de error poner acá
        //        throw new ArgumentException("roleName"); 

        //    user.Roles.Add(r.FirstOrDefault());
        //    _userRepository.Update(user);

        //    await Task.FromResult(0);
        //}

        //public Task<IList<string>> GetRolesAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<IList<string>>(user.Roles.Select(r => r.Name).ToList());
        //}

        //public Task<bool> IsInRoleAsync(User user, string roleName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(roleName))
        //        throw new ArgumentNullException("role");

        //    return Task.FromResult<bool>(user.Roles.Any(r => r.Name.Equals(roleName)));
        //}

        //public Task RemoveFromRoleAsync(User user, string roleName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(roleName))
        //        throw new ArgumentNullException("role");

        //    var role = user.Roles.FirstOrDefault(r => r.Name.Equals(roleName));

        //    if (role == null)
        //        //TODO: ver que tipo de error poner acá
        //        throw new ArgumentException("roleName");

        //    user.Roles.Remove(role);
        //    _userRepository.Update(user);

        //    return Ok();
        //}

        //public Task<bool> HasAccessToActionAsync(User user, string controllerName, string actionName)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    if (string.IsNullOrWhiteSpace(controllerName))
        //        throw new ArgumentNullException("controller");

        //    if (string.IsNullOrWhiteSpace(actionName))
        //        throw new ArgumentNullException("action");

        //    return Task.FromResult<bool>(user.Roles.Any(r => r.Actions.Any(a => a.ControllerName.Equals(controllerName) && a.ActionName.Equals(actionName))));
        //}

        //#endregion

        //#region IUserSecurityStampStore
        //public Task<string> GetSecurityStampAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<string>(user.SecurityStamp);
        //}

        //public Task SetSecurityStampAsync(User user, string stamp)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.SecurityStamp = stamp;
        //    return Ok();
        //}
        //#endregion

        //#region IUserLockoutStore
        //public Task<int> GetAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.AccessFailedCount);
        //}

        //public Task<bool> GetLockoutEnabledAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(user.LockoutEnabled);
        //}

        //public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult(
        //        user.LockoutEndDateUtc.HasValue ?
        //            new DateTimeOffset(DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc)) :
        //            new DateTimeOffset());
        //}

        //public Task<int> IncrementAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.AccessFailedCount++;
        //    return Task.FromResult(user.AccessFailedCount);
        //}

        //public Task ResetAccessFailedCountAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.AccessFailedCount = 0;
        //    return Ok();
        //}

        //public Task SetLockoutEnabledAsync(User user, bool enabled)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.LockoutEnabled = enabled;
        //    return Ok();
        //}

        //public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.LockoutEndDateUtc = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.UtcDateTime);
        //    return Ok();
        //}
        //#endregion

        //#region IUserTwoFactorStore
        //public Task<bool> GetTwoFactorEnabledAsync(User user)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    return Task.FromResult<bool>(user.TwoFactorEnabled);
        //}

        //public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        //{
        //    if (user == null)
        //        throw new EntityException("user");

        //    user.TwoFactorEnabled = enabled;
        //    return Ok();
        //}
        //#endregion

        //#region IQueryableUserStore
        //public IQueryable<User> Users
        //{
        //    get { return _userRepository.GetAll(); }
        //}
        //#endregion

        //public void Dispose()
        //{
        //    // Dispose does nothing since we want Unity to manage the lifecycle of our Unit of Work
        //}

        //private Task Ok()
        //{
        //    return Task.FromResult(0);
        //}
    }
}
