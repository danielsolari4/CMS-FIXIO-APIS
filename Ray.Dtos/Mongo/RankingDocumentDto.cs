using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ray.Dtos.Mongo
{
    public class RankingDocumentDto
    {
        public int visits { get; set; }
        public string url { get; set; }
        public ObjectId id { get; set; }
        public int assetid { get; set; }
        public int nodeId { get; set; }
        public int total { get; set; }
        public DateTime pubdate { get; set; }
    }
}
