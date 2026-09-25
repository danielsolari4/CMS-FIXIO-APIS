using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.BackendApi.Controllers.ExceptionController;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Managers;
using Rino.Repositories;
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Node")]
    public class NodeController : BaseApiController
    {
        private readonly INodeManager _manager;
        private readonly IURLRedirectRepository _repositoryUrlRedirect;
        private readonly AppSettings _appSettings;

        public NodeController(INodeManager manager, IURLRedirectRepository repositoryUrlRedirect, AppSettings appSettings)
        {
            _manager = manager;
            _repositoryUrlRedirect = repositoryUrlRedirect;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id, bool includeContent = false, bool includeAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var node = await _manager.GetById(id, includeContent, includeAssets, languageId);

                    if (node == null)
                        return NotFound();

                    return Ok(CMSResponse(node));
                });
        }
        [HttpGet, Route("GetAll")]
        public async Task<IActionResult> GetAll(PaginationDto pagination, bool includeContent = false, bool includeAssets = false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    LoadPagination(pagination, await _manager.Count());
                    return Ok(CMSResponse(await _manager.GetAll(includeContent, includeAssets, languageId, pagination.Skip(), pagination.Take()), pagination));
                });
        }

        [HttpGet]
        [Route("GetAllSolr")]
        public async Task<IActionResult> GetAllSolr()
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
                });
        }

        [Route("GetTree")]
        public async Task<IActionResult> GetTree(int nodeId = 0, bool includeContent = true, bool onlyActive=false, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
                {
                    return Ok(CMSResponse(_manager.GetTree(nodeId, includeContent, onlyActive, languageId)));
                });
        }

        [Route("GetTreeMicrosite")]
        public async Task<IActionResult> GetTreeMicrosite(int nodeId = 0, bool includeContent = true, int? languageId = null)
        {
            return await TryJsonResultAsync(async () =>
            {
                return Ok(CMSResponse(_manager.GetTreeMicrosite(nodeId, includeContent, languageId)));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(CreateNodeDtoBindingModel nodeDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (_repositoryUrlRedirect.Get(x => !x.IsDeleted && x.From == nodeDto.Description).Result.Any())
                        throw new AlreadyExistsException("redirect");

                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(nodeDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(UpdateNodeDtoBindingModel nodeDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (_repositoryUrlRedirect.Get(x => !x.IsDeleted && x.From == nodeDto.Description).Result.Any())
                        throw new AlreadyExistsException("redirect");

                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(nodeDto);

                    return Ok();
                });
        }

        [HttpDelete]
        [Route("Delete")]
        public async Task<IActionResult> Delete(DeleteNodeDtoBindingModel nodeDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Delete(nodeDto);

                    return Ok();
                });
        }
    }
}
