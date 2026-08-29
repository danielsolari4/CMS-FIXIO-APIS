using Ray.Utils.Configuration;

namespace Ray.Utils.Authentication
{
    public class AuthProvider
    {
        public string Key { get; private set; }
        public string Secret { get; private set; }
        public bool IsEnabled { get; private set; }
        public AuthProviderType Type { get; private set; }

        public AuthProvider(AuthProviderType type)
        {
            this.Type = type;
            this.IsEnabled = IsProviderEnabled();

            if (this.IsEnabled)
            {
                this.Key = GetProviderKey();
                this.Secret = GetProviderSecret();

                if (string.IsNullOrWhiteSpace(this.Key) || string.IsNullOrWhiteSpace(this.Secret))
                    this.IsEnabled = false;
            }
        }

        private bool IsProviderEnabled()
        {
            switch (this.Type)
            {
                //case AuthProviderType.Microsoft:
                //    return ConfigurationHelper.GetValue<bool>("Microsoft.Auth.Enabled", () => false);
                //case AuthProviderType.Twitter:
                //    return ConfigurationHelper.GetValue<bool>("Twitter.Auth.Enabled", () => false);
                //case AuthProviderType.Facebook:
                //    return ConfigurationHelper.GetValue<bool>("Facebook.Auth.Enabled", () => false);
                //case AuthProviderType.Google:
                //    return ConfigurationHelper.GetValue<bool>("Google.Auth.Enabled", () => false);
                default:
                    return false;
            }
        }

        private string GetProviderKey()
        {
            switch (this.Type)
            {
                //case AuthProviderType.Microsoft:
                //    return ConfigurationHelper.GetValue<string>("Microsoft.Auth.ClientId", () => string.Empty);
                //case AuthProviderType.Twitter:
                //    return ConfigurationHelper.GetValue<string>("Twitter.Auth.ConsumerKey", () => string.Empty);
                //case AuthProviderType.Facebook:
                //    return ConfigurationHelper.GetValue<string>("Facebook.Auth.AppId", () => string.Empty);
                //case AuthProviderType.Google:
                //    return ConfigurationHelper.GetValue<string>("Google.Auth.ClientId", () => string.Empty);
            }

            return string.Empty;
        }

        private string GetProviderSecret()
        {
            switch (this.Type)
            {
                //case AuthProviderType.Microsoft:
                //    return ConfigurationHelper.GetValue<string>("Microsoft.Auth.ClientSecret", () => string.Empty);
                //case AuthProviderType.Twitter:
                //    return ConfigurationHelper.GetValue<string>("Twitter.Auth.ConsumerSecret", () => string.Empty);
                //case AuthProviderType.Facebook:
                //    return ConfigurationHelper.GetValue<string>("Facebook.Auth.AppSecret", () => string.Empty);
                //case AuthProviderType.Google:
                //    return ConfigurationHelper.GetValue<string>("Google.Auth.ClientSecret", () => string.Empty);
            }

            return string.Empty;
        }
    }

    public enum AuthProviderType
    {
        Microsoft,
        Twitter,
        Facebook,
        Google
    }
}
