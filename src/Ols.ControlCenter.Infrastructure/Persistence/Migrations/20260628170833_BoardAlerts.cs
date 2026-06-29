using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ols.ControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BoardAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "operation_id",
                table: "alerts",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "board_key",
                table: "alerts",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "board_title",
                table: "alerts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group",
                table: "alerts",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "record_ref",
                table: "alerts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_alerts_board_key",
                table: "alerts",
                column: "board_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alerts_board_key",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "board_key",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "board_title",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "group",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "record_ref",
                table: "alerts");

            migrationBuilder.AlterColumn<long>(
                name: "operation_id",
                table: "alerts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
