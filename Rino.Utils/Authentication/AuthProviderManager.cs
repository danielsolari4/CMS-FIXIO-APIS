namespace Rino.Utils.Authentication
{
    public class AuthProviderManager
    {
        public static AuthProvider Microsoft => new AuthProvider(AuthProviderType.Microsoft);

        public static AuthProvider Twitter => new AuthProvider(AuthProviderType.Twitter);

        public static AuthProvider Facebook => new AuthProvider(AuthProviderType.Facebook);

        public static AuthProvider Google => new AuthProvider(AuthProviderType.Google);
    }
}
