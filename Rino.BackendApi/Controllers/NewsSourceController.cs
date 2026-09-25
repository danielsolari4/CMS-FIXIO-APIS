using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rino.BackendApi.Attributes;
using Rino.BackendApi.Controllers.ExceptionController;
using Rino.Dtos;
using Rino.Managers;
using Rino.Utils.Exception;

namespace Rino.BackendApi.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "HasPermissionPolicy")]
    [Route("api/NewsSource")]
    public class NewsSourceController : BaseApiController
    {
        private readonly INewsSourceManager _manager;

        public NewsSourceController(INewsSourceManager manager)
        {
            _manager = manager;
        }

        public async Task<IActionResult> Get(int id)
        {
            return await TryJsonResultAsync(async () =>
                {
                    var newSource = await _manager.GetById(id);

                    if (newSource == null)
                        return NotFound();

                    return Ok(CMSResponse(newSource));
                });
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            return await TryJsonResultAsync(async () =>
            {
                var list = await _manager.GetAll();

                if (list == null)
                    return NotFound();

                return Ok(CMSResponse(list));
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post(NewsSourceDto NewsSourceDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    var result = await _manager.Add(NewsSourceDto);

                    return Ok(CMSResponse(result));
                });
        }

        [HttpPut]
        public async Task<IActionResult> Put(NewsSourceDto NewsSourceDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());

                    await _manager.Update(NewsSourceDto);

                    return Ok();
                });
        }
        [HttpDelete]
        [HttpPost, Route("Delete")]
        public async Task<IActionResult> Delete(NewsSourceDto NewsSourceDto)
        {
            return await TryJsonResultAsync(async () =>
                {
                    if (!ModelState.IsValid)
                        throw new ModelException(ModelState.GetErrorMessage());
                    
                    await _manager.Delete(NewsSourceDto);

                    return Ok();
                });
        }


        [HttpGet, Route("GetFilesList")]
        public FileInfo[] GetFilesList(string path)
        {
            //var pat = string.Format(@"{0}\", ConfigurationHelper.GetValue<string>("CMS.Media.FileUpload.Root.Folder"));
            
            try
            {          
                DirectoryInfo dir = new DirectoryInfo(path);
                var listado = dir.GetFiles();
                return listado;
            }
            catch (System.Exception)
            {

                return null;
            }
            

            
        }
    }
}
