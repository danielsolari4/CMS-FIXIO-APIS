using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.Factories.Html;
using Rino.Dtos.Mapping;
using Rino.Managers.Core;
using Rino.Managers.Helpers;
using Rino.Model.NewContext.Entities;
using Rino.Repositories;
using Rino.Repositories.Core;
using Rino.Utils.Logging;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface ILayoutInstanceManager : IManager<LayoutInstanceDto>
    {
        Task<LayoutInstanceDto> GetByNodeId(int nodeId);
        ICollection<LayoutInstanceMicrositeDto> GetLayoutsByNodeId(int id);

        Task<bool> Deactivate(int id);
        Task SyncAllLayoutInstances(int? nodeId = null);
    }

    public class LayoutInstanceManager : BaseManager, ILayoutInstanceManager
    {
        private readonly ILayoutInstanceRepository _repository;
        private readonly INodeRepository _repositoryNode;
        private readonly IAssetRepository _repositoryAsset;
        private readonly AppSettings _appSettings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheInvalidationManager _cacheInvalidation;

        //TODO: Ver como agregar esto, si con una clase de configuracion o solo el string de imageUrl
        public LayoutInstanceManager(ILayoutInstanceRepository repository, INodeRepository repositoryNode, IAssetRepository repositoryAsset, AppSettings appSettings, IUnitOfWork unitOfWork, ICacheInvalidationManager cacheInvalidation)
        {
            _repository = repository;
            _repositoryNode = repositoryNode;
            _repositoryAsset = repositoryAsset;
            _appSettings = appSettings;
            _unitOfWork = unitOfWork;
            _cacheInvalidation = cacheInvalidation;
        }

        public ICollection<LayoutInstanceMicrositeDto> GetLayoutsByNodeId(int id)
        {

            var lstTreeData = _unitOfWork.Context.GetTreeLayoutStructureById(id).ToList();
            return lstTreeData.Where(x => x.NodeId != id).Select(x => new LayoutInstanceMicrositeDto
            {
                Id = x.Id ?? 0,
                NodeId = x.NodeId ?? 0,
                NodeDescription = x.NodeDescription,
                CreationUser = x.CreationUser,
                CreationDate = x.CreationDate,
                LastModificationDate = x.LastModificationDate,
                PublicationDate = x.PublicationDate,
                Node_en = x.Node_en,
                Node_es = x.Node_es,
                IsDeleted = x.IsDeleted ?? false,
                NroRow = x.NroRow ?? 0,
                IsMicrosite = x.IsMicrosite ?? false,
                ParentNodeId = x.ParentNodeId ?? 0,
                HasParentMicrosite = x.HasParentMicrosite ?? false,
                LevelNode = x.LevelNode ?? 0
            }).ToList();

        }


        public async Task<LayoutInstanceDto> GetByNodeId(int nodeId)
        {
            LayoutInstance layoutInstance = null;

            if (nodeId <= 0)
                throw new ArgumentNullException("node id");

            var layoutSet = await _repository.Get(h => h.NodeId == nodeId && h.PublicationDate <= DateTime.UtcNow);
            if (layoutSet != null && layoutSet.Any())
            {
                var layoutInstanceByNodeId = layoutSet.OrderByDescending(p => p.PublicationDate).FirstOrDefault();

                if (layoutInstanceByNodeId != null)
                {
                    layoutInstance = layoutSet.Where(t => t.PublicationDate == layoutInstanceByNodeId.PublicationDate)
                        .OrderByDescending(g => g.CreationDate).FirstOrDefault();
                }

            }

            return layoutInstance?.Map();
        }




        public Task<ICollection<LayoutInstanceDto>> GetAll(int? skip = null, int? take = null)
        {
            throw new NotImplementedException();
        }

        public async Task<LayoutInstanceDto> GetById(int id)
        {
            LayoutInstance layoutInstance = null;

            if (id <= 0)
                throw new ArgumentNullException("node id");

            var layoutSet = await _repository.Get(h => h.Id == id);
            if (layoutSet != null && layoutSet.Any())
            {
                var layoutInstanceByNodeId = layoutSet.OrderByDescending(p => p.PublicationDate).FirstOrDefault();

                if (layoutInstanceByNodeId != null)
                {
                    layoutInstance = layoutSet.Where(t => t.PublicationDate == layoutInstanceByNodeId.PublicationDate)
                        .OrderByDescending(g => g.CreationDate).FirstOrDefault();

                    var obj = GetParseStructure(layoutInstance?.Structure);

                    await obj.ParseComponentsAsync(_appSettings.Content.ImageUrl, _appSettings.Solr);

                    if (layoutInstance != null && obj != null)
                        layoutInstance.Structure = JsonConvert.SerializeObject(obj);
                }

            }

            return layoutInstance?.Map();
        }

        public async Task<LayoutInstanceDto> Add(LayoutInstanceDto entity)
        {
            var node = _repositoryNode.Include(x => x.Childs)?.Where(x => x.Id == entity.NodeId)?.FirstOrDefault();
            if (node != null)
                entity.Description = node.Description ?? "/";
            else
                throw new Exception("Node not found");

            //var obj = GetParseStructure(entity.StructureJson);

            entity.LayoutType = entity.StructureJson.LayoutTypeId;

            var layoutInstance = entity.Map();
            layoutInstance.CacheSolr = false;
            layoutInstance.IsDeleted = false;

            if (entity.CurrentPublication)
                layoutInstance.PublicationDate = DateTime.UtcNow;

            var li = await _repository.Add(layoutInstance);

            //sync if current layoutinstance
            await SyncLayoutInstance(li, entity.CurrentPublication);

            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr);
            _cacheInvalidation.InvalidateLayout(entity.NodeId, $"layout:{entity.NodeId} add");

            entity.Id = li.Id;

            return entity;
        }

        public async Task<bool> Deactivate(int id)
        {
            var li = await _repository.Get(x => x.NodeId == id && x.IsEnabled);

            foreach (var it in li.ToList())
            {
                it.IsEnabled = false;
                it.CacheSolr = false;
                await _repository.Update(it);
            }


            //Delete Old Document
            await SolrHelper.DeleteDocumentByQuery(SolrCore.LAYOUTINSTANCEBYNODE,
                "NodeId:" + id, _appSettings.Solr);

            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr);
            _cacheInvalidation.InvalidateLayout(id, $"layout:{id} deactivate");

            return true;
        }

        public async Task Update(LayoutInstanceDto entity)
        {
            var node = _repositoryNode.Include(x => x.Childs)?.Where(x => x.Id == entity.NodeId)?.FirstOrDefault();
            if (node != null)
                entity.Description = node.Description;

            //var obj = GetParseStructure(entity.StructureJson);

            entity.LayoutType = entity.StructureJson.LayoutTypeId;

            var layoutInstance = entity.Map();
            layoutInstance.CacheSolr = false;
            layoutInstance.IsDeleted = false;

            if (entity.CurrentPublication)
                layoutInstance.PublicationDate = DateTime.UtcNow;

            var li = await _repository.Add(layoutInstance);
            entity.Id = li.Id;
            //sync if current layoutinstance
            await SyncLayoutInstance(li, entity.CurrentPublication);


            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr);
            _cacheInvalidation.InvalidateLayout(entity.NodeId, $"layout:{entity.NodeId} update");
        }

        public Task Delete(LayoutInstanceDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<int> Count()
        {
            throw new NotImplementedException();
        }


        public async Task SyncLayoutInstance(LayoutInstance entity, bool isCurrent)
        {
            if (entity != null)
            {
                var node = await _repositoryNode.GetById(entity.NodeId);
                if (node == null) return;

                if (isCurrent)
                {
                    //change current layoutinstance
                    var layoutInstances = await _repository.Get(x => x.NodeId == entity.NodeId && x.IsEnabled);
                    var list = layoutInstances?.ToList();
                    if (list != null && list.Any())
                        foreach (var layoutInstanceOld in list)
                        {
                            if (layoutInstanceOld == null) continue;

                            layoutInstanceOld.IsEnabled = false;
                            layoutInstanceOld.CacheSolr = false;
                            await _repository.Update(layoutInstanceOld);
                        }

                    if (node.IsDiagrammable)
                        await SetSolrLayoutInstance(entity);
                    return;
                }

                if (entity.PublicationDate > DateTime.UtcNow) return;

                var layoutSet = await _repository.Get(h => h.NodeId == entity.NodeId &&
                                                     DateTime.UtcNow >= h.PublicationDate);

                var order = layoutSet?.OrderByDescending(c => c.PublicationDate.Year)
                        .ThenByDescending(c => c.PublicationDate.Month)
                        .ThenByDescending(c => c.PublicationDate.Day)
                        .ThenByDescending(c => c.PublicationDate.Hour)
                        .ThenByDescending(c => c.PublicationDate.Minute)
                        .ThenByDescending(c => c.PublicationDate.Second);

                if (order != null && order.Any())
                {
                    //change current layoutinstance
                    var layoutInstances = await _repository.Get(x => x.NodeId == entity.NodeId && x.IsEnabled);
                    var list = layoutInstances?.ToList();
                    if (list != null && list.Any())
                        foreach (var layoutInstanceOld in list)
                        {
                            if (layoutInstanceOld == null) continue;

                            layoutInstanceOld.IsEnabled = false;
                            layoutInstanceOld.CacheSolr = false;
                            await _repository.Update(layoutInstanceOld);
                        }

                    if (node.IsDiagrammable)
                        await SetSolrLayoutInstance(order.FirstOrDefault());
                }
            }
        }

        public async Task SyncAllLayoutInstances(int? nodeId = null)
        {
            if (nodeId == null)
            {
                var res = await _repository.Get(h => DateTime.UtcNow >= h.PublicationDate);
                var nodesIds = res.Select(x => x.NodeId).Distinct().ToList();
                var nodes = _repositoryNode.Get(x => nodesIds.Contains(x.Id) && x.IsDiagrammable && !x.IsDeleted).Result.ToList();
                foreach (var it in nodes)
                {
                    try
                    {
                        var layoutSet = res.Where(h => h.NodeId == it.Id &&
                                                                    DateTime.UtcNow >= h.PublicationDate);

                        var order = layoutSet?.OrderByDescending(c => c.PublicationDate.Year)
                            .ThenByDescending(c => c.PublicationDate.Month)
                            .ThenByDescending(c => c.PublicationDate.Day)
                            .ThenByDescending(c => c.PublicationDate.Hour)
                            .ThenByDescending(c => c.PublicationDate.Minute)
                            .ThenByDescending(c => c.PublicationDate.Second)
                            .ThenByDescending(x => x.Id);

                        if (order == null || !order.Any()) continue;

                        //change current layoutinstance
                        var list = res.Where(x => x.NodeId == it.Id).OrderByDescending(s => s.Id).Take(10).ToList();
                        if (list.First().IsEnabled) continue;
                        if (list.Any())
                        {
                            foreach (var layoutInstanceOld in list)
                            {
                                if (layoutInstanceOld == null) continue;

                                layoutInstanceOld.IsEnabled = false;
                                layoutInstanceOld.CacheSolr = false;
                                await _repository.Update(layoutInstanceOld);
                            }
                        }

                        await SetSolrLayoutInstance(order.FirstOrDefault());
                    }
                    catch (Exception ex)
                    {
                        CMSLogger.Error(ex.Message);
                    }
                }
            }
            else
            {
                try
                {
                    var node = await _repositoryNode.GetById(nodeId.Value);
                    if (node == null) return;

                    var layoutSet = await _repository.Get(h => h.NodeId == nodeId &&
                                                               DateTime.UtcNow >= h.PublicationDate);

                    var order = layoutSet?.OrderByDescending(c => c.PublicationDate.Year)
                        .ThenByDescending(c => c.PublicationDate.Month)
                        .ThenByDescending(c => c.PublicationDate.Day)
                        .ThenByDescending(c => c.PublicationDate.Hour)
                        .ThenByDescending(c => c.PublicationDate.Minute)
                        .ThenByDescending(c => c.PublicationDate.Second)
                        .ThenByDescending(x => x.Id);

                    if (order != null && order.Any())
                    {
                        //change current layoutinstance
                        var layoutInstances = await _repository.Get(x => x.NodeId == nodeId && x.IsEnabled);
                        var list = layoutInstances?.ToList();
                        if (list != null && list.Any())
                            foreach (var layoutInstanceOld in list)
                            {
                                if (layoutInstanceOld == null) continue;

                                layoutInstanceOld.IsEnabled = false;
                                layoutInstanceOld.CacheSolr = false;
                                await _repository.Update(layoutInstanceOld);
                            }

                        if (node.IsDiagrammable)
                            await SetSolrLayoutInstance(order.FirstOrDefault());
                    }
                }

                catch (Exception ex)
                {
                    CMSLogger.Error(ex.Message);
                }
            }

            await SolrHelper.DataImport(SolrCore.LAYOUTINSTANCE, _appSettings.Solr);
        }

        private async Task SetSolrLayoutInstance(LayoutInstance entity)
        {

            System.Threading.Thread.Sleep(1000);

            try
            {
                var instanceSolr = await GenerateLayoutInstsanceSolrAsync(entity);

                //Delete Old Document
                await SolrHelper.DeleteDocumentByQuery(SolrCore.LAYOUTINSTANCEBYNODE,
                    "NodeId:" + entity.NodeId, _appSettings.Solr);

                //Add New Document
                await SolrHelper.AddLayoutInstanceByNode(SolrCore.LAYOUTINSTANCEBYNODE,
                    new List<dynamic>() { instanceSolr }, _appSettings.Solr);


                entity.IsEnabled = true;
                entity.CacheSolr = false;
                await _repository.Update(entity);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }

        }

        private async Task<dynamic> GenerateLayoutInstsanceSolrAsync(LayoutInstance layoutInstance)
        {
            var obj = GetParseStructure(layoutInstance.Structure);

            var templateHtml = await Parse(obj, _appSettings.StructureJson.Path, _appSettings.Content.ImageUrl, _appSettings.Solr);


            dynamic myDynamic = new System.Dynamic.ExpandoObject();

            myDynamic.NodeId = layoutInstance.NodeId;
            myDynamic.NodeDescription = layoutInstance.Description ?? "/";
            myDynamic.NodeIsEnabled = layoutInstance.Node?.IsEnabled;

            if (layoutInstance.PrintEditionId != null && layoutInstance.PrintEditionId != 0)
                myDynamic.PrintEditionId = layoutInstance.PrintEditionId;

            myDynamic.Description = layoutInstance.Description ?? "/";

            var pageResult = await _repositoryAsset.Get(p => !p.IsDeleted && p.AssetNodes.Any(x => x.NodeId == layoutInstance.NodeId) && p is Page && !(p is News));
            var page = await pageResult.FirstOrDefaultAsync();
            var pageContent = page?.AssetContents.OfType<PageContent>().SingleOrDefault();
            myDynamic.Structure = layoutInstance.Structure;

            if (pageContent != null)
            {
                myDynamic.BackgroundColor = pageContent.BackgroundColor;

                if (page.AssetMedia.Any())
                {
                    var assetMedia = page.AssetMedia.FirstOrDefault(m => m.Featured && m.Media.SizesPaths != null) ??
                                     page.AssetMedia.FirstOrDefault(m => m.Media.SizesPaths != null);

                    if (assetMedia != null)
                    {
                        //image header microsite
                        var sizesPaths = JsonConvert.DeserializeObject<dynamic>(assetMedia.Media.SizesPaths);

                        myDynamic.MediaSize1Path = sizesPaths.Size1Path;
                        myDynamic.MediaSize2Path = sizesPaths.Size2Path;
                        myDynamic.MediaSize3Path = sizesPaths.Size3Path;
                        myDynamic.MediaSize4Path = sizesPaths.Size4Path;
                        myDynamic.MediaSize5Path = sizesPaths.Size5Path;
                    }
                }

                //SocialNetwork
                if (pageContent.PageContentSocialNetworks.Any())
                {
                    var urls = new List<string>();
                    var names = new List<string>();

                    foreach (var pcSocialNetwork in pageContent.PageContentSocialNetworks)
                    {
                        urls.Add(pcSocialNetwork.Url);
                        names.Add(pcSocialNetwork.SocialNetwork.Name);
                    }
                    myDynamic.SocialNetworksUrl = string.Join(",", urls);
                    myDynamic.SocialNetworks = string.Join(",", names);
                }
            }

            myDynamic.AssetIds = string.Join(",", AssetHelper.GetNewsIds(obj).ToArray());
            myDynamic.PublicationDate = layoutInstance.PublicationDate;
            myDynamic.LayoutInstanceId = layoutInstance.Id;
            myDynamic.GUID = Guid.NewGuid();
            myDynamic.Html = templateHtml;

            //Minify Html
            myDynamic.Html = Regex.Replace(myDynamic.Html, @"\n|\t", "");
            myDynamic.Html = Regex.Replace(myDynamic.Html, @"&gt;\s+&lt;", "&gt;&lt;").Trim();
            //myDynamic.Html = Regex.Replace(myDynamic.Html, @"\s{2,}", "");


            return myDynamic;
        }


        //Static Methods
        private static LayoutStructureDto GetParseStructure(string entity)
        {
            return JsonConvert.DeserializeObject<LayoutStructureDto>(entity);
        }

        //TODO: mandar ruta bien del structur json
        private static async Task<string> Parse(LayoutStructureDto obj, string pathStructureJson, string imageUrl, SolrConfig config)
        {
            var structure = LayoutHelper.GetStructure(pathStructureJson);

            await obj.ParseComponentsAsync(imageUrl, config);

            var templateHtml = new HtmlFactory(structure, obj, imageUrl).GetHtml();

            return templateHtml;
        }



    }
}
