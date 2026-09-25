using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers.Core;
using Rino.Managers.MapperProfiles;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IFrontEndUserManager : IManager<UserDto>
    {
        Task Enable(int id);
        Task Disable(int id);
        Task SetExternalLoginIds(UserDto dto);
        Task<UserDto> GetByExternalId(string facebookId, string twitterId);
        Task SetProfileImagePath(UserDto dto);
        Task AddNode(UpdateUserPreferencesDtoBindingModel dto);
        Task RemoveNode(UpdateUserPreferencesDtoBindingModel dto);
        Task RemoveAllNodes(int id);
        Task<ICollection<NodeDto>> GetAllNodes(int id);
        Task<ICollection<AssetDto>> GetAllBookmarks(int id);
        Task AddBookmark(UpdateUserBookmarksDtoBindingModel dto);
        Task RemoveBookmark(UpdateUserBookmarksDtoBindingModel dto);
        Task RemoveAllBookmarks(int id);
    }

    public class FrontEndUserManager : BaseManager, IFrontEndUserManager
    {
        private readonly IUserRepository _repository;
        private readonly INodeRepository _nodeRepository;
        private readonly IAssetRepository _assetRepository;
        private readonly AppSettings _appSettings;

        public FrontEndUserManager(IUserRepository repository, INodeRepository nodeRepository, IAssetRepository assetRepository, AppSettings appSettings)
        {
            _repository = repository;
            _nodeRepository = nodeRepository;
            _assetRepository = assetRepository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<UserProfile>();
                cfg.AddProfile<NodeProfile>();
                cfg.AddProfile<NewsProfile>();
                cfg.AddProfile<PageProfile>();
                cfg.AddProfile<NewsSourceProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<UserDto>> GetAll(int? skip = null, int? take = null)
        {
            var users = new List<UserDto>();

            var userSet = await Task.FromResult(_repository.GetAll().Where(u => !u.IsDeleted));

            if (skip.HasValue)
                userSet = userSet.OrderBy(u => u.Id).Skip(skip.Value);

            if (take.HasValue)
                userSet = userSet.Take(take.Value);

            foreach (var user in userSet)
                users.Add(MapToDto(user));

            return users;
        }

        public async Task<UserDto> GetById(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> GetByExternalId(string facebookId, string twitterId)
        {
            if (string.IsNullOrWhiteSpace(facebookId) && string.IsNullOrWhiteSpace(twitterId))
                throw new ArgumentNullException("external id");

            User user = null;

            if (!string.IsNullOrWhiteSpace(facebookId))
                user = await Task.FromResult(_repository.Get(u => u.FacebookId == facebookId));
            else if (!string.IsNullOrWhiteSpace(twitterId))
                user = await Task.FromResult(_repository.Get(u => u.TwitterId == twitterId));

            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> Add(UserDto dto)
        {
            await Task.FromResult(0);
            throw new NotImplementedException();
        }

        public async Task Update(UserDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var user = await InternalGetById(dto.Id);

            if (user == null)
                throw new EntityException("user");

            _repository.Update(MapFromDto(dto, user));
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task SetExternalLoginIds(UserDto dto)
        {
            if (dto == null || dto.Id <= 0)
                throw new EntityException("dto");

            var user = await InternalGetById(dto.Id);

            if (user == null)
                throw new EntityException("user");

            if (dto.TwitterId != null)
                user.TwitterId = dto.TwitterId;

            if (dto.FacebookId != null)
                user.FacebookId = dto.FacebookId;

            user.CacheSolr = false;
            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task SetProfileImagePath(UserDto dto)
        {
            if (dto == null || dto.Id <= 0)
                throw new EntityException("dto");

            var user = await InternalGetById(dto.Id);

            if (user == null)
                throw new EntityException("user");

            user.ProfileImagePath = dto.ProfileImagePath;
            user.CacheSolr = false;

            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task Enable(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(id);

            if (user == null || user.IsDeleted)
                throw new EntityException("user");

            user.IsEnabled = true;
            user.CacheSolr = false;

            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task Disable(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(id);

            if (user == null || user.IsDeleted)
                throw new EntityException("user");

            user.IsEnabled = false;
            user.CacheSolr = false;

            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task Delete(UserDto dto)
        {
            var user = await InternalGetById(dto.Id);

            if (user == null)
                throw new EntityException("user");

            user.IsEnabled = false;
            user.IsDeleted = true;
            user.CacheSolr = false;

            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task AddNode(UpdateUserPreferencesDtoBindingModel dto)
        {
            if (dto.Id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(dto.Id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            if (dto.NodeId <= 0)
                throw new ArgumentNullException("nodeId");

            if (user.UserNodes.Any(n => n.Node.Id == dto.NodeId))
                return;

            var nodeToAdd = await _nodeRepository.GetById(dto.NodeId);

            if (nodeToAdd == null || nodeToAdd.IsDeleted)
                throw new EntityException("node");

            user.UserNodes.Add(new UserNode { NodeId = nodeToAdd.Id });
            user.CacheSolr = false;

            _repository.Update(user);


            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task RemoveNode(UpdateUserPreferencesDtoBindingModel dto)
        {
            if (dto.Id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(dto.Id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            if (dto.NodeId <= 0)
                throw new ArgumentNullException("nodeId");

            var nodeToRemove = user.UserNodes.FirstOrDefault(n => n.Node.Id == dto.NodeId);

            if (nodeToRemove == null)
                throw new EntityException("node");

            user.UserNodes.Remove(nodeToRemove);

            user.CacheSolr = false;
            _repository.Update(user);

            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task RemoveAllNodes(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            user.UserNodes.Clear();
            user.CacheSolr = false;
            _repository.Update(user);

            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task<ICollection<NodeDto>> GetAllNodes(int userId)
        {
            if (userId <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(userId);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            var nodes = new List<NodeDto>();

            foreach (var userNode in user.UserNodes)
                nodes.Add(_Mapper.Map<NodeDto>(userNode));

            return nodes;
        }

        public async Task AddBookmark(UpdateUserBookmarksDtoBindingModel dto)
        {
            if (dto.Id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(dto.Id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            if (dto.AssetId <= 0)
                throw new ArgumentNullException("assetId");

            if (user.UserAssets.Any(n => n.AssetId == dto.AssetId))
                return;

            var assetToAdd = await _assetRepository.GetById(dto.AssetId);

            if (assetToAdd == null || assetToAdd.IsDeleted)
                throw new EntityException("asset");

            user.UserAssets.Add(new UserAsset(){AssetId = assetToAdd.Id});

            user.CacheSolr = false;
            _repository.Update(user);

            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task RemoveBookmark(UpdateUserBookmarksDtoBindingModel dto)
        {
            if (dto.Id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(dto.Id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            if (dto.AssetId <= 0)
                throw new ArgumentNullException("assetId");

            var assetToRemove = user.UserAssets.FirstOrDefault(n => n.AssetId == dto.AssetId);

            if (assetToRemove == null)
                throw new EntityException("asset");

            user.UserAssets.Remove(assetToRemove);

            user.CacheSolr = false;
            _repository.Update(user);

            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task RemoveAllBookmarks(int id)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(id);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            user.UserAssets.Clear();

            user.CacheSolr = false;
            _repository.Update(user);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }

        public async Task<ICollection<AssetDto>> GetAllBookmarks(int userId)
        {
            if (userId <= 0)
                throw new ArgumentNullException("id");

            var user = await InternalGetById(userId);

            if (user == null || !user.IsEnabled)
                throw new EntityException("user");

            var news = new List<AssetDto>();

            foreach (var userAsset in user.UserAssets)
                if (userAsset is News)
                    news.Add(_Mapper.Map<NewsDto>(userAsset));
                else if (userAsset is Page)
                    news.Add(_Mapper.Map<PageDto>(userAsset));

            return news;
        }

        public async Task<int> Count()
        {
            return await Task.FromResult(_repository.Count(u => !u.IsDeleted));
        }

        private async Task<User> InternalGetById(int id)
        {
            return await Task.FromResult(_repository.Get(u => u.Id == id && !u.IsDeleted));
        }

        private UserDto MapToDto(User user)
        {
            var dto = _Mapper.Map<UserDto>(user);

            if (user.UserRoles.Any())
                dto.Roles.AddRange(user.UserRoles.Select(r => r.Role.Name));

            return dto;
        }

        private User MapFromDto(UserDto userDto, User user)
        {
            _Mapper.Map<UserDto, User>(userDto, user);
            user.CacheSolr = false;
            return user;
        }
    }
}
