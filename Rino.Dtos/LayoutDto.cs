using Newtonsoft.Json;
using Rino.Dtos.JsonEntities;

namespace Rino.Dtos
{
    public class LayoutDto : BaseDto
    {
        public string Name { get; set; }
        public LayoutJson Structure { get; set; }
        public string StructureJson
        {
            get => JsonConvert.SerializeObject(Structure);
            set => Structure = JsonConvert.DeserializeObject<LayoutJson>(value.Replace("replaceAnd","&&"));
        }

        public int Type { get; set; }
        public bool IsDeleted { get; set; }
    }

}
