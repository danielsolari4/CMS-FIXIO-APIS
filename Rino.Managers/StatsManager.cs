using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.Mongo;
using Rino.Utils.Solr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using SolrCore = Rino.Utils.Solr.SolrCore;

namespace Rino.Managers
{
    public interface IStatsManager
    {
        Task ViewCountOld(int Id, int nodeId, DateTime PublicationDate);
        Task ViewCount(int Id, int nodeId, DateTime publicationDate, string slug);
        Task ShareCount(int Id, DateTime PublicationDate);
        Task<string> GetMostReadOldAsync(int total, int nodeId);
        public string GetMostRead(int total);
        public string GetMostShared(int total);
    }

    public class StatsManager : IStatsManager
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        private readonly AppSettings _appSettings;
        private const string _nameCollectionAssetViewCount = "assetviewscounts";

        public StatsManager(AppSettings appSettings)
        {
            _appSettings = appSettings;
            _client = new MongoClient(_appSettings.MongoDb.ConnectionString);
            _database = _client.GetDatabase(_appSettings.MongoDb.DatabaseName);
        }


        public async Task ViewCountOld(int Id, int nodeId, DateTime PublicationDate)
        {
            try
            {
                //View Count
                var filter = Builders<BsonDocument>.Filter.Eq("assetid", Id);
                var update = new BsonDocument("$inc", new BsonDocument { { "total", 1 } });
                var coll = _database.GetCollection<BsonDocument>("assetviewscounts");
                var doc = coll.FindOneAndUpdateAsync(filter, update).Result;

                if (doc == null)
                {
                    var document = new BsonDocument
                        {
                            { "assetid", Id },
                            { "total", 1 },
                            { "nodeId", nodeId },
                            { "pubdate", PublicationDate },
                            { "currentdate", DateTime.UtcNow }
                        };

                    var collection = _database.GetCollection<BsonDocument>("assetviewscounts");
                    await collection.InsertOneAsync(document);
                }

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task ViewCount(int Id, int nodeId, DateTime publicationDate, string slug)
        {
            try
            {
                //View Count
                if (publicationDate == null)
                    publicationDate = DateTime.UtcNow;

                var exist = await CollectionExistsAsync(_nameCollectionAssetViewCount);
                if (!exist)
                {
                    ////102400
                    //var options = new CreateCollectionOptions { Capped = true, MaxSize = 400, };
                    await _database.CreateCollectionAsync(_appSettings.MongoDb.NameCollectionAssetsViewCount);
                    await _database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("assetid"));
                    await _database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("slug"));
                    await _database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(1) });

                    //await _database.GetCollection<BsonDocument>(_nameCollectionAssetViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("assetid"));
                }

                var document = new BsonDocument
                        {
                            { "assetid", Id },
                            { "nodeId", nodeId },
                            { "pubdate", publicationDate },
                            { "expireAt" , DateTime.Now},
                            { "slug", slug.ToLower() }
                        };

                var collection = _database.GetCollection<BsonDocument>(_nameCollectionAssetViewCount);
                await collection.InsertOneAsync(document);

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task ShareCount(int Id, DateTime PublicationDate)
        {
            try
            {
                //Share Count
                var filter = Builders<BsonDocument>.Filter.Eq("assetid", Id);
                var update = new BsonDocument("$inc", new BsonDocument { { "total", 1 } });
                var coll = _database.GetCollection<BsonDocument>("assetsharecounts");
                var doc = coll.FindOneAndUpdateAsync(filter, update).Result;

                if (doc == null)
                {
                    var document = new BsonDocument
                        {
                            { "assetid", Id },
                            { "total", 1 },
                            { "pubdate", PublicationDate }
                        };

                    var collection = _database.GetCollection<BsonDocument>("assetsharecounts");
                    await collection.InsertOneAsync(document);
                }

                return;
            }
            catch
            {
                return;
            }
        }
        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            _client = new MongoClient(_appSettings.MongoDb.ConnectionString + "?uuidRepresentation=Standard");
            _database = _client.GetDatabase(_appSettings.MongoDb.DatabaseName);

            var filter = new BsonDocument("name", collectionName);
            //filter by collection name
            var collections = await _database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            //check for existence
            return await collections.AnyAsync();
        }

        public async Task<string> GetMostReadOldAsync(int total, int nodeId)
        {
            try
            {
                string MostReadList = string.Empty;
                var mainNode = 0;

                var response = await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode("q=-ParentNodeId:[* TO *]  AND IsPublished:true AND IsDeleted:false"), _appSettings.Solr);

                var nodeResult = ParseValidResponse(response.ToString());
                if (nodeResult != null)
                {

                    var node = new NodeDto();
                    mainNode = (int)nodeResult.Id;
                }

                string excludeNodes = _appSettings.MongoDb.GetMostReadExclude;
                int[] excludeNodesSeparados = excludeNodes.Split(';').Select(x => Convert.ToInt32(x)).ToArray();

                var filter = Builders<RankingDocumentDto>.Filter.Nin(x => x.nodeId, excludeNodesSeparados);
                if (nodeId > 0 && nodeId != mainNode)
                {
                    var nodeSlug = string.Empty;
                    var response2 = await SolrHelper.ExecuteQuery(SolrCore.NODE, HttpUtility.UrlDecode("q=Id:" + nodeId.ToString() + " AND IsPublished:true&fl=Id,Title_en,ParentNodeId,Description&sort=Title_en asc&rows=1"), _appSettings.Solr);

                    var nodeResult2 = ParseValidResponse(response2.ToString());
                    if (nodeResult2 != null)
                    {
                        //var node = new NodeDto(nodeResult2);
                        nodeSlug = nodeResult2.Description;
                    }
                    filter = Builders<RankingDocumentDto>.Filter.Regex("slug", "^" + nodeSlug + ".*");
                }


                var coll = _database.GetCollection<RankingDocumentDto>("assetviewscounts");

                var result = coll.Aggregate()
                    .Match(filter)
                    .Group(new BsonDocument { { "_id", "$assetid" }, { "count", new BsonDocument("$sum", 1) } })
                    .Match(new BsonDocument { { "count", new BsonDocument("$gt", 0) } })
                    .Sort(new BsonDocument { { "count", -1 } })
                    .Limit(total).ToCursor();

                var i = total;
                foreach (var document in result.ToEnumerable())
                {
                    if (!String.IsNullOrEmpty(MostReadList))
                        MostReadList += " ";
                    MostReadList += document["_id"] + "^" + i;
                    i--;
                }

                return MostReadList;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }

        public string GetMostRead(int total)
        {
            try
            {
                var client = new WebClient();
                client.Headers.Add("cache-control", "no-cache");
                client.Headers.Add("Host", "www.addthis.com");
                client.Headers.Add("Cache-Control", "no-cache");
                client.Headers.Add("Accept", "*/*");
                client.Headers.Add("x-api-key", _appSettings.AddThis.XApiKey);

                var ss = client.DownloadString(string.Format(_appSettings.AddThis.Url, _appSettings.AddThis.Token) + "/visits/urls.json?range=24hours");
                var result = JsonConvert.DeserializeObject<List<RankingDocumentDto>>(ss).OrderByDescending(_ => _.visits).Take(total);

                var mostReadList = string.Empty;
                var i = total;
                var id = string.Empty;
                foreach (var document in result)
                {
                    if (document.url.Contains('?'))
                    {
                        id = document.url.Split('?').FirstOrDefault().Split('_').LastOrDefault();
                    }
                    else
                        id = document.url.Split('_').LastOrDefault();

                    if (!string.IsNullOrEmpty(mostReadList))
                        mostReadList += " ";
                    if (!string.IsNullOrEmpty(id))
                        mostReadList += id + "^" + i;
                    i--;
                }

                return mostReadList;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }

        }
        public string GetMostShared(int total)
        {
            try
            {
                string MostSharedList = string.Empty;
                var dateFrom = DateTime.Now.AddDays(-1);
                var coll = _database.GetCollection<BsonDocument>("assetsharecounts");

                var filter = Builders<BsonDocument>.Filter.Gt("pubdate", dateFrom);
                var sort = Builders<BsonDocument>.Sort.Descending("total");

                var i = total;
                var cursor = coll.Find(filter).Sort(sort).Limit(total).ToCursor();
                foreach (var document in cursor.ToEnumerable())
                {
                    if (!string.IsNullOrEmpty(MostSharedList))
                        MostSharedList += " ";
                    MostSharedList += document.GetValue("assetid") + "^" + i;
                    i--;
                }

                return MostSharedList;
            }
            catch
            {
                return string.Empty;
            }

        }
        private dynamic ParseValidResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response)) return null;
            var result = JsonConvert.DeserializeObject<dynamic>(response);
            if (result == null || result.response == null || result.response.docs == null) return null;
            return result.response.docs.Count <= 0 ? null : result.response.docs[0];
        }

        private BsonDocument CreateBsonDocument(string name, string value)
            => new BsonDocument { { name, value } };

    }
}
