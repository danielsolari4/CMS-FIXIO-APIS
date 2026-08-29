using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ray.BackendApi.Models;
using Ray.Dtos.Configuration;
using Ray.Dtos.Login;
using Ray.Managers;
using Ray.Model.NewContext.Entities;
using Ray.Utils.Configuration;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;
using Microsoft.Extensions.Options;
using Ray.Utils.Mail;
using Ray.BackendApi.Attributes;
using Newtonsoft.Json;

namespace Ray.BackendApi.Controllers
{
    //[CMSAuthorize]
    [Route("api/Account")]
    public class AccountController : BaseApiController
    {
        private const string LocalLoginProvider = "Local";
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly AppSettings _appSettings;
        private readonly IUserManager _userManagerTwo;
        private readonly SignInManager<User> _signInManager;
        private readonly IOptions<SMTP> _smtpSettings;
        private readonly IMailSender _mailSender;
        private readonly IApplicationUserManager _applicationUserManager;
        public AccountController(IUserManager userManager,
            UserManager<User> identityUserManager,
            AppSettings appSettings, SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            IOptions<SMTP> smtpSettings,
            IMailSender mailSender,
            IApplicationUserManager applicationUserManager)
        {
            _userManager = identityUserManager;
            _appSettings = appSettings;
            _signInManager = signInManager;
            _userManagerTwo = userManager;
            _roleManager = roleManager;
            _smtpSettings = smtpSettings;
            _mailSender = mailSender;
            _applicationUserManager = applicationUserManager;
        }

        //public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }

        //TODO: ver comentado
        //GET api/Account/UserInfo
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [Route("UserInfo")]
        public async Task<UserInfoViewModel> GetUserInfo()
        {
            var externalLogin = ExternalLoginData.FromIdentity(User.Identity as ClaimsIdentity);
            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(ur => ur.RoleActions)
                .ThenInclude(ur => ur.Action)
                .FirstOrDefaultAsync(s => s.Email == User.Identity.Name);

            if (user == null)
                return null;

            #region Get Actions by Role

            //var userManager = ApplicationUserManager.GetInstance();
            //var user = userManager.FindByIdAsync(User.Identity.GetUserId<int>()).Result;
            //var actions = user.Roles.SelectMany(x => x.Actions).Select(s => s.Id).ToArray();

            #endregion

            return new UserInfoViewModel
            {
                Id = user.Id,
                Email = user.UserName,
                HasRegistered = externalLogin == null,
                LoginProvider = externalLogin?.LoginProvider,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Country = user.Country,
                State = user.State,
                City = user.City,
                Address = user.Address,
                ZipCode = user.ZipCode,
                Cellphone = user.CellPhone,
                Description = user.Description,
                TimeZoneId = user.TimeZoneId,
                LanguageId = user.LanguageId,
                ProfileImagePath = user.ProfileImagePath,
                UserRole = string.Join(",", user.UserRoles.Select(s => s.Role.Name).ToArray()),
                Actions = user.UserRoles.SelectMany(x => x.Role.RoleActions.Select(r => r.Action.Code)).ToArray()
            };
        }
        //Actions = user.Roles?.SelectMany(x => x.Actions?.Select(s => s.Code)).ToArray()


        // POST api/Account/Logout
        [Route("Logout")]
        public IActionResult Logout()
            => Ok();

        // GET api/Account/ManageInfo?returnUrl=%2F&generateState=true
        [Route("ManageInfo")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<ManageInfoViewModel> GetManageInfo(string returnUrl, bool generateState = false)
        {
            var user = await _userManager.FindByIdAsync(GetLoggedUser().Id.ToString());

            if (user == null)
            {
                return null;
            }

            var logins = new List<UserLoginInfoViewModel>();

            foreach (var linkedAccount in user.UserLogins)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = linkedAccount.LoginProvider,
                    ProviderKey = linkedAccount.ProviderKey
                });
            }

            if (user.PasswordHash != null)
            {
                logins.Add(new UserLoginInfoViewModel
                {
                    LoginProvider = LocalLoginProvider,
                    ProviderKey = user.UserName,
                });
            }

            return new ManageInfoViewModel
            {
                LocalLoginProvider = LocalLoginProvider,
                Email = user.UserName,
                Logins = logins
                //ExternalLoginProviders = GetExternalLogins(returnUrl, generateState)
            };
        }

        // POST api/Account/ChangePassword
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [HttpPost, Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _userManager.FindByNameAsync((GetLoggedUser().Email.ToString()));
            if (user == null) return BadRequest();

            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword,
                model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/SetPassword
        [HttpPost, Route("SetPassword")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(GetLoggedUser().Id.ToString());
            if (user == null) return BadRequest();


            IdentityResult result = await _userManager.AddPasswordAsync(user, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        //TODO: ver como implementar
        // POST api/Account/AddExternalLogin
        //[Route("AddExternalLogin")]
        //public async Task<IActionResult> AddExternalLogin(AddExternalLoginBindingModel model)
        //{
        //    if (model == null)
        //        ModelState.AddModelError("Model", "GNEX_001");

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    Authentication.SignOut(DefaultAuthenticationTypes.ExternalCookie);

        //    // Fixes Microsoft Web Api Template Error
        //    //AuthenticationTicket ticket = AccessTokenFormat.Unprotect(model.ExternalAccessToken);
        //    AuthenticationTicket ticket = Startup.OAuthOptions.AccessTokenFormat.Unprotect(model.ExternalAccessToken);

        //    if (ticket == null || ticket.Identity == null || (ticket.Properties != null
        //        && ticket.Properties.ExpiresUtc.HasValue
        //        && ticket.Properties.ExpiresUtc.Value < DateTimeOffset.UtcNow))
        //    {
        //        return BadRequest("External login failure.");
        //    }

        //    ExternalLoginData externalData = ExternalLoginData.FromIdentity(ticket.Identity);

        //    if (externalData == null)
        //    {
        //        return BadRequest("The external login is already associated with an account.");
        //    }

        //    IdentityResult result = await IdentityUserManager.AddLoginAsync(User.Identity.GetUserId<int>(),
        //        new UserLoginInfo(externalData.LoginProvider, externalData.ProviderKey));

        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    return Ok();
        //}

        // POST api/Account/RemoveLogin

        [HttpPost, Route("RemoveLogin")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> RemoveLogin(RemoveLoginBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            IdentityResult result;

            if (model.LoginProvider == LocalLoginProvider)
            {
                var user = await _userManager.FindByIdAsync(GetLoggedUser().Id.ToString());
                if (user == null) return BadRequest();

                result = await _userManager.RemovePasswordAsync(user);
            }
            else
            {
                var user = await _userManager.FindByIdAsync((GetLoggedUser().Id.ToString()));
                if (user == null) return BadRequest();
                result = await _userManager.RemoveLoginAsync(user,
                    model.LoginProvider, model.ProviderKey);
            }

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto login)
        {
            IActionResult response = Unauthorized();
            var result = await Authenticate(login?.Email ?? login?.UserName, login?.Password);
            var user = result.User;

            if (user != null)
            {
                var tokenString = await BuildToken(user);
                response = Ok(new
                {
                    token = tokenString,
                    userId = user.Id,
                    user = new
                    {
                        username = user.UserName,
                        firstname = user.FirstName,
                        lastname = user.LastName,
                        twoFactorEnabled = user.TwoFactorEnabled
                    }
                });
            }

            return response;
        }

        [HttpPost("/Token")]
        [AllowAnonymous]
        public async Task<IActionResult> Token([FromForm] LegacyTokenRequest request)
        {
            if (request == null || (!string.IsNullOrWhiteSpace(request.GrantType) && request.GrantType != "password"))
                return BadRequest(new { error = "unsupported_grant_type" });

            var result = await Authenticate(request.UserName, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { error = result.Error, error_description = result.Description });

            var tokenString = await BuildToken(result.User);
            return Ok(new
            {
                access_token = tokenString,
                token_type = "bearer",
                expires_in = (int)TimeSpan.FromDays(_appSettings.Jwt.ExpiryDays).TotalSeconds,
                userName = result.User.UserName,
                twoFactorEnabled = result.User.TwoFactorEnabled.ToString()
            });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [HttpGet("[action]")]
        public IActionResult ValidToken()
        {
            return Ok();
        }

        private async Task<string> BuildToken(User user)
        {
            var rolesInfo =  _applicationUserManager.GetControllerActionUserAllowedList(user);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(rolesInfo)),   
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            user.UserRoles.ToList().ForEach(role =>
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            });

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _appSettings.Jwt.Issuer,
                _appSettings.Jwt.Issuer,
                claims,
                expires: DateTime.Now.AddDays(_appSettings.Jwt.ExpiryDays),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        [HttpPost, Route("SaveTwoFactorCode")]
        [AllowAnonymous]
        public async Task<IActionResult>  SaveTwoFactorCode(TwoFactorModel model)
        {
            //UserManager.
            if (string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Code)) return BadRequest();

            var user = await _userManager.FindByEmailAsync(model.UserName);
            user.Imei = model.Code;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { success = true });

            return BadRequest();
        }

        [HttpPost, Route("ChangeTwoFactorEnabled")]
        [AllowAnonymous]
        public async Task<IActionResult>  ChangeTwoFactorEnabled(TwoFactorUserModel model)
        {
            //UserManager.
            if (string.IsNullOrEmpty(model.UserName)) return BadRequest();

            var user = await _userManager.FindByEmailAsync(model.UserName);
            if (user == null) return BadRequest();
            user.TwoFactorEnabled = true;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { success = true });

            return BadRequest();
        }


        [HttpGet, Route("GetTwoFactorCode")]
        [AllowAnonymous]
        public async Task<IActionResult>  GetTwoFactorCode(string userName)
        {
            var user = await _userManager.FindByEmailAsync(userName);
            return Ok(new { success = true, code = user.Imei });
        }


        [NonAction]
        public async Task<List<Role>> GetUserRoles(User user)
        {
            //var list = new List<Role>();
            return user.UserRoles.Select(x => x.Role)?.ToList();
            //foreach (var it in roles)
            //{
            //    list.Add(it);
            //}
            //return list;
        }


        private async Task<AuthenticationResult> Authenticate(string userNameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(userNameOrEmail) || string.IsNullOrWhiteSpace(password))
                return AuthenticationResult.Failed("invalid_grant", "The user name or password is incorrect.");

            var user = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RoleActions)
                .ThenInclude(ra => ra.Action)
                .FirstOrDefaultAsync(s => s.UserName == userNameOrEmail || s.Email == userNameOrEmail);

            if (user == null)
                return AuthenticationResult.Failed("invalid_grant", "The user name or password is incorrect.");

            if (user.LockoutEndDateUtc.HasValue && DateTime.SpecifyKind(user.LockoutEndDateUtc.Value, DateTimeKind.Utc) > DateTime.UtcNow)
                return AuthenticationResult.Failed("locked_out", "The user account is locked. Try again later.");

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                user.AccessFailedCount++;
                if (user.LockoutEnabled && user.AccessFailedCount >= 5)
                    user.LockoutEndDateUtc = DateTime.UtcNow.AddHours(4);

                await _userManager.UpdateAsync(user);

                if (user.LockoutEndDateUtc.HasValue && user.LockoutEndDateUtc.Value > DateTime.UtcNow)
                    return AuthenticationResult.Failed("locked_out", "The user account has been locked due to too many failed login attempts.");

                return AuthenticationResult.Failed("invalid_grant", "The user name or password is incorrect.");
            }

            user.AccessFailedCount = 0;
            await _userManager.UpdateAsync(user);

            if (!user.IsEnabled || user.IsDeleted)
                return AuthenticationResult.Failed("invalid_grant", "The user name or password is incorrect.");

            System.Threading.Thread.CurrentPrincipal = new GenericPrincipal(HttpContext.User.Identity, user.UserRoles.Select(x => x.Role.Name).ToArray());
            return AuthenticationResult.Success(user);
        }

        private async Task<string> GenerateTokenByUser(User user)
        {
            try
            {
                var passwordToke = await _userManager.GeneratePasswordResetTokenAsync(user);
                return HttpUtility.UrlEncode(passwordToke);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        // GET api/Account/ExternalLogins?returnUrl=%2F&generateState=true

        //TODO: ver como implementar
        //[AllowAnonymous]
        //[Route("ExternalLogins")]
        //public IEnumerable<ExternalLoginViewModel> GetExternalLogins(string returnUrl, bool generateState = false)
        //{
        //    IEnumerable<AuthenticationDescription> descriptions = Authentication.GetExternalAuthenticationTypes();
        //    List<ExternalLoginViewModel> logins = new List<ExternalLoginViewModel>();

        //    string state;

        //    if (generateState)
        //    {
        //        const int strengthInBits = 256;
        //        state = RandomOAuthStateGenerator.Generate(strengthInBits);
        //    }
        //    else
        //    {
        //        state = null;
        //    }

        //    foreach (AuthenticationDescription description in descriptions)
        //    {
        //        ExternalLoginViewModel login = new ExternalLoginViewModel
        //        {
        //            Name = description.Caption,
        //            Url = Url.Route("ExternalLogin", new
        //            {
        //                provider = description.AuthenticationType,
        //                response_type = "token",
        //                client_id = Startup.PublicClientId,
        //                redirect_uri = new Uri(Request.RequestUri, returnUrl).AbsoluteUri,
        //                state = state
        //            }),
        //            State = state
        //        };
        //        logins.Add(login);
        //    }

        //    return logins;
        //}

        #region Custom Methods

        [Route("CreateUserRequest")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> CreateUserRequest(UserRequestBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            var user = await _userManager.FindByNameAsync(model.Email);

            if (user == null)
            {
                var u = new User()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Country = model.Country,
                    State = model.State,
                    City = model.City,
                    Address = model.Address,
                    ZipCode = model.ZipCode,
                    CellPhone = model.Cellphone,
                    Description = model.Description,
                    LanguageId = model.LanguageId,
                    TimeZoneId = model.TimeZoneId,
                    IsEnabled = false,
                    EmailConfirmed = false,
                    CacheSolr = false,
                    Discriminator = UserDiscriminator.Backend
                };

                IdentityResult result = await _userManager.CreateAsync(u);

                if (result.Succeeded)
                {
                    user = await _userManager.FindByNameAsync(u.UserName);
                    await _userManager.AddToRolesAsync(user, model.Roles.ToArray());
                    var token = await _userManager.GenerateUserTokenAsync(user, "Invitation", "EmailConfirmation");
                    await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);

                    var parameters = new Dictionary<string, string>();
                    parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
                    parameters.Add("*|Url|*", string.Format("{0}?userid={1}&token={2}", _appSettings.Admin.ConfirmUserRequestUrl, user.Id, HttpUtility.UrlEncode(token)));
                    parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                    parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);

                    var emailResult = await _mailSender.Send(EmailTemplateType.CreateUserRequest, user.Email, "Bienvenido a Ray.media",
                        parameters, System.Net.Mail.MailPriority.Normal);

                    if (!emailResult)
                    {
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError("Email", "USEX_011");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    return GetErrorResult(result);
                }
            }
            else
            {
                if (user.IsDeleted || user.Discriminator.Equals(UserDiscriminator.Frontend))
                {
                    user.IsDeleted = false;
                    user.IsEnabled = false;
                    user.CacheSolr = false;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;

                    user.Discriminator = UserDiscriminator.Backend;

                    await _userManager.UpdateAsync(user);
                    await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
                    var token = await _userManager.GenerateUserTokenAsync(user, "Invitation", "EmailConfirmation");

                    var parameters = new Dictionary<string, string>();
                    parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
                    parameters.Add("*|Url|*", string.Format("{0}?userid={1}&token={2}", _appSettings.Admin.ConfirmUserRequestUrl, user.Id, HttpUtility.UrlEncode(token)));
                    parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                    parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);

                    var emailResult = await _mailSender.Send(EmailTemplateType.CreateUserRequest, user.Email, "Bienvenido a Ray.media",
                        parameters, System.Net.Mail.MailPriority.Normal);

                    if (!emailResult)
                    {
                        await _userManager.DeleteAsync(user);
                        ModelState.AddModelError("Email", "USEX_011");
                        return BadRequest(ModelState);
                    }
                }
                else
                {
                    ModelState.AddModelError("Model", "USEX_001");
                    return BadRequest(ModelState);
                }
            }

            return Ok(CMSResponse(await _userManagerTwo.GetById(user.Id)));
        }

        [Route("ResendUserRequest")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        public async Task<IActionResult> ResendUserRequest(ResendUserRequestBindingModel model)
        {
            var currentUser = await _userManager.FindByNameAsync(model.UserName);

            if (currentUser != null && !currentUser.EmailConfirmed)
            {
                var token = await _userManager.GenerateUserTokenAsync(currentUser, "Invitation", "EmailConfirmation");

                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", currentUser.FirstName + " " + currentUser.LastName);
                parameters.Add("*|Url|*", string.Format("{0}?userid={1}&token={2}", _appSettings.Admin.ConfirmUserRequestUrl, currentUser.Id, HttpUtility.UrlEncode(token)));
                parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);
                await _mailSender.Send(EmailTemplateType.CreateUserRequest, currentUser.Email, "Bienvenido a Ray.media", 
                    parameters, System.Net.Mail.MailPriority.Normal);

                return Ok();
            }

            ModelState.AddModelError("Model", "USEX_002");
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [Route("GetUserRequest")]
        public async Task<IActionResult> GetUserRequest(int userId, string token)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(token))
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return BadRequest();

            if (await _userManager.VerifyUserTokenAsync(user, "Invitation", "EmailConfirmation", token))
            {
                var userInfo = new UserInfoViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Country = user.Country,
                    State = user.State,
                    City = user.City,
                    Address = user.Address,
                    ZipCode = user.ZipCode,
                    Cellphone = user.CellPhone,
                    Description = user.Description,
                    TimeZoneId = user.TimeZoneId,
                    LanguageId = user.LanguageId
                };

                return Ok(new { data = userInfo });
            }

            ModelState.AddModelError("Model", "USEX_003");

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [Route("ConfirmUserRequest")]
        public async Task<IActionResult> ConfirmUserRequest(UserRequestConfirmationBindingModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userToConfirm = await _userManager.FindByEmailAsync(model.Email);

            if (userToConfirm == null)
            {
                ModelState.AddModelError("Model", "USEX_004");
                return BadRequest(ModelState);
            }

            if (await _userManager.VerifyUserTokenAsync(userToConfirm, "Invitation", "EmailConfirmation", model.Token))
            {
                userToConfirm.FirstName = model.FirstName;
                userToConfirm.LastName = model.LastName;
                userToConfirm.Country = model.Country;
                userToConfirm.State = model.State;
                userToConfirm.City = model.City;
                userToConfirm.Address = model.Address;
                userToConfirm.ZipCode = model.ZipCode;
                userToConfirm.CellPhone = model.Cellphone;
                userToConfirm.Description = model.Description;
                userToConfirm.IsEnabled = true;
                userToConfirm.EmailConfirmed = true;
                userToConfirm.CacheSolr = false;
                userToConfirm.LanguageId = model.LanguageId;
                userToConfirm.TimeZoneId = model.TimeZoneId;
                await _userManager.AddPasswordAsync(userToConfirm, model.Password);
            }
            else
            {
                ModelState.AddModelError("Model", "USEX_005");
                return BadRequest(ModelState);
            }

            IdentityResult result = await _userManager.UpdateAsync(userToConfirm);

            if (result.Succeeded)
            {
                await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);

                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", userToConfirm.FirstName + " " + userToConfirm.LastName);
                parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);


                await _mailSender.Send(EmailTemplateType.ConfirmUserRequest, userToConfirm.Email, "Tu cuenta ha sido activada",
                    parameters, System.Net.Mail.MailPriority.Normal);
            }
            else
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        [AllowAnonymous]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Model", "USEX_006");
                return BadRequest(ModelState);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var parameters = new Dictionary<string, string>();
            parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
            parameters.Add("*|Url|*", string.Format("{0}?email={1}&token={2}", _appSettings.Admin.ResetPasswordRequestUrl, HttpUtility.UrlEncode(user.Email), HttpUtility.UrlEncode(token)));
            parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
            parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);
            await _mailSender.Send(EmailTemplateType.ResetPassword, user.Email, "Solicitud cambio de contraseña", parameters, System.Net.Mail.MailPriority.Normal);

            return Ok();
        }

        [AllowAnonymous]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("Model", "USEX_006");
                return BadRequest(ModelState);
            }

            IdentityResult result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (result.Succeeded)
            {
                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
                parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);

                await _mailSender.Send(EmailTemplateType.ForgotPasswordConfirmation, user.Email, "Contraseña actualizada",
                    parameters, System.Net.Mail.MailPriority.Normal);
            }
            else
            {
                return GetErrorResult(result);
            }

            return Ok();
        }



        #endregion

        [AllowAnonymous]
        [HttpGet]
        [Route("CheckAuthentication")]
        public IActionResult VerifyToken(string userId, string apikey)
        {

            //TODO: ver como implementar y si se usa
            //var api = SecurityExtension.Decrypt(apikey);
            //if (api == ConfigurationManager.AppSettings["CMS.Auth.API"])
            //{
            //    var u = int.Parse(SecurityExtension.Decrypt(userId));

            //    var userManager = ApplicationUserManager.GetInstance();
            //    var user = userManager.Users.FirstOrDefaultAsync(x => x.Id == u).Result;

            //    if (user == null || !user.IsEnabled || !userManager.Store.HasAccessToActionAsync(user, "Account", "TokenAnalitycs").Result)
            //        return Unauthorized();

            //    return Ok();
            //}
            //else
            //{
            //    return Unauthorized();
            //}
            return Unauthorized();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [Route("Register")]
        public async Task<IActionResult> Register(RegisterBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User()
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Country = model.Country,
                State = model.State,
                City = model.City,
                Address = model.Address,
                ZipCode = model.ZipCode,
                CellPhone = model.Cellphone,
                Description = model.Description,
                LanguageId = model.LanguageId,
                TimeZoneId = model.TimeZoneId,
                IsEnabled = true
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var currentUser = await _userManager.FindByNameAsync(user.UserName);
                await _userManager.AddToRoleAsync(currentUser, "Admin");
            }
            else
            {
                return GetErrorResult(result);
            }

            return Ok();
        }

        // POST api/Account/RegisterExternal
        //TODO:ver como implementar y si se usa
        //[OverrideAuthentication]
        //[HostAuthentication(DefaultAuthenticationTypes.ExternalBearer)]
        //[Route("RegisterExternal")]
        //public async Task<IActionResult> RegisterExternal(RegisterExternalBindingModel model)
        //{
        //    if (model == null)
        //        ModelState.AddModelError("Model", "GNEX_001");

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var info = await Authentication.GetExternalLoginInfoAsync();
        //    if (info == null)
        //    {
        //        return InternalServerError();
        //    }

        //    var user = new User() { UserName = model.Email, Email = model.Email };

        //    IdentityResult result = await _userManager.CreateAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }

        //    result = await _userManager.AddLoginAsync(user.Id, info.Login);
        //    if (!result.Succeeded)
        //    {
        //        return GetErrorResult(result);
        //    }
        //    return Ok();
        //}

        // POST api/Account/ResendVerificationFE
        [AllowAnonymous]
        [Route("ResendVerificationFE")]
        public async Task<IActionResult> ResendVerificationFE(ResendUserRequestBindingModel model)
        {
            var currentUser = await _userManager.FindByNameAsync(model.UserName);

            if (currentUser != null && !currentUser.EmailConfirmed)
            {
                var token = await _userManager.GenerateUserTokenAsync(currentUser, "Confirmation", "Create");

                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", currentUser.FirstName + " " + currentUser.LastName);
                parameters.Add("*|Url|*", string.Format("{0}?email={1}&token={2}", _appSettings.Admin.ConfirmUserRequestFEUrl, HttpUtility.UrlEncode(currentUser.Email), HttpUtility.UrlEncode(token)));

                await _mailSender.Send(EmailTemplateType.ResendInvitation, currentUser.Email, $"Bienvenido a {_smtpSettings.Value.FromDisplayName}", 
                    parameters, System.Net.Mail.MailPriority.Normal);
                return Ok();
            }

            ModelState.AddModelError("Model", "USEX_002");
            return BadRequest(ModelState);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && _userManager != null)
        //    {
        //        _userManager.Dispose();
        //        _userManager = null;
        //    }

        //    base.Dispose(disposing);
        //}

        #region Helpers


        private IActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return BadRequest();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

            public IList<Claim> GetClaims()
            {
                IList<Claim> claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.NameIdentifier, ProviderKey, null, LoginProvider));

                if (UserName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserName, null, LoginProvider));
                }

                return claims;
            }

            public static ExternalLoginData FromIdentity(ClaimsIdentity identity)
            {
                if (identity == null)
                {
                    return null;
                }

                Claim providerKeyClaim = identity.FindFirst(ClaimTypes.NameIdentifier);

                if (providerKeyClaim == null || String.IsNullOrEmpty(providerKeyClaim.Issuer)
                    || String.IsNullOrEmpty(providerKeyClaim.Value))
                {
                    return null;
                }

                if (providerKeyClaim.Issuer == ClaimsIdentity.DefaultIssuer)
                {
                    return null;
                }

                return new ExternalLoginData
                {
                    LoginProvider = providerKeyClaim.Issuer,
                    ProviderKey = providerKeyClaim.Value,
                    //UserName = identity.FindFirstValue(ClaimTypes.Name),
                };
            }
        }

        private static class RandomOAuthStateGenerator
        {
            private static RandomNumberGenerator _random = new RNGCryptoServiceProvider();

            public static string Generate(int strengthInBits)
            {
                const int bitsPerByte = 8;

                if (strengthInBits % bitsPerByte != 0)
                {
                    throw new ArgumentException("strengthInBits must be evenly divisible by 8.", "strengthInBits");
                }

                int strengthInBytes = strengthInBits / bitsPerByte;

                byte[] data = new byte[strengthInBytes];
                _random.GetBytes(data);
                //return HttpServerUtility.UrlTokenEncode(data);
                return null;
            }
        }

        public class LegacyTokenRequest
        {
            [FromForm(Name = "grant_type")]
            public string GrantType { get; set; }

            [FromForm(Name = "username")]
            public string UserName { get; set; }

            [FromForm(Name = "password")]
            public string Password { get; set; }
        }

        private class AuthenticationResult
        {
            public bool Succeeded { get; set; }
            public User User { get; set; }
            public string Error { get; set; }
            public string Description { get; set; }

            public static AuthenticationResult Success(User user)
                => new AuthenticationResult { Succeeded = true, User = user };

            public static AuthenticationResult Failed(string error, string description)
                => new AuthenticationResult { Succeeded = false, Error = error, Description = description };
        }

        #endregion
    }
}
