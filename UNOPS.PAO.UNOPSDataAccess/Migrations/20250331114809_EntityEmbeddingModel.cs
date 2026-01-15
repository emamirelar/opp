using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EntityEmbeddingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS vector;");
            migrationBuilder.CreateTable(
                name: "EntityEmbeddings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    FullEmbedding = table.Column<byte[]>(type: "vector(768)", nullable: false),
                    NameEmbedding = table.Column<byte[]>(type: "vector(768)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityEmbeddings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityId",
                schema: "public",
                table: "EntityEmbeddings",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityName",
                schema: "public",
                table: "EntityEmbeddings",
                column: "EntityName");

            migrationBuilder.CreateIndex(
                name: "IX_EntityEmbeddings_EntityName_EntityId",
                schema: "public",
                table: "EntityEmbeddings",
                columns: new[] { "EntityName", "EntityId" },
                unique: true);

            migrationBuilder.Sql(@"CREATE OR REPLACE PROCEDURE public.""InsertEntityEmbedding""(entityName TEXT, entityId INT, embedding TEXT)
                                LANGUAGE plpgsql
                                AS $$
                                BEGIN
                                INSERT INTO public.""EntityEmbeddings"" (""EntityName"", ""EntityId"", ""FullEmbedding"") 
                                VALUES (entityName, entityId, embedding::vector(768)) 
                                ON CONFLICT (""EntityName"", ""EntityId"") 
                                DO UPDATE SET ""FullEmbedding"" = EXCLUDED.""FullEmbedding""; 
                                END;
                                $$;");

            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.RetrieveSimilarityId(entityName TEXT, embedding TEXT)
                    RETURNS INT LANGUAGE plpgsql AS $BODY$ DECLARE
                        entityId INT = 0;      -- Stores the best matching entity
                    BEGIN
                        -- Find the closest entity match using cosine similarity
                        SELECT ""EntityId"" as entityId
                        INTO entityId
                        FROM public.""EntityEmbeddings""
                        WHERE ""EntityName"" = entityName
                        --AND ""FullEmbedding"" <-> embedding::vector(768) < 0.5
                        ORDER BY (""FullEmbedding"" <=> embedding::vector(768))  -- <=> is the cosine distance operator in pgvector
                        LIMIT 1;
                        RETURN entityId;
                    END
                    $BODY$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityEmbeddings",
                schema: "public");
        }
    }
}
