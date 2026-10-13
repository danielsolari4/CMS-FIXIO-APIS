using System.Collections.Concurrent;
using System.Globalization;
using CMS.NewsVersion.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace CMS.NewsVersion.Services
{
    public interface IViewCountService
    {
        Task ViewCount(int id, int nodeId, DateTime publicationDate, string slug, int? authorId = null);
    }

    public class ViewCountService : IViewCountService
    {
        private static IMongoClient? _client;
        private static IMongoDatabase? _database;
        private static readonly ConcurrentDictionary<string, bool> EnsuredCollections = new();

        private readonly MongoDbSettings _mongoDb;
        private readonly ContentSettings _content;
        private readonly ILogger<ViewCountService> _logger;
        private const string AssetViewCountCollection = "assetviewscounts";
        private const string DefaultNewsViewStatsCollection = "newsviewstats";

        public ViewCountService(IOptions<MongoDbSettings> mongoDb, IOptions<ContentSettings> content, ILogger<ViewCountService> logger)
        {
            _mongoDb = mongoDb.Value;
            _content = content.Value;
            _logger = logger;
        }

        public async Task ViewCount(int id, int nodeId, DateTime publicationDate, string slug, int? authorId = null)
        {
            try
            {
                if (!HasMongoConfiguration())
                    return;

                var database = GetMongoDatabase();
                if (publicationDate == default)
                    publicationDate = DateTime.UtcNow;

                var collectionName = string.IsNullOrWhiteSpace(_mongoDb.NameCollectionAssetsViewCount)
                    ? AssetViewCountCollection
                    : _mongoDb.NameCollectionAssetsViewCount;

                var exists = await CollectionExistsAsync(collectionName);
                if (!exists)
                {
                    await database.CreateCollectionAsync(collectionName);
                    var created = database.GetCollection<BsonDocument>(collectionName);
                    await created.Indexes.CreateManyAsync(new[]
                    {
                        new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("assetid")),
                        new CreateIndexModel<BsonDocument>(Builders<BsonDocument>.IndexKeys.Ascending("slug")),
                        new CreateIndexModel<BsonDocument>(
                            Builders<BsonDocument>.IndexKeys.Ascending("expireAt"),
                            new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(1) }),
                    });
                }

                var document = new BsonDocument
                {
                    { "assetid", id },
                    { "nodeId", nodeId },
                    { "pubdate", publicationDate },
                    { "expireAt", DateTime.Now },
                    { "slug", slug.ToLowerInvariant() }
                };

                var collection = database.GetCollection<BsonDocument>(collectionName);
                await collection.InsertOneAsync(document);
                await UpdateNewsViewStats(id, nodeId, publicationDate, slug, authorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewCount failed for news {NewsId}", id);
            }
        }

        private async Task UpdateNewsViewStats(int id, int nodeId, DateTime publicationDate, string slug, int? authorId)
        {
            if (!HasMongoConfiguration())
                return;

            var database = GetMongoDatabase();
            var collectionName = string.IsNullOrWhiteSpace(_mongoDb.NameCollectionNewsViewStats)
                ? DefaultNewsViewStatsCollection
                : _mongoDb.NameCollectionNewsViewStats;
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
                {
                    "todayViews", new BsonDocument("$cond", new BsonArray
                    {
                        new BsonDocument("$eq", new BsonArray { "$todayDate", todayKey }),
                        new BsonDocument("$add", new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$todayViews", 0 }), 1 }),
                        1
                    })
                },
                { "currentMonth", monthKey },
                {
                    "monthViews", new BsonDocument("$cond", new BsonArray
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

            if (EnsuredCollections.ContainsKey(collectionName))
                return;

            var exists = await CollectionExistsAsync(collectionName);
            if (!exists)
                await database.CreateCollectionAsync(collectionName);

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
            EnsuredCollections.TryAdd(collectionName, true);
        }

        private async Task<bool> CollectionExistsAsync(string collectionName)
        {
            if (!HasMongoConfiguration())
                return false;

            var database = GetMongoDatabase();
            var filter = new BsonDocument("name", collectionName);
            var collections = await database.ListCollectionsAsync(new ListCollectionsOptions { Filter = filter });
            return await collections.AnyAsync();
        }

        private bool HasMongoConfiguration()
            => !string.IsNullOrWhiteSpace(_mongoDb.ConnectionString) &&
               !string.IsNullOrWhiteSpace(_mongoDb.DatabaseName);

        private IMongoDatabase GetMongoDatabase()
        {
            if (_database != null)
                return _database;

            if (!HasMongoConfiguration())
                throw new InvalidOperationException("MongoDb settings are required to use view counts.");

            _client = new MongoClient(_mongoDb.ConnectionString);
            _database = _client.GetDatabase(_mongoDb.DatabaseName);
            return _database;
        }

        private DateTime GetCurrentContentDateTime()
        {
            var timeZoneId = _content.TimeZone;
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
    }
}
