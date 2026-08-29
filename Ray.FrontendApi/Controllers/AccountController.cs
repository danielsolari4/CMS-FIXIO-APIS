using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Dtos.Login;
using Ray.FrontendApi.Models;
using Ray.Managers;
using Ray.Model.NewContext.Entities;
using Ray.Utils.Mail;
using Ray.Utils.Solr;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Ray.FrontendApi.Controllers
{
    [Route("api/Account")]
    public class AccountController : BaseApiController
    {
        private const string LocalLoginProvider = "Local";
        private readonly UserManager<User> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IUserManager _userManagerTwo;
        private readonly SignInManager<User> _signInManager;
        private readonly IMailSender _mailSender;
        private readonly IPaywallManager _paywallManager;
        private readonly IApplicationUserManager _applicationUserManager;
        public AccountController(IUserManager userManager,
            UserManager<User> identityUserManager,
            AppSettings appSettings, SignInManager<User> signInManager,
            IMailSender mailSender,
            IPaywallManager paywallManager,
            IApplicationUserManager applicationUserManager)
        {
            _userManager = identityUserManager;
            _appSettings = appSettings;
            _signInManager = signInManager;
            _userManagerTwo = userManager;
            _mailSender = mailSender;
            _paywallManager = paywallManager;
            _applicationUserManager = applicationUserManager;
        }

        // GET api/Account/UserInfo
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
                .FirstOrDefaultAsync(s => s.Email == GetLoggedUser().Email);

            if (user == null)
                return null;

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
                ProfileImagePath = user.ProfileImagePath
            };
        }

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
            };
        }


        // POST api/Account/ChangePassword
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _userManager.FindByNameAsync((GetLoggedUser().Email.ToString()));
            if (user == null) return BadRequest();

            IdentityResult result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);

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
                return GetErrorResult(result);

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

                await _mailSender.Send(EmailTemplateType.ForgotPasswordConfirmation, user.Email, "Contraseña actualizada",
                    parameters, System.Net.Mail.MailPriority.Normal);
            }
            else
            {
                return GetErrorResult(result);
            }

            return Ok();
        }


        [AllowAnonymous]
        [Route("ConfirmUser")]
        public async Task<IActionResult> ConfirmUser(UserConfirmationBindingModel model)
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
                userToConfirm.IsEnabled = true;
                userToConfirm.EmailConfirmed = true;
                userToConfirm.CacheSolr = false;
            }
            else
            {
                ModelState.AddModelError("Model", "USEX_005");
                return BadRequest(ModelState);
            }

            IdentityResult result = await _userManager.UpdateAsync(userToConfirm);

            if (result.Succeeded)
            {
                await SolrHelper.DataImport(Utils.Solr.SolrCore.USER, _appSettings.Solr);
                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", userToConfirm.FirstName + " " + userToConfirm.LastName);
                try
                {
                    await _mailSender.Send(EmailTemplateType.ConfirmUserRequest, userToConfirm.Email, "Tu cuenta ha sido activada",
                        parameters, System.Net.Mail.MailPriority.Normal);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Ok();
                }
            }
            else
            {
                return GetErrorResult(result);
            }

            return Ok();
        }


        // POST api/Account/Register
        [AllowAnonymous]
        [HttpPost, Route("Register")]
        public async Task<IActionResult> Register(RegisterBindingModel model)
        {
            try
            {
                if (model == null)
                    ModelState.AddModelError("Model", "GNEX_001");

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                User user = null;
                UserDto userDto = null;

                if (!string.IsNullOrWhiteSpace(model.Email))
                    user = await _userManager.FindByEmailAsync(model.Email);

                if (!string.IsNullOrWhiteSpace(model.FacebookId) || !string.IsNullOrWhiteSpace(model.TwitterId))
                    userDto = (await _userManagerTwo.GetByExternalId(model.FacebookId, model.TwitterId));

                if (user == null && userDto == null)
                {
                    var u = new User
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
                        Discriminator = UserDiscriminator.Frontend,
                        ProfileImagePath = model.ProfileImagePath
                    };

                    if (string.IsNullOrWhiteSpace(model.FacebookId) && string.IsNullOrWhiteSpace(model.TwitterId))
                    {
                        // form user creation
                        if (string.IsNullOrWhiteSpace(model.Email))
                        {
                            ModelState.AddModelError("Email", "USEX_009");
                            return BadRequest(ModelState);
                        }

                        if (string.IsNullOrWhiteSpace(model.Password))
                        {
                            ModelState.AddModelError("Password", "USEX_010");
                            return BadRequest(ModelState);
                        }

                        IdentityResult result = await _userManager.CreateAsync(u, model.Password);

                        if (result.Succeeded)
                        {
                            user = await _userManager.FindByNameAsync(u.UserName);
                            await _userManager.AddToRoleAsync(user, "User");
                            var token = await _userManager.GenerateUserTokenAsync(user, "Invitation", "EmailConfirmation");
                            await SolrHelper.DataImport(Utils.Solr.SolrCore.USER, _appSettings.Solr);

                            var parameters = new Dictionary<string, string>();
                            parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
                            parameters.Add("*|Url|*", string.Format("{0}?userid={1}&token={2}", _appSettings.Admin.ConfirmUserRequestFEUrl, user.Id, HttpUtility.UrlEncode(token)));
                            var emailResult = await _mailSender.Send(EmailTemplateType.CreateUserRequest, user.Email, $"Bienvenido a {_appSettings.Smtp.FromDisplayName}", parameters, System.Net.Mail.MailPriority.Normal);

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
                        // fb/tw user creation
                        u.IsEnabled = true;
                        u.EmailConfirmed = true;
                        u.FacebookId = model.FacebookId;
                        u.TwitterId = model.TwitterId;

                        if (!string.IsNullOrWhiteSpace(model.ProfileImagePath))
                            u.ProfileImagePath = model.ProfileImagePath;

                        if (string.IsNullOrWhiteSpace(model.Email))
                            u.UserName = !string.IsNullOrWhiteSpace(model.TwitterId) ? model.TwitterId : model.FacebookId;

                        IdentityResult result = await _userManager.CreateAsync(u);

                        if (result.Succeeded)
                        {
                            var currentUser = await _userManager.FindByNameAsync(u.UserName);
                            await _userManager.AddToRoleAsync(currentUser, "User");
                            await SolrHelper.DataImport(Ray.Utils.Solr.SolrCore.USER, _appSettings.Solr);
                        }
                        else
                        {
                            return GetErrorResult(result);
                        }
                    }
                }
                else if (user != null && userDto == null && (!string.IsNullOrWhiteSpace(model.TwitterId) || !string.IsNullOrWhiteSpace(model.FacebookId)))
                {
                    if (user.IsDeleted)
                    {
                        user.IsDeleted = false;
                        user.IsEnabled = true;
                    }

                    if (!string.IsNullOrWhiteSpace(model.FacebookId))
                        user.FacebookId = model.FacebookId;
                    if (!string.IsNullOrWhiteSpace(model.TwitterId))
                        user.TwitterId = model.TwitterId;

                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    ModelState.AddModelError("Model", "USEX_001");
                    return BadRequest(ModelState);
                }

                return Ok();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(FrontLoginDto login)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await Authenticate(login);

            if (user == null || user.IsDeleted)
            {
                ModelState.AddModelError("Model", "The user name or password is incorrect.");
                return BadRequest(ModelState);
            }

            if (!user.IsEnabled && !user.IsDeleted)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var parameters = new Dictionary<string, string>();
                parameters.Add("*|Fullname|*", user.FirstName + " " + user.LastName);
                parameters.Add("*|Url|*", string.Format("{0}?email={1}&token={2}", _appSettings.Admin.ResetPasswordRequestUrl, HttpUtility.UrlEncode(user.Email), HttpUtility.UrlEncode(token)));
                parameters.Add("*|MediaUrl|*", _appSettings.Content.ImageUrl);
                parameters.Add("*|AdminWebUrl|*", _appSettings.Content.AdminUrl);
                await _mailSender.Send(EmailTemplateType.ResetPassword, user.Email, "Solicitud cambio de contraseña", parameters, System.Net.Mail.MailPriority.Normal);
                ModelState.AddModelError("Model", "The user needs to reset password");
                return BadRequest(ModelState);
            }

            if (user != null)
            {
                var tokenString = await GenerateLocalAccessTokenResponse(user, false);
                return Ok(tokenString.ToString());
            }

            return Unauthorized();
        }

        private async Task<User> Authenticate(FrontLoginDto login)
        {
            var user = await _userManager.Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(s => s.Email == login.Username);

            if (user == null) return null;

            var result = await _signInManager.PasswordSignInAsync(user,
                login.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
                return user;

            return null;
        }

        [AllowAnonymous]
        [Route("ExternalSignIn")]
        public async Task<IActionResult> ExternalSignIn(ExternalLogInBindingModel model)
        {
            if (model == null)
                ModelState.AddModelError("Model", "GNEX_001");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userDto = (await _userManagerTwo.GetByExternalId(model.FacebookId, model.TwitterId));

            if (userDto != null)
            {
                var user = await _userManager.FindByNameAsync(userDto.UserName);

                if (user != null)
                {
                    if (user.IsDeleted)
                    {
                        user.IsDeleted = false;
                        user.IsEnabled = true;
                        await _userManager.UpdateAsync(user);
                    }

                    var t = await GenerateLocalAccessTokenResponse(user, false);
                    return Ok(t.ToString());
                }
            }

            var tokenString = await GenerateLocalAccessTokenResponse(null, true);
            return Ok(tokenString.ToString());
        }

        [AllowAnonymous]
        [Route("PaywallSignIn")]
        public async Task<IActionResult> PaywallSignIn(PaywallLoginDto model)
        {
            return await TryJsonResultAsync(async () =>
            {
                var loginResult = await _paywallManager.Login(model);

                if (loginResult == null)
                    return NotFound();

                return Ok(CMSResponse(loginResult));
            });
        }

        private async Task<JObject> GenerateLocalAccessTokenResponse(User user, bool needsRegistration)
        {
            if (user == null || needsRegistration)
                return new JObject(new JProperty("needsRegistration", needsRegistration));

            if (string.IsNullOrEmpty(user.SecurityStamp))
                user.SecurityStamp = new Guid().ToString();

            await _signInManager.SignInAsync(user, true);

            var rolesInfo = _applicationUserManager.GetControllerActionUserAllowedList(user);

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.UserData, JsonConvert.SerializeObject(rolesInfo)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            user.UserRoles.ToList().ForEach(role =>
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Role.Name));
            });

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.Now.AddDays(14);
            var issued = DateTime.Now;

            var token = new JwtSecurityToken(
                _appSettings.Jwt.Issuer,
                _appSettings.Jwt.Issuer,
                claims,
                expires: expiration,
                signingCredentials: creds);

            return new JObject(
                new JProperty("access_token", new JwtSecurityTokenHandler().WriteToken(token)),
                new JProperty("token_type", "bearer"),
                new JProperty("expires_in", (expiration - issued).TotalSeconds.ToString()),
                new JProperty(".issued", issued.ToUniversalTime().ToString()),
                new JProperty(".expires", expiration.ToUniversalTime().ToString()));
        }


        private class ExternalLoginData
        {
            public string LoginProvider { get; set; }
            public string ProviderKey { get; set; }
            public string UserName { get; set; }

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
    }
}