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
    public interface IUserManager : IManager<UserDto>
    {
        Task Enable(int id);
        Task Disable(int id);
        Task SetProfileImagePath(UserDto dto);
        Task<UserDto> GetByExternalId(string facebookId, string twitterId);
    }
    
    public class UserManager : BaseManager, IUserManager
    {
        private readonly IUserRepository _repository;
        private readonly IRoleRepository _roleRepository;
        private readonly AppSettings _appSettings;

        public UserManager(IUserRepository repository, IRoleRepository roleRepository, AppSettings appSettings)
        {
            _repository = repository;
            _roleRepository = roleRepository;
            _appSettings = appSettings;

            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<UserProfile>();
            });

            _Mapper = mapperConfig.CreateMapper();
        }

        public async Task<ICollection<UserDto>> GetAll(int? skip = null, int? take = null)
        {
            var users = new List<UserDto>();

            var userSet = await Task.FromResult(_repository.GetAll().Where(u => !u.IsDeleted && u.Discriminator.Equals(UserDiscriminator.Backend)));

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

            _repository.Update(await MapFromDto(dto, user));
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

        public async Task<int> Count()
        {
            return await Task.FromResult(_repository.Count(u => !u.IsDeleted && u.Discriminator.Equals(UserDiscriminator.Backend)));
        }

        private async Task<User> InternalGetById(int id)
        {
            //return await Task.FromResult(_repository.Get(u => u.Id == id && !u.IsDeleted && u.Discriminator.Equals(UserDiscriminator.Backend)));
            return await Task.FromResult(_repository.Get(u => u.Id == id && !u.IsDeleted));
        }

        private UserDto MapToDto(User user)
        {
            var dto = _Mapper.Map<UserDto>(user);

            if (user.UserRoles.Any())
                dto.Roles.AddRange(user.UserRoles.Select(r => r.Role.Name));

            return dto;
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

        private async Task<User> MapFromDto(UserDto userDto, User user)
        {
            _Mapper.Map<UserDto, User>(userDto, user);

            var rolesToRemove = user.UserRoles.Where(role => !userDto.Roles.Any(r => r.Equals(role.Role.Name, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            foreach (var role in rolesToRemove)
                user.UserRoles.Remove(role);

            var rolesToAdd = userDto.Roles.Where(r => !user.UserRoles.Any(role => role.Role.Name.Equals(r, StringComparison.InvariantCultureIgnoreCase))).ToArray();
            foreach (var roleName in rolesToAdd)
            {
                var r = await _roleRepository.Get(ro => ro.Name.Equals(roleName));

                if (r.FirstOrDefault() == null)
                    throw new ArgumentException("roleName");

                user.UserRoles.Add(new UserRole(){Role = r.FirstOrDefault()});
            }

            user.CacheSolr = false;

            return user;
        }
    }
}
