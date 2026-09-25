using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rino.Dtos
{
    public class PaywallLoginDto
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class PaywallResponseDto<T>
    {
        [JsonProperty("data")]
        public T Data { get; set; }
    }

    public class PaywallUserDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("first_name")]
        public string First_Name { get; set; }
        [JsonProperty("last_name")]
        public string Last_Name { get; set; }
        [JsonProperty("avatar")]
        public string Avatar { get; set; }
        [JsonProperty("phone_prefix")]
        public string Phone_Prefix { get; set; }
        [JsonProperty("phone_number")]
        public string Phone_Number { get; set; }
        [JsonProperty("gender")]
        public string Gender { get; set; }
        [JsonProperty("birth_date")]
        public string Birth_Date { get; set; }
        [JsonProperty("identification_type")]
        public string Identification_Type { get; set; }
        [JsonProperty("identification_number")]
        public string Identification_Number { get; set; }
        [JsonProperty("provider")]
        public string Provider { get; set; }
        [JsonProperty("created_at")]
        public string Crated_At { get; set; }
        [JsonProperty("updated_at")]
        public string Updated_At { get; set; }
        [JsonProperty("banned_at")]
        public string Banned_At { get; set; }
        [JsonProperty("deleted_at")]
        public string Deleted_At { get; set; }

        [JsonProperty("subscriptions")]
        public List<SuscriptionDto> Suscriptions = new List<SuscriptionDto>();
    }

    public class SuscriptionDto
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("site")]
        public string Site { get; set; }

        [JsonProperty("is_active")]
        public bool Is_active { get; set; }
    }
}
