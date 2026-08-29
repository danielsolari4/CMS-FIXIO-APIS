using System;
using System.Collections.Generic;

namespace Ray.FrontendApi.Models
{
    // Models returned by AccountController actions.

    public class ExternalLoginViewModel
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public string State { get; set; }
    }

    public class ManageInfoViewModel
    {
        public string LocalLoginProvider { get; set; }

        public string Email { get; set; }

        public IEnumerable<UserLoginInfoViewModel> Logins { get; set; }

        public IEnumerable<ExternalLoginViewModel> ExternalLoginProviders { get; set; }
    }

    public class UserInfoViewModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public bool HasRegistered { get; set; }

        public string LoginProvider { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string Address { get; set; }

        public string ZipCode { get; set; }

        public string Cellphone { get; set; }

        public string Description { get; set; }

        public int TimeZoneId { get; set; }

        public int LanguageId { get; set; }

        public string ProfileImagePath { get; set; }

        public string IdentificationNumber { get; set; }

        public byte? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        public string Neighborhood { get; set; }

        public string Reference { get; set; }

        public DateTime CreationDate { get; set; }
    }

    public class UserLoginInfoViewModel
    {
        public string LoginProvider { get; set; }

        public string ProviderKey { get; set; }
    }
}
