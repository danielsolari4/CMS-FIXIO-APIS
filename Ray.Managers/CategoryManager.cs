using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers.Core;
using Ray.Managers.MapperProfiles;
using Ray.Model.NewContext.Entities;
using Ray.Repositories;
using Ray.Repositories.Core;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.Managers
{
    public interface ICategoryManager : IManager<CategoryDto>
    {
        Task<ICollection<CategoryDto>> GetAll(int? skip, int? take, bool includeMedia = false, bool includeNodes = false);
        Task<CategoryDto> GetById(int id, bool includeMedia = false, bool includeNodes = false);
        Task<ICollection<CategoryDto>> GetTree(bool includeNodes = true);
    }
    public class CategoryManager : BaseManager, ICategoryManager
    {
        private readonly ICategoryRepository _repository;
        private readonly INodeRepository _nodeRepository;
        private readonly AppSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryManager(ICategoryRepository repository, INodeRepository nodeRepository, AppSettings appSettings, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _nodeRepository = nodeRepository;
            _appSettings = appSettings;
            _unitOfWork = unitOfWork;
        }
        public async Task<ICollection<CategoryDto>> GetAll(int? skip, int? take, bool includeMedia = false, bool includeNodes = false)
        {
            var categories = new List<CategoryDto>();
            var categorySet = await _repository.GetAll();
            if (skip.HasValue)
                categorySet = categorySet.OrderBy(c => c.Id).Skip(skip.Value);
            if (take.HasValue)
                categorySet = categorySet.Take(take.Value);
            foreach (var category in categorySet)
                categories.Add(MapToDto(category, includeMedia, includeNodes));
            return categories;
        }
        public async Task<CategoryDto> GetById(int id, bool includeMedia = false, bool includeNodes = false)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");
            var category = await _repository.GetById(id);
            return category != null ? MapToDto(category, includeMedia, includeNodes) : null;
        }
        public async Task<ICollection<CategoryDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(skip, take, false);
        }
        public async Task<CategoryDto> GetById(int id)
        {
            return await GetById(id, false);
        }
        public async Task<CategoryDto> Add(CategoryDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");
            if ((await _repository.Get(c => c.Name.Equals(dto.Name))).Any())
                throw new AlreadyExistsException("categoría"); //We do not allow two Category with the same name.
            var category = await _repository.Add(await MapFromDto(dto));
            await SolrHelper.DataImport(SolrCore.CATEGORY, _appSettings.Solr);
            return MapToDto(category);
        }
        public async Task Update(CategoryDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");
            var category = await _repository.GetById(dto.Id);
            if (category == null)
                throw new EntityException("category");
            if ((await _repository.Get(c => c.Name.Equals(dto.Name) && c.Id != dto.Id)).Any())
                throw new AlreadyExistsException("category");
            await _repository.Update(await MapFromDto(dto, category));
            await SolrHelper.DataImport(SolrCore.CATEGORY, _appSettings.Solr);
        }
        public async Task Delete(CategoryDto dto)
        {
            var category = await _repository.GetById(dto.Id);
            if (category == null)
                throw new EntityException("category");
            await _repository.Delete(category);
            await SolrHelper.DeleteDocumentById(SolrCore.CATEGORY, category.Id, _appSettings.Solr);
        }
        public async Task<int> Count()
        {
            return await _repository.Count();
        }
        public async Task<ICollection<CategoryDto>> GetTree(bool includeNodes = true)
        {
            var categories = new List<CategoryDto>();

            var lstTreeData = _unitOfWork.Context.GetTreeCategoryStructure().ToList();
            var parents = lstTreeData.Where(x => x.ParentCategoryId == null);
            foreach (var parent in parents)
            {
                var categoryParent = new CategoryDto
                {
                    Id = parent.Id.GetValueOrDefault(0),
                    //CreationDate = parent.CreationDate.GetValueOrDefault(DateTime.Now),
                    //CreationUser = parent.CreationUser,
                    Name = parent.Name,
                    IsEnabled = parent.IsEnabled.GetValueOrDefault(false)
                    //LastModificationDate = parent.LastModificationDate.GetValueOrDefault(DateTime.Now),
                    //LastModificationUser = parent.LastModificationUser
                };
                categories.Add(categoryParent);
                if (lstTreeData.Any(x => x.ParentCategoryId == categoryParent.Id))
                    MapChildsContentTree(categoryParent, lstTreeData);
            }

            //using (var ctx = new UnitOfWork())
            //{
            //    var test = ctx.Context.GetTreeCategoryStructure().ToList();
            //}
            //var categories = new List<CategoryDto>();
            //var parents = await _repository.Get(n => n.Parent == null && n.IsEnabled);
            //var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile<TreeCategoryProfile>();
            //    cfg.AddProfile<NodeProfile>();
            //});
            //_Mapper = mapperConfig.CreateMapper();
            //foreach (var parent in parents)
            //{
            //    var parentDto = MapToDto(parent, false, true, false);
            //    if (parentDto.Childs != null)
            //    {
            //        parentDto.Childs = parentDto.Childs.OrderBy(c => c.Name).ToList();
            //    }
            //    categories.Add(parentDto);
            //    if (includeNodes)
            //        MapChildsNodes(parentDto, parent);
            //}
            return categories.OrderBy(c => c.Name).ToArray();
        }
        private void MapChildsContentTree(CategoryDto dtoCategory, List<GetTreeCategoryStructure_Result> categories)
        {
            var childsOfNode = categories.Where(x => x.ParentCategoryId == dtoCategory.Id);
            foreach (var child in childsOfNode)
            {
                var categoryChild = new CategoryDto
                {
                    Id = child.Id.GetValueOrDefault(0),
                    //CreationDate = child.CreationDate.GetValueOrDefault(DateTime.Now),
                    //CreationUser = child.CreationUser,
                    Name = child.Name,
                    IsEnabled = child.IsEnabled.GetValueOrDefault(false),
                    //LastModificationDate = child.LastModificationDate.GetValueOrDefault(DateTime.Now),
                    //LastModificationUser = child.LastModificationUser
                };
                dtoCategory.Childs.Add(categoryChild);
                if (categories.Any(x => x.ParentCategoryId == categoryChild.Id))
                    MapChildsContentTree(categoryChild, categories);
            }
        }
        private CategoryDto MapToDto(Category category, bool includeMedia = false, bool includeNodes = false, bool defaultMapper = true)
        {
            if (defaultMapper)
                InitCategoryMapperProfiles();
            var dto = _Mapper.Map<CategoryDto>(category);
            if (includeMedia)
            {
                foreach (var it in category.MediaCategories)
                {
                    var mediaDto = _Mapper.Map<MediaDto>(it.Media);
                    if (!string.IsNullOrWhiteSpace(it.Media.SizesPaths))
                    {
                        var sizesPaths = JsonConvert.DeserializeObject<dynamic>(it.Media.SizesPaths);
                        if (sizesPaths != null)
                        {
                            mediaDto.Size1Path = _appSettings.Content.AdminDomain + sizesPaths.Size1Path;
                            mediaDto.Size2Path = _appSettings.Content.AdminDomain + sizesPaths.Size2Path;
                            mediaDto.Size3Path = _appSettings.Content.AdminDomain + sizesPaths.Size3Path;
                            mediaDto.Size4Path = _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                            mediaDto.Size5Path = _appSettings.Content.AdminDomain + sizesPaths.Size4Path;
                        }
                    }
                    dto.Media.Add(mediaDto);
                }
            }
            if (includeNodes)
            {
                foreach (var categoryNode in category.CategoryNodes)
                {
                    var categoryNodeDto = new CategoryNodeDto
                    {
                        Featured = categoryNode.Featured,
                        Node = _Mapper.Map<NodeDto>(categoryNode.Node)
                    };
                    dto.CategoryNode.Add(categoryNodeDto);
                }
            }
            return dto;
        }
        private async Task<Category> MapFromDto(CategoryDto categoryDto, Category category = null)
        {
            InitCategoryMapperProfiles();
            Category ret;
            if (category == null)
            {
                var newCategory = _Mapper.Map<Category>(categoryDto);
                if (categoryDto.CategoryNode != null && categoryDto.CategoryNode.Any())
                {
                    foreach (var categoryNodeDto in categoryDto.CategoryNode)
                    {
                        var node = await _nodeRepository.GetById(categoryNodeDto.Node.Id);
                        if (node == null)
                            throw new EntityException("node");
                        var categoryNode = new CategoryNode
                        {
                            Node = node,
                            Featured = categoryNodeDto.Featured
                        };
                        newCategory.CategoryNodes.Add(categoryNode);
                    }
                }
                if (categoryDto.ParentCategory != null)
                {
                    var parent = await _repository.GetById(categoryDto.ParentCategory.Id);
                    if (parent != null)
                        newCategory.ParentCategoryId = parent.Id;
                }
                ret = newCategory;
            }
            else
            {
                _Mapper.Map<CategoryDto, Category>(categoryDto, category);
                await UpdateNodes(categoryDto, category);
                if (categoryDto.ParentCategory == null)
                {
                    category.Parent = null;
                }
                else if (categoryDto.ParentCategory.Id != category.ParentCategoryId)
                {
                    var parent = await _repository.GetById(categoryDto.ParentCategory.Id);
                    if (parent != null)
                        category.ParentCategoryId = parent.Id;
                }
                ret = category;
            }
            ret.CacheSolr = false;
            return ret;
        }
        private async Task UpdateNodes(CategoryDto categoryDto, Category category)
        {
            var nodesToRemove = category.CategoryNodes.Where(cn => !categoryDto.CategoryNode.Any(cnDto => cnDto.Node.Id == cn.NodeId)).ToArray();
            if (nodesToRemove != null && nodesToRemove.Any())
                foreach (var cn in nodesToRemove)
                    category.CategoryNodes.Remove(cn);
            var nodesToAdd = categoryDto.CategoryNode.Where(cnDto => !category.CategoryNodes.Any(cn => cn.NodeId == cnDto.Node.Id));
            if (nodesToAdd != null && nodesToAdd.Any())
                foreach (var categoryNodeDto in nodesToAdd)
                {
                    var node = await _nodeRepository.GetById(categoryNodeDto.Node.Id);
                    if (node == null)
                        throw new EntityException("node");
                    var categoryNode = new CategoryNode
                    {
                        Node = node,
                        Featured = categoryNodeDto.Featured
                    };
                    category.CategoryNodes.Add(categoryNode);
                }
        }
        private async void MapChildsNodes(CategoryDto dtoCategory, Category parent)
        {
            InitCategoryMapperProfiles();
            foreach (var childDto in dtoCategory.Childs)
            {
                var child = parent.Childs.SingleOrDefault(c => c.Id == childDto.Id);
                if (childDto.Childs.Any())
                    MapChildsNodes(childDto, child);
                if (child != null)
                {
                    foreach (var n in child.CategoryNodes)
                    {
                        var node = await _nodeRepository.GetById(n.Node.Id);
                        if (node == null)
                            throw new EntityException("node");
                        var categoryNodeDto = new CategoryNodeDto
                        {
                            Node = _Mapper.Map<NodeDto>(node),
                            Featured = n.Featured
                        };
                        childDto.CategoryNode.Add(categoryNodeDto);
                    }
                }
            }
        }
        private void InitCategoryMapperProfiles()
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CategoryProfile>();
                cfg.AddProfile<CreateMediaProfile>();
                cfg.AddProfile<KeywordProfile>();
                cfg.AddProfile<NodeProfile>();
            });
            _Mapper = mapperConfig.CreateMapper();
        }
    }
}