using System;
using System.Collections.Generic;

#nullable disable

namespace Ray.Model.NewContext.Entities
{
    public partial class Node
    {
        public Node()
        {
            AssetNodes = new HashSet<AssetNode>();
            CategoryNodes = new HashSet<CategoryNode>();
            Childs = new HashSet<Node>();
            LayoutInstances = new HashSet<LayoutInstance>();
            Content = new HashSet<NodeContent>();
            NodeKeywords = new HashSet<NodeKeyword>();
            PrintEditions = new HashSet<PrintEdition>();
            ProgrammingGuideNodes = new HashSet<ProgrammingGuideNode>();
            ProgrammingGuides = new HashSet<ProgrammingGuide>();
            UserNodes = new HashSet<UserNode>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public int? Order { get; set; }
        public int? ParentNodeId { get; set; }
        public bool IsPrint { get; set; }
        public bool? IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public string LastModificationUser { get; set; }
        public bool IsPublished { get; set; }
        public bool? CacheSolr { get; set; }
        public bool IsDiagrammable { get; set; }
        public string SeoTitle { get; set; }
        public string SeoDescription { get; set; }
        public string Keywords { get; set; }
        public string OGTitle { get; set; }
        public string OGDescription { get; set; }
        public string SeoImage { get; set; }
        public int? NewSourceId { get; set; }

        public virtual NewsSource NewSource { get; set; }
        public virtual Node ParentNode { get; set; }
        public virtual ICollection<AssetNode> AssetNodes { get; set; }
        public virtual ICollection<CategoryNode> CategoryNodes { get; set; }
        public virtual ICollection<Node> Childs { get; set; }
        public virtual ICollection<LayoutInstance> LayoutInstances { get; set; }
        public virtual ICollection<NodeContent> Content { get; set; }
        public virtual ICollection<NodeKeyword> NodeKeywords { get; set; }
        public virtual ICollection<PrintEdition> PrintEditions { get; set; }
        public virtual ICollection<ProgrammingGuideNode> ProgrammingGuideNodes { get; set; }
        public virtual ICollection<ProgrammingGuide> ProgrammingGuides { get; set; }
        public virtual ICollection<UserNode> UserNodes { get; set; }
    }
}
