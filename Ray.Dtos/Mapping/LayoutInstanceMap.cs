using System.Linq;
using Newtonsoft.Json;
using Ray.Dtos.JsonEntities;
using Ray.Model.NewContext.Entities;

namespace Ray.Dtos.Mapping
{
    public static class LayoutInstanceMap
    {
        public static LayoutInstance Map(this LayoutInstanceDto model)
        {
            return new LayoutInstance
            {
                Id = model.Id,
                NodeId = model.NodeId,
                LayoutId = model.LayoutId,
                LayoutType = model.LayoutType,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                PublicationDate = model.PublicationDate,
                Description = model.Description,
                Structure = GetJsonEntityStructure(model.StructureJson),
                PrintEditionId = model.PrintEditionId
            };
        }

        public static LayoutInstanceDto Map(this LayoutInstance model)
        {
            return new LayoutInstanceDto
            {
                Id = model.Id,
                NodeId = model.NodeId,
                NodeName = model.Node?.Content?.FirstOrDefault()?.Title,
                LayoutId = model.LayoutId,
                LayoutType = model.LayoutType,
                CreationUser = model.CreationUser,
                CreationDate = model.CreationDate,
                PublicationDate = model.PublicationDate,
                Description = model.Description,
                StructureJson = JsonConvert.DeserializeObject<LayoutStructureDto>(model.Structure),
                PrintEditionId = model.PrintEditionId
            };
        }

        public static string GetJsonEntityStructure(LayoutStructureDto structure)
        {
            var ser = JsonConvert.SerializeObject(structure);
            var obj = JsonConvert.DeserializeObject<LayoutInstanceJson>(ser);
            return JsonConvert.SerializeObject(obj);
        }
    }
}
