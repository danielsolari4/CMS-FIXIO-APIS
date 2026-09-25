using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rino.Dtos.Configuration;
using System;
namespace Rino.BackendApi.CustomTokenProviders
{
    public class InvitationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public InvitationTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<InvitationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger, AppSettings _appSettings) : base(dataProtectionProvider, options, logger)
        {
            options.Value.TokenLifespan = TimeSpan.FromHours(_appSettings.TokenSettings.UserConfirmationDuration);
        }
    }

    public class InvitationTokenProviderOptions : DataProtectionTokenProviderOptions
    {
        public InvitationTokenProviderOptions()
        {
            Name = "Invitation";
        }
    }
}
