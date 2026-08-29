using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ray.Dtos
{
    public class UserDto : BaseDto
    {
        [StringLength(256, ErrorMessage = "Email max length is 256")]
        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        [StringLength(256, ErrorMessage = "UserName max length is 256")]
        public string UserName { get; set; }

        [StringLength(100, ErrorMessage = "FirstName max length is 100")]
        public string FirstName { get; set; }

        [StringLength(100, ErrorMessage = "LastName max length is 100")]
        public string LastName { get; set; }

        [StringLength(100, ErrorMessage = "Country max length is 100")]
        public string Country { get; set; }

        [StringLength(100, ErrorMessage = "State max length is 100")]
        public string State { get; set; }

        [StringLength(100, ErrorMessage = "City max length is 100")]
        public string City { get; set; }

        [StringLength(256, ErrorMessage = "Address max length is 256")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "ZipCode max length is 50")]
        public string ZipCode { get; set; }

        [StringLength(100, ErrorMessage = "CellPhone max length is 100")]
        public string CellPhone { get; set; }

        public virtual int LanguageId { get; set; }

        public virtual int TimeZoneId { get; set; }

        public string Description { get; set; }

        public List<string> Roles { get; set; }

        public bool IsEnabled { get; set; }

        public bool EmailConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public string ProfileImagePath { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        [StringLength(50, ErrorMessage = "IdentificationNumber max length is 50")]
        public string IdentificationNumber { get; set; }

        [Range(0, 4, ErrorMessage = "Gender")]
        public byte? Gender { get; set; }

        public DateTime? BirthDate { get; set; }

        [StringLength(300, ErrorMessage = "Neighborhood max length is 300")]
        public string Neighborhood { get; set; }

        public string Reference { get; set; }

        [StringLength(50, ErrorMessage = "IMEI max length is 50")]
        public string IMEI { get; set; }

        public List<NodeDto> Node { get; set; }

        public List<AssetDto> Assets { get; set; }

        public UserDto()
        {
            Roles = new List<string>();
            Node = new List<NodeDto>();
            Assets = new List<AssetDto>();
        }
    }

    public class CreateUserDtoBindingModel : UserDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "LanguageId")]
        public override int LanguageId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "TimeZoneId")]
        public override int TimeZoneId { get; set; }
    }

    public class UpdateUserDtoBindingModel : UserDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }
    }

    public class UpdateUserPreferencesDtoBindingModel : UserDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "NodeId")]
        public int NodeId { get; set; }
    }
    public class UpdateUserBookmarksDtoBindingModel : UserDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Id")]
        public override int Id { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "AssetId")]
        public int AssetId { get; set; }
    }
}
