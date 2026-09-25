using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Rino.Repositories.Core;

namespace Rino.Repositories
{
    public class CacheInvalidationQueueItem
    {
        public long Id { get; set; }
        public string Kind { get; set; }
        public int? NewsId { get; set; }
        public int? NodeId { get; set; }
        public string PreviousJson { get; set; }
        public string PathsJson { get; set; }
        public string Reason { get; set; }
        public int Attempts { get; set; }
    }

    public interface ICacheInvalidationQueueRepository
    {
        void EnsureSchema();
        void Enqueue(CacheInvalidationQueueItem item);
        Task<IList<CacheInvalidationQueueItem>> Claim(int max, string owner);
        Task MarkDone(long id);
        Task MarkRetry(long id, string error, DateTime nextAttemptUtc);
        Task MarkDeadLetter(long id, string error);
        Task<int> ReleaseStuck(TimeSpan olderThan);
        Task<int> PurgeCompleted(TimeSpan olderThan);
    }

    public class CacheInvalidationQueueRepository : ICacheInvalidationQueueRepository
    {
        public const string TableName = "CacheInvalidationQueue";

        private readonly IUnitOfWork _unitOfWork;

        public CacheInvalidationQueueRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void EnsureSchema()
        {
            const string sql = @"
IF OBJECT_ID('dbo.CacheInvalidationQueue', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CacheInvalidationQueue (
        Id              BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CacheInvalidationQueue PRIMARY KEY,
        Kind            VARCHAR(20)    NOT NULL,
        NewsId          INT            NULL,
        NodeId          INT            NULL,
        PreviousJson    NVARCHAR(MAX)  NULL,
        PathsJson       NVARCHAR(MAX)  NULL,
        Reason          NVARCHAR(400)  NULL,
        Status          VARCHAR(20)    NOT NULL CONSTRAINT DF_CacheInvalidationQueue_Status DEFAULT ('Pending'),
        Attempts        INT            NOT NULL CONSTRAINT DF_CacheInvalidationQueue_Attempts DEFAULT (0),
        LastError       NVARCHAR(MAX)  NULL,
        CreationDate    DATETIME2(3)   NOT NULL CONSTRAINT DF_CacheInvalidationQueue_Creation DEFAULT (SYSUTCDATETIME()),
        NextAttemptDate DATETIME2(3)   NOT NULL CONSTRAINT DF_CacheInvalidationQueue_NextAttempt DEFAULT (SYSUTCDATETIME()),
        ProcessedDate   DATETIME2(3)   NULL,
        LockedBy        NVARCHAR(128)  NULL,
        LockedAt        DATETIME2(3)   NULL
    );

    CREATE INDEX IX_CacheInvalidationQueue_Pending
        ON dbo.CacheInvalidationQueue (Status, NextAttemptDate) INCLUDE (Kind);

    CREATE INDEX IX_CacheInvalidationQueue_Stuck
        ON dbo.CacheInvalidationQueue (Status, LockedAt);
END";

            _unitOfWork.Context.Database.ExecuteSqlRaw(sql);
        }

        public void Enqueue(CacheInvalidationQueueItem item)
        {
            const string sql = @"
INSERT INTO dbo.CacheInvalidationQueue (Kind, NewsId, NodeId, PreviousJson, PathsJson, Reason, Status, Attempts, CreationDate, NextAttemptDate)
VALUES (@Kind, @NewsId, @NodeId, @PreviousJson, @PathsJson, @Reason, 'Pending', 0, SYSUTCDATETIME(), SYSUTCDATETIME())";

            _unitOfWork.Context.Database.ExecuteSqlRaw(sql,
                Param("@Kind", item.Kind),
                Param("@NewsId", item.NewsId),
                Param("@NodeId", item.NodeId),
                Param("@PreviousJson", item.PreviousJson),
                Param("@PathsJson", item.PathsJson),
                Param("@Reason", item.Reason));
        }

        public async Task<IList<CacheInvalidationQueueItem>> Claim(int max, string owner)
        {
            const string sql = @"
UPDATE TOP (@Max) q
SET q.Status = 'Processing',
    q.LockedBy = @Owner,
    q.LockedAt = SYSUTCDATETIME(),
    q.Attempts = q.Attempts + 1
OUTPUT INSERTED.Id, INSERTED.Kind, INSERTED.NewsId, INSERTED.NodeId,
       INSERTED.PreviousJson, INSERTED.PathsJson, INSERTED.Reason, INSERTED.Attempts
FROM dbo.CacheInvalidationQueue AS q WITH (READPAST, UPDLOCK, ROWLOCK)
WHERE q.Status = 'Pending' AND q.NextAttemptDate <= SYSUTCDATETIME()";

            return await _unitOfWork.Context.Database
                .SqlQueryRaw<CacheInvalidationQueueItem>(sql, Param("@Max", max), Param("@Owner", owner))
                .ToListAsync();
        }

        public async Task MarkDone(long id)
        {
            const string sql = @"
UPDATE dbo.CacheInvalidationQueue
SET Status = 'Done', ProcessedDate = SYSUTCDATETIME(), LockedBy = NULL, LockedAt = NULL, LastError = NULL
WHERE Id = @Id";

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(sql, Param("@Id", id));
        }

        public async Task MarkRetry(long id, string error, DateTime nextAttemptUtc)
        {
            const string sql = @"
UPDATE dbo.CacheInvalidationQueue
SET Status = 'Pending', NextAttemptDate = @NextAttempt, LastError = @Error, LockedBy = NULL, LockedAt = NULL
WHERE Id = @Id";

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(sql,
                Param("@Id", id),
                Param("@NextAttempt", nextAttemptUtc),
                Param("@Error", Truncate(error, 4000)));
        }

        public async Task MarkDeadLetter(long id, string error)
        {
            const string sql = @"
UPDATE dbo.CacheInvalidationQueue
SET Status = 'Failed', ProcessedDate = SYSUTCDATETIME(), LastError = @Error, LockedBy = NULL, LockedAt = NULL
WHERE Id = @Id";

            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(sql,
                Param("@Id", id),
                Param("@Error", Truncate(error, 4000)));
        }

        public async Task<int> ReleaseStuck(TimeSpan olderThan)
        {
            const string sql = @"
UPDATE dbo.CacheInvalidationQueue
SET Status = 'Pending', LockedBy = NULL, LockedAt = NULL
WHERE Status = 'Processing' AND LockedAt IS NOT NULL AND LockedAt < @Threshold";

            return await _unitOfWork.Context.Database.ExecuteSqlRawAsync(sql,
                Param("@Threshold", DateTime.UtcNow.Subtract(olderThan)));
        }

        public async Task<int> PurgeCompleted(TimeSpan olderThan)
        {
            const string sql = @"
DELETE FROM dbo.CacheInvalidationQueue
WHERE Status = 'Done' AND ProcessedDate IS NOT NULL AND ProcessedDate < @Threshold";

            return await _unitOfWork.Context.Database.ExecuteSqlRawAsync(sql,
                Param("@Threshold", DateTime.UtcNow.Subtract(olderThan)));
        }

        private static SqlParameter Param(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        private static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max)
                return value;

            return value.Substring(0, max);
        }
    }
}
