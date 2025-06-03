using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UNOPS.PAO.UNOPSDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixPartnerTreeDiscriminators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix invalid discriminator values in PartnerTrees table
            // Update records with discriminator 's' to 'PartnerTree'
            migrationBuilder.Sql(@"
                UPDATE public.""PartnerTrees"" 
                SET ""Discriminator"" = 'PartnerTree' 
                WHERE ""Discriminator"" = 's';
            ");

            // Update any other invalid discriminator values to default base class
            migrationBuilder.Sql(@"
                UPDATE public.""PartnerTrees"" 
                SET ""Discriminator"" = 'PartnerTree' 
                WHERE ""Discriminator"" NOT IN ('PartnerTree', 'UNOPSPartnerTree');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Note: We cannot safely rollback discriminator value changes
            // as we don't know what the original invalid values were
            // This migration only fixes data, so no rollback is needed
        }
    }
}
