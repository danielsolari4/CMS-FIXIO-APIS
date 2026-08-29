using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Ray.Dtos;
using Ray.Utils.Configuration;
using Ray.Utils.Exception;

namespace Ray.Managers.Helpers
{
    public static class LayoutHelper
    {
        public static JsonStructure GetStructure(string pathStructureJson)
        {
            var directory = System.IO.Directory.GetCurrentDirectory();
            var dynamicRoot = directory + pathStructureJson;
            
            if (File.Exists($"{dynamicRoot}"))
                return JsonConvert.DeserializeObject<JsonStructure>(
                    File.ReadAllText($"{dynamicRoot}"));

            throw new ConfigurationException("File Not Found - Json Structure");
        }
    }
}
