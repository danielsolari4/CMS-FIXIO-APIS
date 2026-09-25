using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rino.Dtos.Configuration;
using System;

namespace Rino.FrontendApi.CustomTokenProviders
{
    public class ResetPasswordTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public ResetPasswordTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<ResetPasswordTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger, AppSettings _appSettings) : base(dataProtectionProvider, options, logger)
        {
            options.Value.TokenLifespan = TimeSpan.FromHours(_appSettings.TokenSettings.ResetPasswordDuration);
        }
    }

    public class ResetPasswordTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public ResetPasswordTokenProviderOptions()
        {
            Name = "ResetPassword";
        }
    }
}
