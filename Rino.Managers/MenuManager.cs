using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.JsonEntities;
using Rino.Managers.Core;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IMenuManager : IManager<MenuJson>
    {
        ICollection<ItemType> GetTypes();
        Task<MenuJson> GetByTypeId(int typeId);
    }

    public class MenuManager : BaseManager, IMenuManager
    {
        private readonly IMenuRepository _repository;
        private readonly INodeRepository _nodeRepository;
        private readonly AppSettings _appSettings;

        public MenuManager(IMenuRepository repository, INodeRepository nodeRepository, AppSettings appSettings)
        {
            _repository = repository;
            _nodeRepository = nodeRepository;
            _appSettings = appSettings;
        }

        public ICollection<ItemType> GetTypes()
        {
            var list = Enum.GetValues(typeof(MenuType))
                .Cast<MenuType>()
                .Select(v => new ItemType
                {
                    Name = v.ToString(),
                    Id = (int)v
                })
                .ToList();

            return list;
        }

        public async Task<MenuJson> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("Id");
            var entity = await _repository.GetById(id);
            return new MenuJson();
        }

        public async Task<MenuJson> GetByTypeId2(int typeId)
        {
            if (typeId <= 0)
                throw new ArgumentNullException("MenuType");

            var all = await _repository.GetAll();
            var entity = all.FirstOrDefault(x => x.MenuType == typeId);
            if (entity == null) return null;
            var menu = new MenuJson
            {
                Id = entity.Id,
                Type = entity.MenuType,
                Items = JsonConvert.DeserializeObject<List<ItemMenu>>(entity.Structure)
            };


            await EachMenu(menu.Items);

            return menu;
        }

        public async Task<MenuJson> GetByTypeId(int typeId)
        {
            if (typeId <= 0)
                throw new ArgumentNullException("MenuType");

            var result = await SolrHelper.ExecuteQuery(Utils.Solr.SolrCore.MENU, HttpUtility.UrlDecode("q=MenuType:" + typeId), _appSettings.Solr);
            var stringJson = JsonConvert.SerializeObject(result);

            SolrResponse settings = JsonConvert.DeserializeObject<SolrResponse>(stringJson);

            if (settings?.response?.docs == null || settings?.response?.docs.Count == 0)
                return null;

            var structure = settings.response.docs[0].Structure?.ToString();

            if (string.IsNullOrWhiteSpace(structure))
                return null;

            var items = JsonConvert.DeserializeObject<System.Collections.Generic.List<ItemMenu>>(structure);
            var val = new MenuJson
            {
                Id = settings.response.docs[0].Id,
                Type = settings.response.docs[0].MenuType,
                Items = items
            };

            if (val == null)
                return null;


            await EachMenu(val.Items);

            return val;
        }

        private async Task EachMenu(List<ItemMenu> items)
        {
            if (items == null) return;
            foreach (var it in items)
            {
                if (it == null || it.Menu == null) continue;

                if (it.Menu.MenuType == (int)MenuItemType.Internal)
                {
                    var nde = await _nodeRepository.GetById(((InternalItem)it.Menu).NodeId);
                    it.Menu.Url = $"/{nde?.Description}";
                }
                else switch (it.Menu.MenuType)
                {
                    case (int)MenuItemType.External:
                    {
                        var a = it.Menu as ExternalLink;
                        break;
                    }
                    case (int)MenuItemType.Asset:
                    {
                        var a = it.Menu as AssetItem;
                        break;
                    }
                }

                await EachMenu(it.Childs);
            }
        }

        public async Task<MenuJson> Add(MenuJson dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var entity = new Menu
            {
                MenuType = dto.Type,
                Structure = JsonConvert.SerializeObject(dto.Items)
            };

            var exists = _repository.GetAll().Result.FirstOrDefault(x => x.MenuType == dto.Type);

            if (exists != null)
            {
                exists.Structure = entity.Structure;
                await _repository.Update(exists);

                dto.Id = exists.Id;
                await SolrHelper.DataImport(SolrCore.MENU, _appSettings.Solr, true);

                return dto;
            }

            var result = await _repository.Add(entity);
            var json = JsonConvert.SerializeObject(result);

            await SolrHelper.DataImport(SolrCore.MENU, _appSettings.Solr, true);

            return JsonConvert.DeserializeObject<MenuJson>(json);

        }

        public async Task Update(MenuJson dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var node = await _repository.GetById(dto.Id);

            await SolrHelper.DataImport(SolrCore.MENU, _appSettings.Solr, true);
        }
        public async Task Delete(MenuJson dto)
        {
            var node = await _repository.GetById(dto.Id);

            await _repository.Update(node);

            await SolrHelper.DataImport(SolrCore.MENU, _appSettings.Solr, true);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        public Task<ICollection<MenuJson>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }
    }
}