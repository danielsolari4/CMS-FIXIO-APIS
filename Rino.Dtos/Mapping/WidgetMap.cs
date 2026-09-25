using Rino.Model.NewContext.Entities;

namespace Rino.Dtos.Mapping
{
    public static class WidgetMap
    {
        public static Widget Map(this WidgetDto model)
        {
            return new Widget
            {
                Id = model.Id,
                Html = model.Html,
                Name = model.Name,
                IsDeleted = model.IsDeleted,
                WidgetTypeId = model.WidgetTypeId,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                LastModificationUser = model.LastModificationUser,
                CacheSolr = model.CacheSolr,
                Icon = model.Icon
            };
        }

        public static WidgetDto Map(this Widget model)
        {
            return new WidgetDto
            {
                Id = model.Id,
                Html = model.Html,
                Name = model.Name,
                IsDeleted = model.IsDeleted,
                WidgetTypeId = model.WidgetTypeId,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                LastModificationDate = model.LastModificationDate,
                LastModificationUser = model.LastModificationUser,
                CacheSolr = model.CacheSolr,
                Icon = model.Icon
            };
        }


        public static Widget Map(this Widget entity, WidgetDto model)
        {
            entity.Id = model.Id;
            entity.Html = model.Html;
            entity.Name = model.Name;
            entity.IsDeleted = model.IsDeleted;
            entity.WidgetTypeId = model.WidgetTypeId;
            entity.CreationUser = model.CreationUser;
            entity.CreationDate = model.CreationDate;
            entity.LastModificationDate = model.LastModificationDate;
            entity.LastModificationUser = model.LastModificationUser;
            entity.CacheSolr = model.CacheSolr;
            entity.Icon = model.Icon;

            return entity;
        }
    }
}
