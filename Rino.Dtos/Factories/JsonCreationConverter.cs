using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rino.Dtos.Components;
using Rino.Dtos.JsonEntities;

namespace Rino.Dtos.Factories
{
    public abstract class JsonCreationConverter<T> : JsonConverter
    {
        protected abstract T Create(Type objectType, JObject jsonObject);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObject = JObject.Load(reader);
            var target = Create(objectType, jsonObject);
            serializer.Populate(jsonObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            JObject jo = new JObject();
            Type type = value.GetType();
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                    }
                }
            }
            jo.WriteTo(writer);
        }
    }

    public class ComponentCreationConverter : JsonCreationConverter<ComponentBaseDto>
    {
        protected override ComponentBaseDto Create(Type objectType, JObject jsonObject)
        {
            string typeName = (jsonObject["type"])?.ToString() ?? (jsonObject["Type"]).ToString();
            if (!string.IsNullOrEmpty(typeName))
                return ComponentFactory.GetInstance(Convert.ToInt32(typeName));

            return null;
        }
    }


    public class ComponentInstanceCreationConverter : JsonCreationConverter<ComponentInstanceBaseJson>
    {
        protected override ComponentInstanceBaseJson Create(Type objectType, JObject jsonObject)
        {
            string typeName = (jsonObject["type"])?.ToString() ?? (jsonObject["Type"]).ToString();
            return !string.IsNullOrEmpty(typeName) ? ComponentFactory.GetJsonInstance(Convert.ToInt32(typeName)) : null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            JObject jo = new JObject();
            Type type = value.GetType();
            foreach (PropertyInfo prop in type.GetProperties())
            {
                if (prop.CanRead)
                {
                    object propVal = prop.GetValue(value, null);
                    if (propVal != null)
                    {
                        jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
                    }
                }
            }
            jo.WriteTo(writer);
        }

        public static T Cast<T>(object o)
        {
            return (T)o;
        }
    }

    public class MenuCreationConverter : JsonCreationConverter<ItemBase>
    {
        protected override ItemBase Create(Type objectType, JObject jsonObject)
        {
            var typeName = (jsonObject["menuType"])?.ToString() ?? (jsonObject["MenuType"]).ToString();

            if (string.IsNullOrEmpty(typeName)) return null;
            
            var type = (MenuItemType)(Convert.ToInt32(typeName));
            switch (type)
            {
                case MenuItemType.Internal: return new InternalItem();
                case MenuItemType.External: return new ExternalLink();
                case MenuItemType.Asset: return new AssetItem();
                default:
                    return null;
            }
        }
    }
}
