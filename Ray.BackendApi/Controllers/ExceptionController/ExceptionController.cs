using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ray.Utils.Exception;

namespace Ray.BackendApi.Controllers.ExceptionController
{
    public class ExceptionController : ControllerBase
    {
        protected virtual IActionResult TryAction(Func<IActionResult> action)
        {
            try
            {
                var res = Try.It(action);
                return res;
            }
            catch (EntityException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("ENEX_001-" + ex.Message);
            }
            catch (AlreadyExistsException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("AEEX_001-" + ex.Message);
            }
            catch (InUseException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("IUEX_001-" + ex.Message);
            }
            catch (ConfigurationException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("CNEX_001-" + ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("ANEX_001-" + ex.Message);
            }
            catch (ArgumentException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("AREX_001");
            }
            //catch (DbUpdateException ex)
            //{
            //    LogError(ex);
            //    CatchError(ex.Message);
            //    return BadRequest("DBEX_001");
            //}
            catch (ItemNotFoundException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("NFEX_001-" + ex.Message);
            }
            catch (DeleteException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("DLEX_001-" + ex.Message);
            }
            catch (Exception ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest(ex.Message);
            }
        }


        protected virtual async Task<IActionResult> TryJsonResultAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                var res = await Try.It(action);
                return res;
            }
            catch (EntityException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("ENEX_001-" + ex.Message);
            }
            catch (AlreadyExistsException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("AEEX_001-" + ex.Message);
            }
            catch (InUseException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("IUEX_001-" + ex.Message);
            }
            catch (ConfigurationException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("CNEX_001-" + ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("ANEX_001-" + ex.Message);
            }
            catch (ArgumentException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("AREX_001");
            }
            //catch (DbUpdateException ex)
            //{
            //    LogError(ex);
            //    CatchError(ex.Message);
            //    return BadRequest("DBEX_001");
            //}
            catch (ItemNotFoundException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("NFEX_001-" + ex.Message);
            }
            catch (DeleteException ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest("DLEX_001-" + ex.Message);
            }
            catch (Exception ex)
            {
                LogError(ex);
                CatchError(ex.Message);
                return LegacyBadRequest(ex.Message);
            }
        }

        protected virtual FileStreamResult TryFileStreamResult(Func<FileStreamResult> action)
        {
            FileStreamResult result = null;

            try
            {
                Try.It(() => { result = action(); });
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex);
                CatchProccess(ex);
                return result;
            }
        }

        protected virtual FileContentResult TryFileContentResult(Func<FileContentResult> action)
        {
            FileContentResult result = null;

            try
            {
                Try.It(() => { result = action(); });

                return result;
            }
            catch (Exception ex)
            {
                LogError(ex);
                CatchProccess(ex);
                return result;
            }
        }



        protected virtual void CatchProccess(Exception ex)
        {
            ModelState.AddModelError("", "An error occurred. Please try again in a few minutes.");
        }

        protected virtual void CatchError(string mes)
        {
            ModelState.AddModelError("", mes);
        }

        protected IActionResult LegacyBadRequest(string message)
        {
            return BadRequest(new { Message = message });
        }



        private void LogError(Exception exceptionMessage)
        {
            try
            {
                var logDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDirectory);
                var filePath = Path.Combine(logDirectory, "Error.txt");

                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("Message :" + exceptionMessage.Message + "<br/>" + Environment.NewLine + "StackTrace :" + exceptionMessage.StackTrace +
                                     "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }


    public static class ModelStateExtensions
    {
        public static string GetErrorMessage(this ModelStateDictionary modelState)
        {
            IEnumerable<ModelError> allErrors = modelState.Values.SelectMany(v => v.Errors);
            var modelErrors = allErrors as ModelError[] ?? allErrors.ToArray();
            return string.Join("\r\n ", modelErrors.Select(x => x.ErrorMessage).ToArray());
        }
    }

    public static class MethodInfoExtensions
    {
        public static string GetMethodName(this MethodInfo mi)
        {
            var str = string.Empty;
            return mi.Name.LastIndexOf('<') == -1 ? mi.Name : mi.Name.Substring(1, mi.Name.LastIndexOf('>') - 1);
        }

        public static string GetClassName(this MethodInfo mi)
        {
            var str1 = string.Empty;
            string str2 = null;
            if (mi.DeclaringType != null && mi.DeclaringType.FullName.LastIndexOf('+') != -1)
            {
                var fullName = mi.DeclaringType.FullName;
                str2 = fullName.Substring(0, fullName.LastIndexOf('+'));
            }
            else if (mi.DeclaringType != null) str2 = mi.DeclaringType.FullName;
            return str2;
        }
    }
}
