using Newtonsoft.Json;
using Ray.Dtos.JsonEntities;
using Ray.Model.NewContext.Entities;

namespace Ray.Dtos.Mapping
{
    public static class LayoutMap
    {
        public static Layout Map(this LayoutDto model)
        {
            return new Layout
            {
                Id = model.Id,
                IsDeleted = model.IsDeleted,
                Structure = JsonConvert.SerializeObject(model.Structure),
                Type = model.Structure.LayoutTypeId,
                Name = model.Name,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                LastModificationUser = model.LastModificationUser
            };
        }

        public static LayoutDto Map(this Layout model)
        {
            return new LayoutDto
            {
                Id = model.Id,
                Structure = JsonConvert.DeserializeObject<LayoutJson>(model.Structure),
                Name = model.Name,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                LastModificationUser = model.LastModificationUser
            };
        }


        public static Layout Map(this Layout entity, LayoutDto model)
        {
            entity.Id = model.Id;
            entity.Structure = JsonConvert.SerializeObject(model.Structure);
            entity.Type = model.Structure.LayoutTypeId;
            entity.Name = model.Name;
            entity.CreationUser = model.CreationUser;
            entity.CreationDate = model.CreationDate;
            entity.LastModificationDate = model.LastModificationDate;
            entity.LastModificationUser = model.LastModificationUser;
            return entity;
        }
    }
}
