using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Rino.Dtos;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Rino.FrontendApi.Controllers
{
    public abstract class BaseApiController : ExceptionController.ExceptionController
    {
        protected object CMSResponse(object o, PaginationDto p = null)
        {
            dynamic response = new System.Dynamic.ExpandoObject();
            response.data = o;
            response.pagination = p;

            return response;
        }

        protected LoggedUserDto GetLoggedUser()
        {
            if (User == null) return null;
            var roles = User.Claims.Where(x => x.Type == ClaimTypes.Role)?.ToList();
            int userId;
            var claimsUserId = User.FindAll(ClaimTypes.NameIdentifier).ToList();
            var userIdParsed = int.TryParse(claimsUserId.FirstOrDefault().Value, out userId);
            if (!userIdParsed)
                int.TryParse(claimsUserId[1].Value, out userId);

            return new LoggedUserDto()
            {
                Id = userId,
                Email = User.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault()?.Value,
                Roles = roles.Count > 0 ? roles.Select(x => x.Value).ToList() : new List<string>()
            };
        }

        protected void LoadPagination(PaginationDto pagination, int count)
        {
            pagination.Total = count;

            if (pagination.PageNumber != null && pagination.PageSize != null)
            {
                if (pagination.PageNumber > 1)
                    pagination.Previous = string.Format("{0}?PageNumber={1}&PageSize={2}", Request.GetDisplayUrl(), pagination.PageNumber - 1, pagination.PageSize);
                else
                    pagination.Previous = null;

                if (count > (pagination.PageNumber * pagination.PageSize))
                    pagination.Next = string.Format("{0}?PageNumber={1}&PageSize={2}", Request.GetDisplayUrl(), pagination.PageNumber + 1, pagination.PageSize);
                else
                    pagination.Next = null;
            }
        }
    }
}