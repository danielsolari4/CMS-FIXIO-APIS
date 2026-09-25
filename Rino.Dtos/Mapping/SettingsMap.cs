using Rino.Model.NewContext.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rino.Dtos.Mapping
{
    public static class SettingsMap
    {
        public static Settings Map(this SettingsDto model)
        {
            return new Settings
            {
                Id = model.Id,
                Name = model.Name,
                Value = model.Value
            };
        }

        public static SettingsDto Map(this Settings model)
        {
            return new SettingsDto
            {
                Id = model.Id,
                Name = model.Name,
                Value = model.Value
            };
        }


        public static Settings Map(this Settings entity, SettingsDto model)
        {
            entity.Id = model.Id;
            entity.Value = model.Value;
            entity.Name = model.Name;

            return entity;
        }
    }
}
