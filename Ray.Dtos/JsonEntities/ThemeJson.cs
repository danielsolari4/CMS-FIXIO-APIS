using System.Collections.Generic;
using Newtonsoft.Json;
using Ray.Dtos.Factories;

namespace Ray.Dtos.JsonEntities
{
    public enum MenuItemType
    {
        Internal = 1,
        External = 2,
        Asset = 3
    }

    public enum MenuType
    {
        Header = 1,
        Footer = 2,
        Sidebar = 3
    }

    public enum ColorType
    {
        NoColor = 1,
        Color = 2,
        ParentColor = 3
    }

    public class MenuJson
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public List<ItemMenu> Items { get; set; }
    }

    public class ItemType
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }

    public class ItemMenu
    {
        public string Title { get; set; }
        public string Label { get; set; }
        public ItemBase Menu { get; set; }
        public List<ItemMenu> Childs { get; set; }
        public bool ActiveMenuProgram { get; set; }
    }

    [JsonConverter(typeof(MenuCreationConverter))]
    public abstract class ItemBase
    {
        public int MenuType { get; set; }
        public string Url { get; set; }
        //public int ColorType { get; set; }
        //public string Color { get; set; }
    }

    public class InternalItem : ItemBase
    {
        public int NodeId { get; set; }
    }

    public class ExternalLink : ItemBase
    {
    }

    public class AssetItem : ItemBase
    {
        public int AssetId { get; set; }
    }
}
