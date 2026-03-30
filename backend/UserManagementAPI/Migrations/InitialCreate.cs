using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserManagementAPI.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Applications",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false, defaultValue: "")
            },
            constraints: table => { table.PrimaryKey("PK_Applications", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Username = table.Column<string>(nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table => { table.PrimaryKey("PK_Users", x => x.Id); });

        migrationBuilder.CreateTable(
            name: "UserApplications",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<int>(nullable: false),
                ApplicationId = table.Column<int>(nullable: false),
                AssignedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "NOW()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserApplications", x => x.Id);
                table.ForeignKey("FK_UserApplications_Users", x => x.UserId, "Users", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_UserApplications_Applications", x => x.ApplicationId, "Applications", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_Users_Username", "Users", "Username", unique: true);
        migrationBuilder.CreateIndex("IX_UserApplications_UserId_ApplicationId", "UserApplications", new[] { "UserId", "ApplicationId" }, unique: true);

        // Seed applications
        migrationBuilder.InsertData("Applications", new[] { "Id", "Name", "Description" },
            new object[] { 1, "HR", "Human Resources application" });
        migrationBuilder.InsertData("Applications", new[] { "Id", "Name", "Description" },
            new object[] { 2, "Accounting", "Accounting and finance application" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("UserApplications");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("Applications");
    }
}
