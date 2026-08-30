using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ray.Dtos;
using Ray.Dtos.Configuration;
using Ray.Managers;
using Ray.Repositories;
using Ray.Utils.Solr;
using SolrCore = Ray.Utils.Solr.SolrCore;

namespace Ray.FrontendApi.Controllers
{
    [AllowAnonymous]
    [Route("api/LayoutInstance")]
    public class LayoutInstanceController : BaseApiController
    {
        private readonly ILayoutInstanceManager _manager;
        private readonly INodeManager _nodeManager;
        private readonly INodeRepository _nodeRepository;
        private readonly IUserManager _userManager;
        private readonly AppSettings _appSettings;

        public LayoutInstanceController(ILayoutInstanceManager manager, INodeManager nodeManager,
            INodeRepository nodeRepository, IUserManager userManager, AppSettings appSettings)
        {
            _manager = manager;
            _nodeManager = nodeManager;
            _nodeRepository = nodeRepository;
            _userManager = userManager;
            _appSettings = appSettings;
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
                response.Content = new StringContent(layoutInstance.StructureJson.ToString(), Encoding.GetEncoding("utf-8"));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            }
            else
            {
                response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

            return response;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
            {
                var layoutInstance = await _manager.GetById(id);

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

        [HttpGet]
        [HttpGet("GetAll")]
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
                    HttpUtility.UrlDecode(Request.QueryString.ToString()), _appSettings.Solr));
            });
        }

        [HttpGet]
        [Route("GetSolrByNodeId")]
        public async Task<IActionResult> GetSolrByNodeId(int nodeId)
        {
            return await TryJsonResultAsync(async () =>
            {
                var result = await SolrHelper.GetLayoutInstanceByNodeId(nodeId, _appSettings.Solr);
                return Ok(result);
            });
        }
    }
}
