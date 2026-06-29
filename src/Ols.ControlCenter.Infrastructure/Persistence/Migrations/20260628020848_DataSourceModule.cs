using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ols.ControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DataSourceModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "file_name",
                table: "data_sync_logs",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sheet_name",
                table: "data_sync_logs",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "last_sync_error",
                table: "data_sources",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "access_type",
                table: "data_sources",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_success_sync_at",
                table: "data_sources",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "url",
                table: "data_sources",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "default_value",
                table: "data_source_column_mappings",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "source_column_index",
                table: "data_source_column_mappings",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "transform_type",
                table: "data_source_column_mappings",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "imported_raw_rows",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    data_sync_log_id = table.Column<long>(type: "bigint", nullable: true),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_imported = table.Column<bool>(type: "boolean", nullable: false),
                    error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_imported_raw_rows", x => x.id);
                    table.ForeignKey(
                        name: "fk_imported_raw_rows_data_sources_data_source_id",
                        column: x => x.data_source_id,
                        principalTable: "data_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_imported_raw_rows_data_source_id_data_sync_log_id",
                table: "imported_raw_rows",
                columns: new[] { "data_source_id", "data_sync_log_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imported_raw_rows");

            migrationBuilder.DropColumn(
                name: "file_name",
                table: "data_sync_logs");

            migrationBuilder.DropColumn(
                name: "sheet_name",
                table: "data_sync_logs");

            migrationBuilder.DropColumn(
                name: "access_type",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "last_success_sync_at",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "url",
                table: "data_sources");

            migrationBuilder.DropColumn(
                name: "default_value",
                table: "data_source_column_mappings");

            migrationBuilder.DropColumn(
                name: "source_column_index",
                table: "data_source_column_mappings");

            migrationBuilder.DropColumn(
                name: "transform_type",
                table: "data_source_column_mappings");

            migrationBuilder.AlterColumn<string>(
                name: "last_sync_error",
                table: "data_sources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
