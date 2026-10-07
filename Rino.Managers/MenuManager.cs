using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        private readonly AppSettings _appSettings;
        private readonly ICacheInvalidationManager _cacheInvalidation;

        public MenuManager(IMenuRepository repository, AppSettings appSettings, ICacheInvalidationManager cacheInvalidation)
        {
            _repository = repository;
            _appSettings = appSettings;
            _cacheInvalidation = cacheInvalidation;
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

            var internalNodeIds = GetInternalNodeIds(items).Distinct().ToList();
            var nodeUrls = internalNodeIds.Count > 0
                ? await GetNodeUrlsFromSolr(internalNodeIds)
                : new Dictionary<int, string>();

            ApplyInternalMenuUrls(items, nodeUrls);
        }

        private static IEnumerable<int> GetInternalNodeIds(List<ItemMenu> items)
        {
            foreach (var it in items)
            {
                if (it == null || it.Menu == null) continue;

                if (it.Menu is InternalItem internalItem && internalItem.NodeId > 0)
                    yield return internalItem.NodeId;

                if (it.Childs == null) continue;

                foreach (var nodeId in GetInternalNodeIds(it.Childs))
                    yield return nodeId;
            }
        }

        private async Task<Dictionary<int, string>> GetNodeUrlsFromSolr(ICollection<int> nodeIds)
        {
            var query = $"q=Id:({string.Join(" OR ", nodeIds)}) AND IsDeleted:false&fl=Id,Description&rows={nodeIds.Count}";
            var result = await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode(query), _appSettings.Solr);
            var docs = (result as JObject)?["response"]?["docs"] as JArray;

            if (docs == null)
                return new Dictionary<int, string>();

            return docs
                .OfType<JObject>()
                .Select(x => new
                {
                    Id = x.Value<int?>("Id"),
                    Description = x.Value<string>("Description")
                })
                .Where(x => x.Id.HasValue && !string.IsNullOrWhiteSpace(x.Description))
                .ToDictionary(x => x.Id.Value, x => x.Description);
        }

        private static void ApplyInternalMenuUrls(List<ItemMenu> items, Dictionary<int, string> nodeUrls)
        {
            foreach (var it in items)
            {
                if (it == null || it.Menu == null) continue;

                if (it.Menu is InternalItem internalItem)
                {
                    if (nodeUrls.TryGetValue(internalItem.NodeId, out var description))
                    {
                        it.Menu.Url = "/" + description.Trim('/');
                    }
                    else if (!string.IsNullOrWhiteSpace(it.Menu.Url))
                    {
                        it.Menu.Url = "/" + it.Menu.Url.Trim('/');
                    }
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

                if (it.Childs != null)
                    ApplyInternalMenuUrls(it.Childs, nodeUrls);
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
                await ReindexAndInvalidateMenu(dto.Type, "menu:update");

                return dto;
            }

            var result = await _repository.Add(entity);
            var json = JsonConvert.SerializeObject(result);

            await ReindexAndInvalidateMenu(dto.Type, "menu:add");

            return JsonConvert.DeserializeObject<MenuJson>(json);

        }

        public async Task Update(MenuJson dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var node = await _repository.GetById(dto.Id);
            if (node == null)
                throw new EntityException("menu");

            node.MenuType = dto.Type;
            node.Structure = JsonConvert.SerializeObject(dto.Items);

            await _repository.Update(node);

            await ReindexAndInvalidateMenu(dto.Type, "menu:update");
        }
        public async Task Delete(MenuJson dto)
        {
            var node = await _repository.GetById(dto.Id);

            await _repository.Update(node);

            await ReindexAndInvalidateMenu(node?.MenuType ?? dto.Type, "menu:delete");
        }

        private async Task ReindexAndInvalidateMenu(int menuType, string reason)
        {
            await SolrHelper.DataImportAndWait(SolrCore.MENU, _appSettings.Solr, true);

            var paths = menuType > 0
                ? new[] { $"/api/site/navigation/{menuType}" }
                : new[] { "/api/site/navigation/1", "/api/site/navigation/2" };

            _cacheInvalidation.InvalidatePaths(paths, reason);
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
