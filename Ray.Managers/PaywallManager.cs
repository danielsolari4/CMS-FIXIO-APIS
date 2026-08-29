using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Managers.Core;

namespace Ray.Managers
{
    public interface IPaywallManager : IManager<PaywallUserDto>
    {
        Task<PaywallUserDto> Login(PaywallLoginDto model);
    }

    public class PaywallManager : IPaywallManager
    {
        public async Task<PaywallUserDto> Login(PaywallLoginDto model)
        {
            string ResponseString = string.Empty;
            HttpWebResponse response = null;
            try
            {
                //TODO: Ver esto comentado
                var request = (HttpWebRequest) WebRequest.Create("");//string.Format(ConfigurationManager.AppSettings["Paywall.Api.Url"], ConfigurationManager.AppSettings["Paywall.Api.Url.Login"]));
                request.Accept = "application/json";
                request.Method = "POST";
                //request.Headers.Add("Authorization", $"Bearer {ConfigurationManager.AppSettings["Paywall.Api.Auth.Bearer"]}");

                var data = Encoding.ASCII.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(model));

                request.ContentType = "application/json";
                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                response = (HttpWebResponse)request.GetResponse();
                ResponseString = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
                var dresult = JsonConvert.DeserializeObject<PaywallResponseDto<PaywallUserDto>>(ResponseString);
                return dresult.Data;
            }
            catch (WebException ex)
            {
                return null;
            }
        }

        public Task<PaywallUserDto> Add(PaywallUserDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }

        public Task Delete(PaywallUserDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<PaywallUserDto>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public Task<PaywallUserDto> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(PaywallUserDto entity)
        {
            throw new NotImplementedException();
        }
    }
}
