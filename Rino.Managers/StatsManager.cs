using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rino.Dtos;
using Rino.Dtos.Configuration;
using Rino.Dtos.Mongo;
using Rino.Utils.Solr;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        Task ViewCount(int Id, int nodeId, DateTime publicationDate, string slug, int? authorId = null);
        Task<NewsViewStatsDashboardDto> GetNewsViewStatsDashboard(string creationUser, int rows = 8, int maxNews = 500);
        Task ShareCount(int Id, DateTime PublicationDate);
        Task<string> GetMostReadOldAsync(int total, int nodeId);
        public string GetMostRead(int total);
        public string GetMostShared(int total);
    }

    public class StatsManager : IStatsManager
    {
        protected static IMongoClient _client;
        protected static IMongoDatabase _database;
        private static readonly ConcurrentDictionary<string, bool> _ensuredCollections = new ConcurrentDictionary<string, bool>();
        private readonly AppSettings _appSettings;
        private const string _nameCollectionAssetViewCount = "assetviewscounts";
        private const string _defaultNewsViewStatsCollection = "newsviewstats";

        public StatsManager(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }


        public async Task ViewCountOld(int Id, int nodeId, DateTime PublicationDate)
        {
            try
            {
                var database = GetMongoDatabase();
                //View Count
                var filter = Builders<BsonDocument>.Filter.Eq("assetid", Id);
                var update = new BsonDocument("$inc", new BsonDocument { { "total", 1 } });
                var coll = database.GetCollection<BsonDocument>("assetviewscounts");
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

                    var collection = database.GetCollection<BsonDocument>("assetviewscounts");
                    await collection.InsertOneAsync(document);
                }

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task ViewCount(int Id, int nodeId, DateTime publicationDate, string slug, int? authorId = null)
        {
            try
            {
                var database = GetMongoDatabase();
                //View Count
                if (publicationDate == null)
                    publicationDate = DateTime.UtcNow;

                var exist = await CollectionExistsAsync(_nameCollectionAssetViewCount);
                if (!exist)
                {
                    ////102400
                    //var options = new CreateCollectionOptions { Capped = true, MaxSize = 400, };
                    await database.CreateCollectionAsync(_appSettings.MongoDb.NameCollectionAssetsViewCount);
                    await database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("assetid"));
                    await database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("slug"));
                    await database.GetCollection<BsonDocument>(_appSettings.MongoDb.NameCollectionAssetsViewCount).Indexes.CreateOneAsync(Builders<BsonDocument>.IndexKeys.Ascending("expireAt"), new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(1) });

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

                var collection = database.GetCollection<BsonDocument>(_nameCollectionAssetViewCount);
                await collection.InsertOneAsync(document);
                await UpdateNewsViewStats(Id, nodeId, publicationDate, slug, authorId);

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
                var database = GetMongoDatabase();
                //Share Count
                var filter = Builders<BsonDocument>.Filter.Eq("assetid", Id);
                var update = new BsonDocument("$inc", new BsonDocument { { "total", 1 } });
                var coll = database.GetCollection<BsonDocument>("assetsharecounts");
                var doc = coll.FindOneAndUpdateAsync(filter, update).Result;

                if (doc == null)
                {
                    var document = new BsonDocument
                        {
                            { "assetid", Id },
                            { "total", 1 },
                            { "pubdate", PublicationDate }
                        };

                    var collection = database.GetCollection<BsonDocument>("assetsharecounts");
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
            if (!HasMongoConfiguration())
                return false;

            var database = GetMongoDatabase();
            var filter = new BsonDocument("name", collectionName);
            //filter by collection name
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            //check for existence
            return await collections.AnyAsync();
        }

        public async Task<NewsViewStatsDashboardDto> GetNewsViewStatsDashboard(string creationUser, int rows = 8, int maxNews = 500)
        {
            var dashboard = new NewsViewStatsDashboardDto();
            var notes = await GetDashboardNewsFromSolr(creationUser, maxNews);

            if (!notes.Any())
                return dashboard;

            var statsByNewsId = await GetNewsViewStats(notes.Select(x => x.NewsId).ToList());

            foreach (var note in notes)
            {
                NewsViewStatsDashboardItemDto stats;
                if (statsByNewsId.TryGetValue(note.NewsId, out stats))
                {
                    note.TotalViews = stats.TotalViews;
                    note.TodayViews = IsCurrentToday(stats) ? stats.TodayViews : 0;
                    note.MonthViews = IsCurrentMonth(stats) ? stats.MonthViews : 0;
                    note.LastViewAt = stats.LastViewAt;
                    note.HasStats = true;
                }
            }

            dashboard.RecentNotes = notes.Take(rows).ToList();
            dashboard.TopToday = notes.Where(x => x.TodayViews > 0).OrderByDescending(x => x.TodayViews).ThenByDescending(x => x.PublicationDate).Take(rows).ToList();
            dashboard.TopMonth = notes.Where(x => x.MonthViews > 0).OrderByDescending(x => x.MonthViews).ThenByDescending(x => x.PublicationDate).Take(rows).ToList();
            dashboard.Highlight = dashboard.TopToday.FirstOrDefault() ?? dashboard.TopMonth.FirstOrDefault() ?? notes.Where(x => x.TotalViews > 0).OrderByDescending(x => x.TotalViews).FirstOrDefault();

            dashboard.Summary = new NewsViewStatsSummaryDto
            {
                NewsWithStats = notes.Count(x => x.HasStats),
                NewsWithoutStats = notes.Count(x => !x.HasStats),
                TotalViews = notes.Sum(x => x.TotalViews),
                TodayViews = notes.Sum(x => x.TodayViews),
                MonthViews = notes.Sum(x => x.MonthViews)
            };

            return dashboard;
        }

        private async Task UpdateNewsViewStats(int id, int nodeId, DateTime publicationDate, string slug, int? authorId)
        {
            if (!HasMongoConfiguration())
                return;

            var database = GetMongoDatabase();
            var collectionName = GetNewsViewStatsCollectionName();
            await EnsureNewsViewStatsCollection(collectionName);

            var currentDate = GetCurrentContentDateTime();
            var todayKey = currentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var monthKey = currentDate.ToString("yyyy-MM", CultureInfo.InvariantCulture);
            var nowUtc = DateTime.UtcNow;

            var setFields = new BsonDocument
            {
                { "newsId", id },
                { "assetid", id },
                { "nodeId", nodeId },
                { "slug", slug?.ToLowerInvariant() ?? string.Empty },
                { "publicationDate", publicationDate },
                { "firstRegistrationDate", new BsonDocument("$ifNull", new BsonArray { "$firstRegistrationDate", nowUtc }) },
                { "lastViewAt", nowUtc },
                { "totalViews", new BsonDocument("$add", new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$totalViews", 0 }), 1 }) },
                { "todayDate", todayKey },
                { "todayViews", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$todayDate", todayKey }),
                        new BsonDocument("$add", new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$todayViews", 0 }), 1 }),
                        1
                    })
                },
                { "currentMonth", monthKey },
                { "monthViews", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$currentMonth", monthKey }),
                        new BsonDocument("$add", new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$monthViews", 0 }), 1 }),
                        1
                    })
                }
            };

            if (authorId.HasValue)
                setFields.Add("authorId", authorId.Value);

            var filter = Builders<BsonDocument>.Filter.Eq("newsId", id);
            var pipeline = new EmptyPipelineDefinition<BsonDocument>()
                .AppendStage<BsonDocument, BsonDocument, BsonDocument>(new BsonDocument("$set", setFields));
            var update = Builders<BsonDocument>.Update.Pipeline(pipeline);
            var options = new UpdateOptions { IsUpsert = true };

            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.UpdateOneAsync(filter, update, options);
        }

        private async Task EnsureNewsViewStatsCollection(string collectionName)
        {
            var database = GetMongoDatabase();

            if (_ensuredCollections.ContainsKey(collectionName))
                return;

            var exists = await CollectionExistsAsync(collectionName);
            if (!exists)
            {
                await database.CreateCollectionAsync(collectionName);
            }

            var collection = database.GetCollection<BsonDocument>(collectionName);
            var indexModels = new[]
            {
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("newsId"),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Descending("totalViews")),
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("todayDate").Descending("todayViews")),
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("currentMonth").Descending("monthViews")),
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending("authorId"))
            };

            await collection.Indexes.CreateManyAsync(indexModels);
            _ensuredCollections.TryAdd(collectionName, true);
        }

        private string GetNewsViewStatsCollectionName()
            => string.IsNullOrWhiteSpace(_appSettings.MongoDb.NameCollectionNewsViewStats)
                ? _defaultNewsViewStatsCollection
                : _appSettings.MongoDb.NameCollectionNewsViewStats;

        private bool HasMongoConfiguration()
            => _appSettings?.MongoDb != null &&
               !string.IsNullOrWhiteSpace(_appSettings.MongoDb.ConnectionString) &&
               !string.IsNullOrWhiteSpace(_appSettings.MongoDb.DatabaseName);

        private IMongoDatabase GetMongoDatabase()
        {
            if (_database != null)
                return _database;

            if (!HasMongoConfiguration())
                throw new InvalidOperationException("MongoDb settings are required to use stats.");

            _client = new MongoClient(_appSettings.MongoDb.ConnectionString);
            _database = _client.GetDatabase(_appSettings.MongoDb.DatabaseName);
            return _database;
        }

        private DateTime GetCurrentContentDateTime()
        {
            var timeZoneId = _appSettings.Content?.TimeZone;
            if (string.IsNullOrWhiteSpace(timeZoneId))
                return DateTime.Now;

            try
            {
                var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            }
            catch
            {
                return DateTime.Now;
            }
        }

        private async Task<List<NewsViewStatsDashboardItemDto>> GetDashboardNewsFromSolr(string creationUser, int maxNews)
        {
            if (string.IsNullOrWhiteSpace(creationUser))
                return new List<NewsViewStatsDashboardItemDto>();

            var query = "q=IsDeleted:false AND CreationUser:\"" + EscapeSolrValue(creationUser) + "\"" +
                        "&rows=" + Math.Max(1, maxNews) +
                        "&fl=Id,Title_en,Url,Nodes_en,Status,PublicationDate,CreationDate" +
                        "&sort=LastModificationDate desc";

            var response = await SolrHelper.ExecuteQuery(SolrCore.NEWS, HttpUtility.UrlDecode(query), _appSettings.Solr);
            if (response == null || string.IsNullOrWhiteSpace(response.ToString()))
                return new List<NewsViewStatsDashboardItemDto>();

            var result = JsonConvert.DeserializeObject<JObject>(response.ToString());
            var docs = result?["response"]?["docs"] as JArray;
            if (docs == null)
                return new List<NewsViewStatsDashboardItemDto>();

            return docs
                .Select(ParseDashboardNews)
                .Where(x => x != null && x.NewsId > 0)
                .ToList();
        }

        private async Task<Dictionary<int, NewsViewStatsDashboardItemDto>> GetNewsViewStats(List<int> newsIds)
        {
            if (!HasMongoConfiguration())
                return new Dictionary<int, NewsViewStatsDashboardItemDto>();

            var collectionName = GetNewsViewStatsCollectionName();
            var exists = await CollectionExistsAsync(collectionName);
            if (!exists || newsIds == null || newsIds.Count == 0)
                return new Dictionary<int, NewsViewStatsDashboardItemDto>();

            var collection = GetMongoDatabase().GetCollection<BsonDocument>(collectionName);
            var filter = Builders<BsonDocument>.Filter.In("newsId", newsIds);
            var documents = await collection.Find(filter).ToListAsync();

            return documents
                .Select(ParseNewsViewStats)
                .Where(x => x != null && x.NewsId > 0)
                .GroupBy(x => x.NewsId)
                .ToDictionary(x => x.Key, x => x.First());
        }

        private NewsViewStatsDashboardItemDto ParseDashboardNews(JToken item)
        {
            return new NewsViewStatsDashboardItemDto
            {
                NewsId = GetTokenValue<int>(item, "Id"),
                Title = GetTokenValue<string>(item, "Title_en") ?? "Nota sin titulo",
                Url = GetTokenValue<string>(item, "Url"),
                Section = GetFirstArrayValue(item, "Nodes_en") ?? "Sin seccion",
                Status = GetTokenValue<string>(item, "Status"),
                PublicationDate = GetTokenDate(item, "PublicationDate") ?? GetTokenDate(item, "CreationDate")
            };
        }

        private NewsViewStatsDashboardItemDto ParseNewsViewStats(BsonDocument document)
        {
            if (document == null)
                return null;

            return new NewsViewStatsDashboardItemDto
            {
                NewsId = GetBsonInt(document, "newsId"),
                TotalViews = GetBsonLong(document, "totalViews"),
                TodayViews = GetBsonLong(document, "todayViews"),
                MonthViews = GetBsonLong(document, "monthViews"),
                LastViewAt = GetBsonDate(document, "lastViewAt"),
                TodayDate = GetBsonString(document, "todayDate"),
                CurrentMonth = GetBsonString(document, "currentMonth"),
                HasStats = true,
                Status = GetBsonString(document, "status")
            };
        }

        private bool IsCurrentToday(NewsViewStatsDashboardItemDto stats)
        {
            if (stats == null)
                return false;

            var currentDate = GetCurrentContentDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            return currentDate == stats.TodayDate;
        }

        private bool IsCurrentMonth(NewsViewStatsDashboardItemDto stats)
        {
            if (stats == null)
                return false;

            var currentMonth = GetCurrentContentDateTime().ToString("yyyy-MM", CultureInfo.InvariantCulture);
            return currentMonth == stats.CurrentMonth;
        }

        private string EscapeSolrValue(string value)
            => value?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? string.Empty;

        private T GetTokenValue<T>(JToken item, string propertyName)
        {
            var token = item?[propertyName];
            if (token == null || token.Type == JTokenType.Null)
                return default(T);

            if (token is JArray array && array.Count > 0)
                return array[0].ToObject<T>();

            return token.ToObject<T>();
        }

        private DateTime? GetTokenDate(JToken item, string propertyName)
        {
            var value = GetTokenValue<string>(item, propertyName);
            DateTime date;
            return DateTime.TryParse(value, out date) ? date : (DateTime?)null;
        }

        private string GetFirstArrayValue(JToken item, string propertyName)
        {
            var token = item?[propertyName];
            if (token is JArray array && array.Count > 0)
                return array[0]?.ToString();

            return token?.ToString();
        }

        private int GetBsonInt(BsonDocument document, string key)
            => document.Contains(key) && IsBsonNumeric(document[key]) ? document[key].ToInt32() : 0;

        private long GetBsonLong(BsonDocument document, string key)
            => document.Contains(key) && IsBsonNumeric(document[key]) ? document[key].ToInt64() : 0;

        private string GetBsonString(BsonDocument document, string key)
            => document.Contains(key) && !document[key].IsBsonNull ? document[key].ToString() : null;

        private DateTime? GetBsonDate(BsonDocument document, string key)
            => document.Contains(key) && document[key].BsonType == BsonType.DateTime ? document[key].ToUniversalTime() : (DateTime?)null;

        private bool IsBsonNumeric(BsonValue value)
            => value != null &&
                (value.BsonType == BsonType.Int32 ||
                 value.BsonType == BsonType.Int64 ||
                 value.BsonType == BsonType.Double ||
                 value.BsonType == BsonType.Decimal128);

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


                var coll = GetMongoDatabase().GetCollection<RankingDocumentDto>("assetviewscounts");

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
                var coll = GetMongoDatabase().GetCollection<BsonDocument>("assetsharecounts");

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
