using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Managers.MapperProfiles;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface IURLRedirectManager : IManager<URLRedirectDto>
    {
        //Task<ICollection<URLRedirectDto>> GetAll(int? skip, int? take);
        //Task<URLRedirectDto> GetById(int id);
    }

    public class URLRedirectManager : BaseManager, IURLRedirectManager
    {
        private readonly IURLRedirectRepository _repository;
        private readonly AppSettings _appSettings;

        public URLRedirectManager(IURLRedirectRepository repository, AppSettings appSettings)
        {
            _repository = repository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<URLRedirectProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<URLRedirectDto> Add(URLRedirectDto entity)
        {
            //Validate own entity (URLRedirect)
            if (_repository.Get(x => x.From == entity.From && x.IsDeleted == false).Result.Any())
                throw new Exception("ULEX_001");
            var ent = _Mapper.Map<URLRedirect>(entity);
            ent.CacheSolr = false;

            await _repository.Add(ent);

            await SolrHelper.DataImport(SolrCore.URLREDIRECT, _appSettings.Solr);
            
            return new URLRedirectDto
            {
                Id = ent.Id,
                From = ent.From,
                To = ent.To
            };
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }

        public async Task Delete(URLRedirectDto entity)
        {
            var urlToDeleted = await _repository.GetById(entity.Id);
            urlToDeleted.IsDeleted = true;
            urlToDeleted.CacheSolr = false;
            await _repository.Update(urlToDeleted);

            await SolrHelper.DataImport(SolrCore.URLREDIRECT, _appSettings.Solr);
            
        }

        public async Task<ICollection<URLRedirectDto>> GetAll()
        {
            try
            {
                var urlRedirects = new List<URLRedirectDto>();

                var all = await _repository.GetAll();

                foreach (var item in all)
                {
                    urlRedirects.Add(_Mapper.Map<URLRedirectDto>(item));

                }
                return urlRedirects;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<ICollection<URLRedirectDto>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public async Task<URLRedirectDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var urlRedirect = await _repository.GetById(id);

            if (urlRedirect != null)
                return _Mapper.Map<URLRedirectDto>(urlRedirect);
            return null;
        }

        public async Task Update(URLRedirectDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var redirect = (await _repository.Get(n => n.Id == dto.Id && !n.IsDeleted)).SingleOrDefault();

            if (redirect == null)
                throw new ItemNotFoundException("redirect");

            redirect.From = dto.From;
            redirect.To = dto.To;
            redirect.CacheSolr = false;

            await _repository.Update(redirect);
            await SolrHelper.DataImport(SolrCore.URLREDIRECT, _appSettings.Solr);
            
        }

        private async void UpdateSiteRedirects()
        {
            using (var client = new WebClient() { Encoding = Encoding.UTF8 })
            {
                string address = string.Format("{0}/Home/UpdateRedirects", _appSettings.Content.FrontendUrl);

                try
                {
                    await Task.FromResult(JsonConvert.DeserializeObject<dynamic>(client.DownloadString(new Uri(address))));
                }
                catch (WebException)
                { }
                catch (System.Exception)
                { }
            }
        }
    }
}
