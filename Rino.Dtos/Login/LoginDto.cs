using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Security;

namespace Rino.Dtos.Login
{
    public class FrontLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginDto
    {
        private SecureString _password { get; set; }
        private SecureString _phoneNumber { get; set; }

        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password
        {
            get => _password.SecureStringToString();
            set
            {
                if (value == null) return;
                _password = new SecureString();
                foreach (var x in value)
                {
                    _password.AppendChar(x);
                }
                Password = null;
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber.SecureStringToString();
            set
            {
                if (value == null) return;
                _phoneNumber = new SecureString();
                foreach (var x in value)
                {
                    _phoneNumber.AppendChar(x);
                }
                PhoneNumber = null;
            }
        }
    }


    public class SendCodeViewModel
    {

        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }


    public class VerifyCodeViewModel
    {

        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class TwoFactorModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
    }


    public class TwoFactorUserModel
    {
        [Required]
        [Display(Name = "UserName")]
        public string UserName { get; set; }
    }

    public class TwoFactorEnableModel
    {
        //[Required]
        //public int Id { get; set; }
        public bool Enable { get; set; }
    }

    public static class LoginHelper
    {
        public static string SecureStringToString(this SecureString value)
        {
            if (value == null) return null;
            var valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
