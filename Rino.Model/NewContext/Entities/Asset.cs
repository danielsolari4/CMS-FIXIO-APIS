using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Rino.Model.NewContext.Entities
{
    public partial class Asset
    {
        public Asset()
        {
            AssetAssetAssets = new HashSet<AssetAsset>();
            AssetAssetRelatedAssets = new HashSet<AssetAsset>();
            AssetContents = new HashSet<AssetContent>();
            Galleries = new HashSet<AssetGallery>();
            AssetJsons = new HashSet<AssetJson>();
            AssetMedia = new HashSet<AssetMedia>();
            AssetNodes = new HashSet<AssetNode>();
            UserAssets = new HashSet<UserAsset>();
            OnCreated();
        }

        partial void OnCreated();

        public int Id { get; set; }
        public string Status { get; set; }
        public int Version { get; set; }
        public string DataExtension { get; set; }
        public string Discriminator { get; set; }

        public bool IsEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public string CreationUser { get; set; }
        public DateTime CreationDate { get; set; }
        public string LastModificationUser { get; set; }
        public DateTime? LastModificationDate { get; set; }
        public int? AccessTypeId { get; set; }
        public bool? CacheSolr { get; set; }

        public virtual AccessType AccessType { get; set; }
        //public virtual Page Page { get; set; }
        public virtual ICollection<AssetAsset> AssetAssetAssets { get; set; }
        public virtual ICollection<AssetAsset> AssetAssetRelatedAssets { get; set; }
        public virtual ICollection<AssetContent> AssetContents { get; set; }
        public virtual ICollection<AssetGallery> Galleries { get; set; }
        public virtual ICollection<AssetJson> AssetJsons { get; set; }
        public virtual ICollection<AssetMedia> AssetMedia { get; set; }
        public virtual ICollection<AssetNode> AssetNodes { get; set; }
        public virtual ICollection<UserAsset> UserAssets { get; set; }
    }
}
