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
using Rino.Repositories.Core;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface INodeManager : IManager<NodeDto>
    {
        Task<ICollection<NodeDto>> GetAll(bool includeContent = false, bool includeAssets = false, int? languageId = null, int? skip = null, int? take = null);
        Task<NodeDto> GetById(int id, bool includeContent = false, bool includeAssets = false, int? languageId = null);
        ICollection<NodeDto> GetTree(int nodeId = 0, bool includeContent = true, bool onlyActive = false, int? languageId = null);
        ICollection<NodeDto> GetTreeMicrosite(int nodeId = 0, bool includeContent = true, int? languageId = null);

        Task<ICollection<NodeDto>> GetNodesWithCustomLayout(int? nodeId);
    }
    public class NodeManager : BaseManager, INodeManager
    {
        private List<Node> _listOfNodes { get; set; }

        private readonly IKeywordRepository _keywordRepository;
        private readonly AppSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INodeRepository _repository;

        public NodeManager(INodeRepository repository, IKeywordRepository keywordRepository, AppSettings appSettings, IUnitOfWork unitOfWork)
        {
            _keywordRepository = keywordRepository;
            _appSettings = appSettings;
            _unitOfWork = unitOfWork;
            _repository = repository;
            _listOfNodes = new List<Node>();
        }
        public async Task<ICollection<NodeDto>> GetAll(bool includeContent = false, bool includeAssets = false, int? languageId = null, int? skip = null, int? take = null)
        {
            var nodes = new List<NodeDto>();
            var nodeSet = await _repository.Get(n => !n.IsDeleted);
            if (skip.HasValue)
                nodeSet = nodeSet.OrderBy(n => n.Id).Skip(skip.Value);
            if (take.HasValue)
                nodeSet = nodeSet.Take(take.Value);
            foreach (var node in nodeSet)
                nodes.Add(MapToDto(node, includeContent, includeAssets, languageId));
            return nodes;
        }
        public async Task<ICollection<NodeDto>> GetAll(int? skip = null, int? take = null)
        {
            return await GetAll(false, false, null, skip, take);
        }
        public async Task<ICollection<NodeDto>> GetNodesWithCustomLayout(int? nodeId = null)
        {
            var nodes = new List<NodeDto>();
            if (!nodeId.HasValue)
            {
                var nodeSet = await _repository.Get(n => !n.IsDeleted && n.IsPublished);
                foreach (var node in nodeSet)
                    nodes.Add(MapToDto(node));
            }
            else
            {
                var node = await _repository.GetById(nodeId.Value);
                if (node != null && !node.IsDeleted && node.IsPublished)
                    nodes.Add(MapToDto(node));
            }
            return nodes;
        }
        public async Task<NodeDto> GetById(int id, bool includeContent = false, bool includeAssets = false, int? languageId = null)
        {
            if (id <= 0)
                throw new ArgumentNullException("id");
            var node = await _repository.GetById(id);
            if (!includeAssets)
                node.AssetNodes = new List<AssetNode>();
            return node != null && !node.IsDeleted ? MapToDto(node, includeContent, includeAssets, languageId) : null;
        }
        public async Task<NodeDto> GetById(int id)
        {
            return await GetById(id, false, false, null);
        }
        public async Task<NodeDto> Add(NodeDto dto)
        {
            if (dto == null)
                throw new EntityException("dto");

            var node = await MapFromDto(dto);

            UpdateKeywords(dto, node);

            var result = await _repository.Add(node);

            var addedNode = MapToDto(result);

            //Update childs of microsite
            if (addedNode.IsEnabled)
            {
                var childItems = _repository.GetAll().Result.Where(x => x.ParentNodeId == addedNode.Id);
                _listOfNodes.AddRange(childItems);
                await UpdateChildsMicrositeAsync();
            }



            await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr, true);

            return addedNode;
        }
        public async Task Update(NodeDto dto)
        {
            if (dto.NewSourceId == 0)
            {
                dto.NewSourceId = null;
            }

            if (dto == null)
                throw new EntityException("dto");

            var node = await _repository.GetById(dto.Id);

            if (node == null || node.IsDeleted)
                throw new EntityException("node");

            if (!node.Content.FirstOrDefault().Title.ToLower().Equals(dto.Content.FirstOrDefault().Title.ToLower()))
            {
                if (dto.UpdateAssets)
                {
                    foreach (var it in node.AssetNodes)
                    {
                        it.Asset.LastModificationDate = DateTime.UtcNow;
                        it.Asset.CacheSolr = false;
                    }
                }

                foreach (var layoutInstance in node.LayoutInstances)
                {
                    layoutInstance.LastModificationDate = DateTime.UtcNow;
                    layoutInstance.CacheSolr = false;
                }

                foreach (var it in node.UserNodes)
                {
                    it.User.LastModificationDate = DateTime.UtcNow;
                    it.User.CacheSolr = false;
                }
            }

            #region If update Description who contains URL in childs, update that field

            if (dto.Description != null && !node.Description.Equals(dto.Description))
                await UpdateSlugChildNodesAsync(node, node.Description, dto.Description);

            #endregion

            var nodeToUpdate = await MapFromDto(dto, node);

            UpdateKeywords(dto, node);

            await _repository.Update(nodeToUpdate);

            //Update childs of microsite
            if ((bool)nodeToUpdate.IsEnabled)
            {
                var childItems = _repository.GetAll().Result.Where(x => x.ParentNodeId == nodeToUpdate.Id);
                _listOfNodes.AddRange(childItems);
                await UpdateChildsMicrositeAsync();
            }



            await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.NEWS, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.PAGE, _appSettings.Solr);
            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr, true);
            await SolrHelper.DataImport(SolrCore.USER, _appSettings.Solr);
        }
        public async Task Delete(NodeDto dto)
        {
            var node = await _repository.GetById(dto.Id);
            if (node == null || node.IsDeleted)
                throw new EntityException("node");
            node.IsEnabled = false;
            node.IsDeleted = true;
            node.CacheSolr = false;
            await _repository.Update(node);
            await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);
        }

        public async Task<int> Count()
        {
            return await _repository.Count();
        }

        public ICollection<NodeDto> GetTree(int nodeId = 0, bool includeContent = true, bool onlyActive = false, int? languageId = null)
        {
            var nodes = new List<NodeDto>();

            
                var lstTreeData = _unitOfWork.Context.GetTreeNodeStructure().ToList();

                if (onlyActive)
                {
                    lstTreeData = lstTreeData.Where(x => x.IsPublished == true).ToList();
                }

                IEnumerable<GetTreeNodeStructure_Result> parents;

                if (nodeId > 0)
                {
                    GetTreeNodeStructure_Result dataParentNode;
                    var dataNode = lstTreeData.FirstOrDefault(x => x.Id == nodeId);

                    //For the first time, has father
                    if (dataNode != null && dataNode.ParentNodeId != null)
                    {
                        dataParentNode = lstTreeData.FirstOrDefault(x => x.Id == dataNode.ParentNodeId);

                        //Father was founded and is a isMicrosite == true
                        if (dataParentNode != null && dataParentNode.IsEnabled.GetValueOrDefault(false))
                        {
                            dataNode = lstTreeData.FirstOrDefault(x => x.Id == dataNode.ParentNodeId);

                            while (dataNode != null && dataNode.ParentNodeId != null && dataNode.IsEnabled.GetValueOrDefault(false))
                            {
                                dataParentNode = dataNode;
                                dataNode = lstTreeData.FirstOrDefault(x => x.Id == dataNode.ParentNodeId);
                            }

                            parents = lstTreeData.Where(x => x.Id == dataParentNode.Id);
                        }
                        else
                        {
                            parents = lstTreeData.Where(x => x.Id == dataNode.Id);
                        }
                    }
                    else
                    {
                        parents = lstTreeData.Where(x => x.Id == nodeId);
                    }
                }
                else
                    parents = lstTreeData.Where(x => x.ParentNodeId == null);

                foreach (var parent in parents)
                {
                    var nodeParent = new NodeDto
                    {
                        Id = parent.Id.GetValueOrDefault(0),
                        Description = parent.Description,
                        IsEnabled = parent.IsEnabled.GetValueOrDefault(false),
                        Order = parent.Order.GetValueOrDefault(0),
                        IsPublished = parent.IsPublished.GetValueOrDefault(false),
                        IsPrint = parent.IsPrint.GetValueOrDefault(false),
                        IsDiagrammable = parent.IsDiagrammable.GetValueOrDefault(false),
                        NewSourceId = parent.NewSourceId.GetValueOrDefault(0),
                        SeoTitle = parent.SeoTitle,
                        SeoDescription = parent.SeoDescription,
                        SeoImage = parent.SeoImage,
                        ParentNodeId = parent.ParentNodeId ?? 0,
                        OGDescription = parent.OGDescription,
                        OGTitle = parent.OGTitle,
                        Keywords = parent.Keywords,
                        RelatedKeywords = parent.RelatedKeywords,
                        Content = new List<NodeContentDto>
                        {
                            new NodeContentDto
                            {
                                LanguageId = parent.LanguageId.GetValueOrDefault(0),
                                Title = parent.Title
                            }
                        }
                    };

                    nodes.Add(nodeParent);

                    if (lstTreeData.Any(x => x.ParentNodeId == nodeParent.Id))
                        MapChildsContentTree(nodeParent, lstTreeData);
                }
         

            return nodes;
        }

        public ICollection<NodeDto> GetTreeMicrosite(int nodeId = 0, bool includeContent = true, int? languageId = null)
        {
            
                var lstTreeData = _unitOfWork.Context.GetTreeMicrosite().ToList();

                var tree = lstTreeData.Where(x =>
                                            !x.Sort.Contains(".") && (!x.IsDeleted ?? true)).Select(x => new NodeDto
                                            {
                                                Id = x.Id.GetValueOrDefault(0),
                                                Description = x.Node_en,
                                                IsEnabled = x.IsMicrosite.GetValueOrDefault(false),
                                                Order = x.LevelNode ?? 0,
                                                IsDiagrammable = x.IsDiagrammable.GetValueOrDefault(false),
                                                Content = new List<NodeContentDto>
                    {
                        new NodeContentDto
                        {
                            Title = x.Node_en// + " - " + x.NodeDescription
                        }
                    },
                                                Childs = x.Id != null ? DrawTree(lstTreeData, x.Id ?? 0) : null
                                            }).ToList();

                return nodeId > 0 ? tree.Where(x => x.Id == nodeId).ToList() : tree;
            
        }

        private static List<NodeDto> DrawTree(List<GetTreeMicrosite_Result> lstTreeData, int parentId)
        {
            var childs = lstTreeData.Where(x => x.ParentNodeId == parentId && (!x.IsDeleted ?? true)).Select(x => new NodeDto
            {
                Id = x.Id.GetValueOrDefault(0),
                Description = x.Node_en,
                IsEnabled = x.IsMicrosite.GetValueOrDefault(false),
                IsDiagrammable = x.IsDiagrammable.GetValueOrDefault(false),
                Order = x.LevelNode ?? 0,
                Content = new List<NodeContentDto>
                {
                    new NodeContentDto
                    {
                        Title = x.Node_en// + " - " + x.NodeDescription
                    }
                },
                Childs = x.Id != null ? DrawTree(lstTreeData, x.Id ?? 0) : null
            }).ToList();

            return childs;
        }

        private void MapChildsContentTree(NodeDto dtoNode, List<GetTreeNodeStructure_Result> nodes)
        {
            var childsOfNode = nodes.Where(x => x.ParentNodeId == dtoNode.Id);
            foreach (var child in childsOfNode)
            {
                var nodeChild = new NodeDto
                {
                    Id = child.Id.GetValueOrDefault(0),
                    Description = child.Description,
                    IsEnabled = child.IsEnabled.GetValueOrDefault(false),
                    Order = child.Order.GetValueOrDefault(0),
                    IsPublished = child.IsPublished.GetValueOrDefault(false),
                    IsDiagrammable = child.IsDiagrammable.GetValueOrDefault(false),
                    IsPrint = child.IsPrint.GetValueOrDefault(false),
                    SeoTitle = child.SeoTitle,
                    SeoDescription = child.SeoDescription,
                    RelatedKeywords = child.RelatedKeywords,
                    SeoImage = child.SeoImage,
                    ParentNodeId = child.ParentNodeId ?? 0,
                    OGDescription = child.OGDescription,
                    NewSourceId = child.NewSourceId.GetValueOrDefault(0),
                    OGTitle = child.OGTitle,
                    Keywords = child.Keywords,
                    Content = new List<NodeContentDto>
                    {
                        new NodeContentDto
                        {
                            LanguageId = child.LanguageId.GetValueOrDefault(0),
                            Title = child.Title
                        }
                    }
                };
                dtoNode.Childs.Add(nodeChild);
                if (nodes.Any(x => x.ParentNodeId == nodeChild.Id))
                    MapChildsContentTree(nodeChild, nodes);
            }
        }

        protected async void UpdateKeywords(NodeDto nodeDto, Node node)
        {
            var keywordsList = !string.IsNullOrWhiteSpace(nodeDto.RelatedKeywords) ? nodeDto.RelatedKeywords.Split(',').Select(k => k.Trim()) : new List<string>();
            //node.Keyword = new List<Keyword>();

            foreach (var sKeyword in keywordsList)
            {
                var keyword = (await _keywordRepository.Get(k => k.Name.Equals(sKeyword) && k.IsEnabled)).SingleOrDefault();

                if (keyword == null)
                    node.NodeKeywords.Add(new NodeKeyword() { Keyword = new Keyword(sKeyword) });
                else
                {
                    if (!node.NodeKeywords.Any(k => k.Keyword.Name.Equals(keyword.Name)))
                        node.NodeKeywords.Add(new NodeKeyword() { Keyword = keyword});
                }

                //node.Keywords = string.IsNullOrWhiteSpace(node.Keywords) ? sKeyword : string.Format("{0},{1}", node.Keywords, sKeyword);
            }

            foreach (var keyword in node.NodeKeywords.Where(keyword => !keywordsList.Any(sKeyword => keyword.Keyword.Name.Equals(sKeyword))).ToArray())
            {
                node.NodeKeywords.Remove(keyword);
            }
        }

        private async Task UpdateSlugChildNodesAsync(Node node, string originalString, string slugUpdated)
        {
            var childItems = _repository.GetAll().Result.Where(x => x.ParentNodeId == node.Id && !x.IsDeleted).ToArray();

            if (childItems != null && childItems.Any())
            {
                foreach (var nodeChildItem in childItems)
                {
                    if (!string.IsNullOrEmpty(nodeChildItem.Description))
                    {
                        nodeChildItem.Description = nodeChildItem.Description.Replace(originalString, slugUpdated);
                        await _repository.Update(nodeChildItem);

                        if (nodeChildItem.Childs != null && nodeChildItem.Childs.Any())
                            await UpdateSlugChildNodesAsync(nodeChildItem, originalString, slugUpdated);
                    }
                }
            }
        }

        private NodeDto MapToDtoTree(Node node,
                                     bool includeContent = false,
                                     bool includeAssets = false,
                                     int? languageId = null,
                                     bool defaultMapper = true)
        {
            if (defaultMapper)
                InitNodeMapperProfiles();
            var dto = _Mapper.Map<NodeDto>(node);
            if (includeContent)
            {
                foreach (var content in node.Content.Where(c => !languageId.HasValue || c.LanguageId == languageId.Value))
                {
                    dto.Content.Add(_Mapper.Map<NodeContentDto>(content));
                }
            }
            if (includeAssets)
            {
                foreach (var asset in node.AssetNodes)
                {
                    if (asset is News)
                        dto.Assets.Add(_Mapper.Map<NewsDto>(asset));
                    else if (asset is Page)
                        dto.Assets.Add(_Mapper.Map<PageDto>(asset));
                }
            }
            return dto;
        }

        private NodeDto MapToDto(Node node, bool includeContent = false, bool includeAssets = false, int? languageId = null, bool defaultMapper = true)
        {
            if (defaultMapper)
                InitNodeMapperProfiles();
            var dto = _Mapper.Map<NodeDto>(node);
            if (includeContent)
            {
                foreach (var content in node.Content.Where(c => !languageId.HasValue || c.LanguageId == languageId.Value))
                {
                    dto.Content.Add(_Mapper.Map<NodeContentDto>(content));
                }
            }
            if (includeAssets && node.AssetNodes != null)
            {
                foreach (var asset in node.AssetNodes)
                {
                    if (asset is News)
                        dto.Assets.Add(_Mapper.Map<NewsDto>(asset));
                    else if (asset is Page)
                        dto.Assets.Add(_Mapper.Map<PageDto>(asset));
                }
            }
            return dto;
        }

        private async Task<Node> MapFromDto(NodeDto nodeDto, Node node = null)
        {
            InitNodeMapperProfiles();
            Node ret;


            if (node == null)
            {
                var newNode = _Mapper.Map<Node>(nodeDto);
                if (nodeDto.ParentNode != null)
                {
                    var parent = await _repository.GetById(nodeDto.ParentNode.Id);
                    if (parent != null)
                        newNode.ParentNodeId = parent.Id;
                }
                ret = newNode;
            }
            else
            {
                _Mapper.Map<NodeDto, Node>(nodeDto, node);
                if (nodeDto.ParentNode == null)
                {
                    node.ParentNode = null;
                }
                else if (nodeDto.ParentNode.Id != node.ParentNodeId)
                {
                    var parent = await _repository.GetById(nodeDto.ParentNode.Id);
                    if (parent != null)
                        node.ParentNodeId = parent.Id;
                }
                ret = node;
            }
            if (nodeDto.Content != null)
            {
                foreach (var content in nodeDto.Content)
                {
                    var existingContent = ret.Content.SingleOrDefault(c => c.LanguageId == content.LanguageId);
                    if (existingContent != null)
                    {
                        _Mapper.Map<NodeContentDto, NodeContent>(content, existingContent);
                    }
                    else
                    {
                        var newContent = _Mapper.Map<NodeContent>(content);
                        newContent.LanguageId = content.LanguageId;
                        ret.Content.Add(newContent);
                    }
                }
            }

            ret.CacheSolr = false;
            return ret;
        }

        private void MapChildsContent(NodeDto dtoNode, Node parent, int? languageId = null)
        {
            InitNodeMapperProfiles();
            dtoNode.Childs = dtoNode.Childs.OrderBy(n => n.Order).ToList();
            foreach (var childDto in dtoNode.Childs)
            {
                var child = parent.Childs.SingleOrDefault(c => c.Id == childDto.Id);
                if (childDto.Childs.Any())
                    MapChildsContent(childDto, child, languageId);
                foreach (var content in child.Content.Where(c => !languageId.HasValue || c.LanguageId == languageId.Value))
                    childDto.Content.Add(_Mapper.Map<NodeContentDto>(content));
            }
        }

        private void InitNodeMapperProfiles()
        {
            var mapperConfig = new AutoMapper.MapperConfiguration(cfg =>
            {
                cfg.ShouldMapMethod = methodInfo => false;
                cfg.AddProfile<NodeProfile>();
                cfg.AddProfile<NewsProfile>();
                cfg.AddProfile<PageProfile>();
            });
            _Mapper = mapperConfig.CreateMapper();
        }

        private async Task UpdateChildsMicrositeAsync()
        {
            var nodes = _listOfNodes;
            _listOfNodes = new List<Node>();

            foreach (var item in nodes)
            {
                var childItems = _repository.GetAll().Result.Where(x => x.ParentNodeId == item.Id);

                if (childItems != null && childItems.Any())
                    _listOfNodes.AddRange(childItems);

                await _repository.Update(item);
            }

            if (_listOfNodes != null && _listOfNodes.Any())
                await UpdateChildsMicrositeAsync();
        }
    }
}
