using Microsoft.EntityFrameworkCore.Migrations;

namespace Ray.Model.Migrations
{
    public partial class StoreProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var GetTreeNodeStructure_Result = @"DROP PROCEDURE [dbo].[GetTreeNodeStructure]
				GO
				CREATE PROCEDURE [dbo].[GetTreeNodeStructure]
				AS
				BEGIN

					;WITH NodeFull AS (
					SELECT	n.[Id], 
							n.[Description],
							n.[Order],
							n.[ParentNodeId],
							n.[IsPublished],
							n.[IsEnabled],
							n.[IsDeleted],
							n.[IsPrint],
							n.[NewSourceId],
							n.[CreationUser],
							n.[CreationDate],
							n.[LastModificationUser],
							n.[LastModificationDate],
							n.[SeoTitle],
							n.[SeoDescription],
							n.[Keywords],
							n.[OGTitle],
							n.[OGDescription],
							n.[SeoImage],
							nc.[Title],
							nc.[LanguageId],
							n.[IsDiagrammable]
					FROM [Node]	n WITH(NOLOCK)
					JOIN NodeContent nc WITH(NOLOCK) ON n.[Id] = nc.[NodeId]
					WHERE n.[IsDeleted] = 0
				),
				Recursives AS (
							SELECT n.[Id], 
								   n.[Description],
								   n.[Order],
								   n.[ParentNodeId],
								   n.[IsPublished],
								   n.[IsEnabled],
								   n.[IsDeleted],
								   n.[IsPrint],
								   n.NewSourceId,
								   n.[CreationUser],
								   n.[CreationDate],
								   n.[LastModificationUser],
								   n.[LastModificationDate],
								   n.[SeoTitle],
								   n.[SeoDescription],
								   n.[Keywords],
								   n.[OGTitle],
								   n.[OGDescription],
								   n.[SeoImage],
								   n.[Title],
								   n.[LanguageId],
								   n.[IsDiagrammable],
								   (select stuff((select '; ' + Name
									   FROM Keyword ky WITH(NOLOCK)
									   JOIN NodeKeyword nk WITH(NOLOCK) on n.Id = nk.NodeId
									   WHERE ky.Id = nk.KeywordId
									   for xml path ('')
								   ), 1, 2, '')) RelatedKeywords,
								   1 AS [Level]
							FROM NodeFull n
							WHERE n.[ParentNodeId] IS NULL

							UNION ALL

							SELECT n.[Id], 
								   n.[Description],
								   n.[Order],
								   n.[ParentNodeId],
								   n.[IsPublished],
								   n.[IsEnabled],
								   n.[IsDeleted],
								   n.[IsPrint],
								   n.NewSourceId,
								   n.[CreationUser],
								   n.[CreationDate],
								   n.[LastModificationUser],
								   n.[LastModificationDate],
								   n.[SeoTitle],
								   n.[SeoDescription],
								   n.[Keywords],
								   n.[OGTitle],
								   n.[OGDescription],
								   n.[SeoImage],
								   n.[Title],
								   n.[LanguageId],
								   n.[IsDiagrammable],
									(select stuff((select '; ' + Name
									   FROM Keyword ky WITH(NOLOCK)
									   JOIN NodeKeyword nk WITH(NOLOCK) on n.Id = nk.NodeId
									   WHERE ky.Id = nk.KeywordId
									   for xml path ('')
								   ), 1, 2, '')) RelatedKeywords,
								   (r.[Level] + 1) AS [Level]
							FROM NodeFull n
							JOIN Recursives r ON n.[ParentNodeId] = r.[Id]
					)

					SELECT * FROM Recursives WITH(NOLOCK)
					ORDER BY [Level], [Order]

				END
				GO
				";

            migrationBuilder.Sql(GetTreeNodeStructure_Result);

			var GetTreeCategoryStructure_Result = @"DROP PROCEDURE [dbo].[GetTreeCategoryStructure]
						GO

						/****** Object:  StoredProcedure [dbo].[GetTreeCategoryStructure]    Script Date: 6/1/2022 12:52:12 PM ******/
						SET ANSI_NULLS ON
						GO

						SET QUOTED_IDENTIFIER ON
						GO


						CREATE PROCEDURE [dbo].[GetTreeCategoryStructure]
						AS
						BEGIN

							;WITH Recursives AS (
									SELECT -- Category --
										   c.[Id], 
										   c.[Name],
										   c.[IsEnabled],
										   c.[ParentCategoryId],
										   c.[CreationUser],
										   c.[CreationDate],
										   c.[LastModificationUser],
										   c.[LastModificationDate],

										   -- Node --
										   --n.[Id], 
										   --n.[Description],
										   --n.[Order],
										   --n.[ParentNodeId],
										   --n.[HasCustomLayout],
										   --n.[IsEnabled],
										   --n.[IsDeleted],
										   --n.[CreationUser],
										   --n.[CreationDate],
										   --n.[LastModificationUser],
										   --n.[LastModificationDate],
										   --nc.[Title],
										   --nc.[LanguageId],

										   1 AS [Level]
									FROM [Category] c WITH(NOLOCK)
									--JOIN CategoryNode cn ON c.[Id] = cn.[CategoryId]
									--JOIN Node n ON cn.[NodeId] = n.[Id] --AND n.[IsDeleted] = 0
									--JOIN NodeContent nc ON n.[Id] = nc.[NodeId]
									WHERE c.[ParentCategoryId] IS NULL
									AND c.[IsEnabled] = 1

									UNION ALL

									SELECT -- Category --
										   c.[Id], 
										   c.[Name],
										   c.[IsEnabled],
										   c.[ParentCategoryId],
										   c.[CreationUser],
										   c.[CreationDate],
										   c.[LastModificationUser],
										   c.[LastModificationDate],

										   -- Node --
										   --n.[Id], 
										   --n.[Description],
										   --n.[Order],
										   --n.[ParentNodeId],
										   --n.[HasCustomLayout],
										   --n.[IsEnabled],
										   --n.[IsDeleted],
										   --n.[CreationUser],
										   --n.[CreationDate],
										   --n.[LastModificationUser],
										   --n.[LastModificationDate],
										   --nc.[Title],
										   --nc.[LanguageId],

										   ((r.[level]) + 1) AS [Level]
									FROM [Category] c WITH(NOLOCK)
									JOIN Recursives r ON c.[ParentCategoryId] = r.[Id]
									--JOIN CategoryNode cn ON c.[Id] = cn.[CategoryId]
									--JOIN Node n ON cn.[NodeId] = n.[Id] AND n.[IsDeleted] = 0
									--JOIN NodeContent nc ON n.[Id] = nc.[NodeId]
									WHERE c.[IsEnabled] = 1
							)

							SELECT * FROM Recursives 
							ORDER BY [Level]
						END
						GO
						";

			migrationBuilder.Sql(GetTreeCategoryStructure_Result);


			var GetTreeMicrosite_Result = @"

					ALTER PROCEDURE [dbo].[GetTreeMicrosite]

					AS
					SET FMTONLY OFF
					IF OBJECT_ID('tempdb..#tmp_arbol') IS NOT NULL DROP TABLE #tmp_arbol
					IF OBJECT_ID('tempdb..#tmp_arbol2') IS NOT NULL DROP TABLE #tmp_arbol2
					IF OBJECT_ID('tempdb..#tmp_arbol') IS NOT NULL DROP TABLE #tmp_microsite
					IF OBJECT_ID('tempdb..#tmp_arbol2') IS NOT NULL DROP TABLE #tmp_microsite_final

					CREATE TABLE #tmp_arbol2(
						Id int,
						NodeDescription VARCHAR(MAX),
						CreationUser VARCHAR(MAX),
						CreationDate  VARCHAR(MAX)  ,
						LastModificationDate VARCHAR(MAX)  ,
						Node_en VARCHAR(MAX)  ,
						Node_es VARCHAR(MAX) ,
						IsDeleted bit  ,
						IsMicrosite bit ,
						ParentNodeId int  ,
						LevelNode int,
						IsDiagrammable bit,
						Sort VARCHAR(MAX)
					)

					--get all microsite (parent without microsite)
								select n.*
								into #tmp_microsites
								from node n
								where IsEnabled = 1 


								SELECT t1.*
								INTO #tmp_microsite_final
								FROM #tmp_microsites t1
								LEFT JOIN #tmp_microsites t2 ON t2.id = t1.Parentnodeid
								WHERE t2.id IS NULL

					BEGIN
			

					WITH q AS 
							(
								SELECT  
									m.*, 
									ROW_NUMBER() OVER (ORDER BY m.Id) AS LevelNode,
									CAST(ROW_NUMBER() OVER (ORDER BY m.Id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN AS Sort
								FROM    #tmp_microsite_final m
			
								UNION ALL
			
								SELECT  
									m.*,
									q.LevelNode + 1,
									q.Sort + '.' + CAST(ROW_NUMBER() OVER (PARTITION BY m.parentNodeId ORDER BY m.Id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN
								FROM    Node m
								JOIN    q
								ON      m.parentNodeID = q.ID
							)

		


					SELECT * into #tmp_arbol FROM q 

					--SELECT * from q

					INSERT INTO #tmp_arbol2
								SELECT d.Id,
									   d.[Description] AS NodeDescription,
									   d.CreationUser,
									   d.CreationDate,
									   d.LastModificationDate,
									   b.Title AS Node_en,
									   c.Title AS Node_es,
									   e.IsDeleted,
									   d.IsEnabled AS IsMicrosite,
									   d.ParentNodeId,
									   tmp.LevelNode,
									   d.[IsDiagrammable],
									   tmp.Sort
								FROM Node d WITH(NOLOCK)
								LEFT JOIN [User] U WITH(NOLOCK) ON d.CreationUser = U.UserName
								LEFT JOIN NodeContent b WITH(NOLOCK) ON d.Id = b.NodeId AND b.LanguageId = 1 -- EN
								LEFT JOIN NodeContent c WITH(NOLOCK) ON d.Id = c.NodeId AND c.LanguageId = 2 -- ES
								JOIN Asset e WITH(NOLOCK) ON d.Id = e.Id
								JOIN #tmp_arbol tmp ON tmp.Id = d.Id
					ORDER BY tmp.LevelNode

					select Id,
						NodeDescription,
						CreationUser,
						CreationDate,
						LastModificationDate,
						Node_en,
						Node_es,
						IsDeleted,
						IsMicrosite,
						IsDiagrammable,
						ParentNodeId,
						LevelNode,
						Sort from #tmp_arbol2
						Order by Sort


						end
						";

			migrationBuilder.Sql(GetTreeMicrosite_Result);

			var GetTreeLayoutStructureById_Result = @"
						DROP PROCEDURE [dbo].[GetTreeLayoutStructureById]
						GO

						/****** Object:  StoredProcedure [dbo].[GetTreeLayoutStructureById]    Script Date: 6/1/2022 1:13:01 PM ******/
						SET ANSI_NULLS ON
						GO

						SET QUOTED_IDENTIFIER ON
						GO



						CREATE PROCEDURE [dbo].[GetTreeLayoutStructureById]
						(
							@nodeid	int
						)
						AS

						SET FMTONLY OFF
						IF OBJECT_ID('tempdb..#tmp_arbol') IS NOT NULL DROP TABLE #tmp_arbol
						IF OBJECT_ID('tempdb..#tmp_arbol2') IS NOT NULL DROP TABLE #tmp_arbol2
						CREATE TABLE #tmp_arbol2(
							Id int,
							NodeId   int,
							NodeDescription VARCHAR(MAX),
							CreationUser VARCHAR(MAX),
							CreationDate  VARCHAR(MAX)  ,
							LastModificationDate VARCHAR(MAX)  ,
							PublicationDate VARCHAR(MAX)  ,
							Node_en VARCHAR(MAX)  ,
							Node_es VARCHAR(MAX) ,
							IsDeleted bit  ,
							NroRow int  ,
							IsMicrosite bit ,
							ParentNodeId int ,
							HasParentMicrosite bit ,
							LevelNode int,
							IsDiagrammable bit,
							Sort VARCHAR(MAX)
						)

						BEGIN


						WITH q AS 
								(
									SELECT  
										m.*, 
										ROW_NUMBER() OVER (ORDER BY m.Id) AS LevelNode,
										CAST(ROW_NUMBER() OVER (ORDER BY m.Id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN AS Sort
									FROM    Node m
									WHERE   Id = @nodeId
			
									UNION ALL
			
									SELECT  
										m.*,
										q.LevelNode + 1,
										q.Sort + '.' + CAST(ROW_NUMBER() OVER (PARTITION BY m.parentNodeId ORDER BY m.Id) AS VARCHAR(MAX)) COLLATE Latin1_General_BIN
									FROM    Node m
									JOIN    q
									ON      m.parentNodeID = q.ID
								)

		

						SELECT * into #tmp_arbol FROM q 

						INSERT INTO #tmp_arbol2 SELECT TBL.Id,
										   TBL.NodeId,
										   d.[Description] AS NodeDescription,
										   TBL.UserInfo AS CreationUser,
										   TBL.CreationDate,
										   TBL.LastModificationDate,
										   TBL.PublicationDate,
										   b.Title AS Node_en,
										   c.Title AS Node_es,
										   e.IsDeleted,
										   TBL.NroRow,
										   d.IsEnabled AS IsMicrosite,
										   d.ParentNodeId,
										   (
										   case when (select COUNT(li.Id) From node li where li.Id =  d.ParentNodeId and IsEnabled = 0) > 0 then
													CAST(1 as bit)
												else
													CAST(0 as bit)
											end
										   ) as HasParentMicrosite,
										   tmp.LevelNode,
										   d.[IsDiagrammable],
										   tmp.Sort
									FROM (
									SELECT	LI.Id,
											LI.NodeId,
											LI.[Description],
											(U.FirstName + ' ' + U.LastName) AS UserInfo,
											LI.CreationDate,
											LI.LastModificationDate,
											LI.PublicationDate,
											ROW_NUMBER() OVER(PARTITION BY LI.NodeId ORDER BY LI.CreationDate DESC) AS NroRow

									FROM LayoutInstance LI WITH(NOLOCK)
									LEFT JOIN [User] U WITH(NOLOCK) ON LI.CreationUser = U.UserName
								) TBL 

									LEFT JOIN NodeContent b WITH(NOLOCK) ON TBL.NodeId = b.NodeId AND b.LanguageId = 1 -- EN
									LEFT JOIN NodeContent c WITH(NOLOCK) ON TBL.NodeId = c.NodeId AND c.LanguageId = 2 -- ES
									JOIN Node d WITH(NOLOCK) ON TBL.NodeId = d.Id
									JOIN Asset e WITH(NOLOCK) ON TBL.NodeId = e.Id
									JOIN #tmp_arbol tmp ON tmp.Id = d.Id
									WHERE TBL.NroRow = 1
			
						ORDER BY tmp.LevelNode

						select Id,
							NodeId,
							NodeDescription,
							CreationUser,
							CreationDate,
							LastModificationDate,
							PublicationDate,
							Node_en,
							Node_es,
							IsDeleted,
							NroRow,
							IsMicrosite,
							ParentNodeId,
							HasParentMicrosite,
							IsDiagrammable,
							LevelNode,
							Sort from #tmp_arbol2
							Order by Sort


							end
						GO

						";

			migrationBuilder.Sql(GetTreeLayoutStructureById_Result);
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
