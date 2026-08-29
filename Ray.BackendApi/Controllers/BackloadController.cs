
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ray.BackendApi.Attributes;
using Ray.Managers;

namespace Ray.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/Backload")]
    public class BackloadController : BaseApiController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IBackloadManager _backloadManager;

        public BackloadController(IHttpContextAccessor httpContextAccessor, IBackloadManager backloadManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _backloadManager = backloadManager;
        }

        /// <summary>
        /// El reconocimiento de los archivos separados, cómo unirlos y cuánto falta para ello se basan en el header "Content-Range" enviado por el front con el
        /// plugin de file upload.
        /// Si ese header no es enviado, no importa cuánto pese el archivo, se toma como un file upload normal
        /// </summary>
        /// <returns></returns>
        /// 
        [HttpPost]
        public async Task<JsonResult> Post()
        {
            var result = await _backloadManager.SaveFile(_httpContextAccessor, Request);
            return new JsonResult(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string fileName)
        {
            await _backloadManager.Delete(fileName);
            return Ok();
        }
    }
}

