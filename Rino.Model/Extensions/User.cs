using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;


namespace Rino.Model.NewContext.Entities
{
    public partial class User : IAuditableEntity
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            //TODO: ver si funciona
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = new ClaimsIdentity(await manager.GetClaimsAsync(this), authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class UserDiscriminator
    {
        public static string Frontend = "FE";
        public static string Backend = "BE";
    }
}
