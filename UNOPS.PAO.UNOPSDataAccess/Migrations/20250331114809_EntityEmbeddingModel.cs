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
            migrationBuilder.CreateTable(
                name: "EntityEmbeddings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    FullEmbedding = table.Column<byte[]>(type: "bytea", nullable: false),
                    NameEmbedding = table.Column<byte[]>(type: "bytea", nullable: true)
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

            // NOTE: pgvector stored procedures temporarily disabled for databases without pgvector extension.
            // These will be re-enabled when pgvector is installed on the Cloud SQL instance.
            // Original procedures used embedding::vector(768) casts which require pgvector.
            migrationBuilder.Sql(@"CREATE OR REPLACE PROCEDURE public.""InsertEntityEmbedding""(entityName TEXT, entityId INT, embedding TEXT)
                                LANGUAGE plpgsql
                                AS $$
                                BEGIN
                                INSERT INTO public.""EntityEmbeddings"" (""EntityName"", ""EntityId"", ""FullEmbedding"") 
                                VALUES (entityName, entityId, decode(embedding, 'hex')) 
                                ON CONFLICT (""EntityName"", ""EntityId"") 
                                DO UPDATE SET ""FullEmbedding"" = EXCLUDED.""FullEmbedding""; 
                                END;
                                $$;");

            // Similarity search function (stub without pgvector - returns 0)
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.RetrieveSimilarityId(entityName TEXT, embedding TEXT)
                    RETURNS INT LANGUAGE plpgsql AS $BODY$ DECLARE
                        entityId INT = 0;
                    BEGIN
                        -- Stub: pgvector not available, return 0
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
