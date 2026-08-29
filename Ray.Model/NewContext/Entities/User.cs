using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class User : IdentityUser<int>
    {
        public User()
        {
            UserAssets = new HashSet<UserAsset>();
            UserClaims = new HashSet<UserClaim>();
            UserLogins = new HashSet<UserLogin>();
            UserNodes = new HashSet<UserNode>();
            UserRoles = new HashSet<UserRole>();
        }

        public DateTime? LockoutEndDateUtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string CellPhone { get; set; }
        public int LanguageId { get; set; }
        public int TimeZoneId { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string ProfileImagePath { get; set; }
        public int? MigrationId { get; set; }
        public string FacebookId { get; set; }
        public string TwitterId { get; set; }
        public string Discriminator { get; set; }
        public string IdentificationNumber { get; set; }
        public byte? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Neighborhood { get; set; }
        public string Reference { get; set; }
        public string Imei { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual Language Language { get; set; }
        public virtual TimeZone TimeZone { get; set; }
        public virtual ICollection<UserAsset> UserAssets { get; set; }
        public virtual ICollection<UserClaim> UserClaims { get; set; }
        public virtual ICollection<UserLogin> UserLogins { get; set; }
        public virtual ICollection<UserNode> UserNodes { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
