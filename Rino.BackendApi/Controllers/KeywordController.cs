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
using Rino.Utils.Exception;
using Rino.Utils.Solr;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Keyword")]
    public class KeywordController : BaseApiController
    {
        private readonly IKeywordManager _manager;
        private readonly AppSettings _appSettings;

        public KeywordController(IKeywordManager manager, AppSettings appSettings)
        {
            _manager = manager;
            _appSettings = appSettings;
        }

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
                    await SolrHelper.DataImport(SolrCore.KEYWORD, _appSettings.Solr);
                    return Ok(await SolrHelper.ExecuteQuery(SolrCore.KEYWORD, HttpUtility.UrlDecode(HttpContext.Request.QueryString.ToString()),_appSettings.Solr));
                });
        }

        [HttpPut]
        [Route("SetFeaturedKeywords")]
        public async Task<IActionResult> SetFeaturedKeywords(SetFeaturedKeywordsDtoBindingModel dto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.SetFeaturedKeywords(dto);

                    return Ok();
                });
        }
    }
}
