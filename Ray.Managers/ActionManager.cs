//using CMS.Dtos;
//using CMS.Managers.Core;
//using CMS.Repositories;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CMS.Managers
//{

//    public interface IActionManager : IManager<Dtos.ActionDto>
//    {
//    }
//    public class ActionManager : BaseManager, IActionManager
//    {
//        protected readonly IActionRepository _repository;


//        protected ActionManager(IActionRepository repository)
//        {
//            _repository = repository;
//        }

//        public Task<ActionDto> Add(ActionDto entity)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<int> Count()
//        {
//            throw new NotImplementedException();
//        }

//        public Task Delete(ActionDto entity)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ICollection<ActionDto>> GetAll(int? skip = null, int? take = null)
//        {
//            try
//            {
//                var urlRedirects = new List<URLRedirectDto>();

//                var all = await _repository.GetAll();

//                foreach (var item in all)
//                {
//                    urlRedirects.Add(_Mapper.Map<URLRedirectDto>(item));

//                }
//                return urlRedirects;
//            }
//            catch (Exception ex)
//            {
//                return null;
//            }
//        }

//        public Task<ActionDto> GetById(int id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task Update(ActionDto entity)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
