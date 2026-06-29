using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ols.ControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class BoardComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "operation_id",
                table: "comments",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<string>(
                name: "board_key",
                table: "comments",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "board_title",
                table: "comments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "group",
                table: "comments",
                type: "character varying(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "record_ref",
                table: "comments",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_comments_board_key_record_ref_created_at",
                table: "comments",
                columns: new[] { "board_key", "record_ref", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_comments_board_key_record_ref_created_at",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "board_key",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "board_title",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "group",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "record_ref",
                table: "comments");

            migrationBuilder.AlterColumn<long>(
                name: "operation_id",
                table: "comments",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
