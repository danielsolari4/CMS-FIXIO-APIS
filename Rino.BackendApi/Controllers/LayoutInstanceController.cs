using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.BackendApi.Controllers.ExceptionController;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Repositories;
using Rino.Utils.Configuration;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/LayoutInstance")]
    public class LayoutInstanceController : BaseApiController
    {
        private readonly ILayoutInstanceManager _manager;
        private readonly INodeManager _nodeManager;
        private readonly INodeRepository _nodeRepository;
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _env;

        public LayoutInstanceController(ILayoutInstanceManager manager, INodeManager nodeManager,
            INodeRepository nodeRepository, IUserManager userManager, AppSettings appSettings, IWebHostEnvironment env)
        {
            _manager = manager;
            _nodeManager = nodeManager;
            _nodeRepository = nodeRepository;
            _userManager = userManager;
            _appSettings = appSettings;
            _env = env;
        }

        [AllowAnonymous]
        [Route("GetHtml")]
        public async Task<HttpResponseMessage> GetHtml(int id)
        {
            var layoutInstance = await _manager.GetById(id);
            HttpResponseMessage response;

            if (layoutInstance != null)
            {
                response = new HttpResponseMessage();
                //response.Content = new StringContent(layoutInstance.Layout.Html, Encoding.GetEncoding("utf-8"));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            }
            else
            {
                response = new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return response;
        }

        [AllowAnonymous]
        [Route("GetStructure")]
        public async Task<string> GetStructure(int id)
        {
            var contentRoot = $"{_env.ContentRootPath}/HtmlTemplate/Json/Structure.json";
            return await System.IO.File.ReadAllTextAsync(contentRoot);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var layoutInstance = await _manager.GetById(id);

                    if (layoutInstance == null)
                        return NotFound();

                    #region Change Create and Modification User Info

                    //var userCreationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == layoutInstance.CreationUser);

                    //if (userCreationInfo != null)
                    //    layoutInstance.CreationUser = userCreationInfo.FirstName + " " + userCreationInfo.LastName;

                    //var userModificationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == layoutInstance.LastModificationUser);

                    //if (userModificationInfo != null)
                    //    layoutInstance.LastModificationUser = userModificationInfo.FirstName + " " + userModificationInfo.LastName;

                    #endregion

                    return Ok(CMSResponse(layoutInstance));
                });
        }

        [HttpGet]
        [Route("GetByNodeId")]
        public async Task<IActionResult> GetByNodeId(int nodeId)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var layoutInstance = await _manager.GetByNodeId(nodeId);

                    if (layoutInstance == null)
                        return NotFound();

                    #region Change Create and Modification User Info

                    var userCreationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == layoutInstance.CreationUser);

                    if (userCreationInfo != null)
                        layoutInstance.CreationUser = userCreationInfo.FirstName + " " + userCreationInfo.LastName;

                    var userModificationInfo = _userManager.GetAll().Result.FirstOrDefault(x => x.UserName == layoutInstance.LastModificationUser);

                    if (userModificationInfo != null)
                        layoutInstance.LastModificationUser = userModificationInfo.FirstName + " " + userModificationInfo.LastName;

                    #endregion

                    return Ok(CMSResponse(layoutInstance));
                });
        }

        [HttpGet]
        [Route("GetChildByNodeId")]
        public async Task<IActionResult> GetChildByNodeId(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var layoutInstances = _manager.GetLayoutsByNodeId(id);

                if (layoutInstances == null)
                    return NotFound();

                return Ok(CMSResponse(layoutInstances));
            });
        }

        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(pagination.Skip(), pagination.Take()), pagination));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.LAYOUTINSTANCE,
                        HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()), _appSettings.Solr));
                });
        }

        [HttpGet]
        [Route("GetSolrByNodeId")]
        public async Task<IActionResult> GetSolrByNodeId(int nodeId)
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.GetLayoutInstanceByNodeId(nodeId, _appSettings.Solr));
                });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]LayoutInstanceDto layoutInstanceDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    return Ok(CMSResponse(await _manager.Add(layoutInstanceDto)));
                });
        }

        [HttpPost]
        [Route("SyncSolrByNode")]
        public async Task<IActionResult> SyncSolrByNode(SyncLayoutInstanceDtoBindingModel model)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (model != null && model.NodeId.HasValue && model.NodeId > 0)
                        await SyncLayoutInstances(model.NodeId);

                    await SyncLayoutInstances();
                    return Ok();
                });
        }

        [HttpPut]
        [Route("Deactivate")]
        public async Task<IActionResult> Deactivate(int nodeId, bool deactivate)
        {
            return await TryJsonResultAsync(async () =>
            {
                if (nodeId == 0) return null;

                if (deactivate)
                {
                    var node = await _nodeRepository.GetById(nodeId);
                    node.IsDiagrammable = false;
                    node.CacheSolr = false;
                    await _nodeRepository.Update(node);
                    await _manager.Deactivate(nodeId);
                }
                else
                {
                    var node = await _nodeRepository.GetById(nodeId);
                    node.IsDiagrammable = true;
                    node.CacheSolr = false;
                    await _nodeRepository.Update(node);
                    await _manager.SyncAllLayoutInstances(nodeId);
                }

                await SolrHelper.DataImport(SolrCore.NODE, _appSettings.Solr);

                return Ok();
            });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("InternalSyncSolrByNode")]
        public async Task<IActionResult> InternalSyncSolrByNode()
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (HttpContext.Request.Headers.Count > 0 && HttpContext.Request.Headers["Authorization"].FirstOrDefault() != null)
                    {
                        var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                        if (string.IsNullOrWhiteSpace(token) || !token.Equals(_appSettings.SyncLayout.Token))
                            return NotFound();

                        await SyncLayoutInstances();
                        return Ok();
                    }

                    return NotFound();
                });

        }

        private async Task SyncLayoutInstances(int? nodeId = null)
        {
            await TryJsonResultAsync(async () =>
            {
                await _manager.SyncAllLayoutInstances(nodeId);
                return Ok();
            });
        }
    }
}
