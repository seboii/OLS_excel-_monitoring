using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ols.ControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingBoards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "target_board",
                table: "data_sources",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "road_archive_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    departure_date = table.Column<string>(type: "text", nullable: true),
                    import_country = table.Column<string>(type: "text", nullable: true),
                    sender = table.Column<string>(type: "text", nullable: true),
                    receiver = table.Column<string>(type: "text", nullable: true),
                    plate = table.Column<string>(type: "text", nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: true),
                    package_count = table.Column<string>(type: "text", nullable: true),
                    weight = table.Column<string>(type: "text", nullable: true),
                    stackable = table.Column<string>(type: "text", nullable: true),
                    order_date = table.Column<string>(type: "text", nullable: true),
                    arrival_warehouse = table.Column<string>(type: "text", nullable: true),
                    purchase_freight = table.Column<string>(type: "text", nullable: true),
                    ydg_included = table.Column<string>(type: "text", nullable: true),
                    supplier = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_archive_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_load_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_rep = table.Column<string>(type: "text", nullable: true),
                    departure_date = table.Column<string>(type: "text", nullable: true),
                    vehicle_location = table.Column<string>(type: "text", nullable: true),
                    import_country = table.Column<string>(type: "text", nullable: true),
                    sender = table.Column<string>(type: "text", nullable: true),
                    receiver = table.Column<string>(type: "text", nullable: true),
                    plate = table.Column<string>(type: "text", nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: true),
                    package_count = table.Column<string>(type: "text", nullable: true),
                    weight = table.Column<string>(type: "text", nullable: true),
                    stackable = table.Column<string>(type: "text", nullable: true),
                    arrival_warehouse = table.Column<string>(type: "text", nullable: true),
                    freight = table.Column<string>(type: "text", nullable: true),
                    ydg = table.Column<string>(type: "text", nullable: true),
                    supplier = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_load_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "road_transit_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    date = table.Column<DateOnly>(type: "date", nullable: true),
                    shipper = table.Column<string>(type: "text", nullable: true),
                    consignee = table.Column<string>(type: "text", nullable: true),
                    origin_country = table.Column<string>(type: "text", nullable: true),
                    plate = table.Column<string>(type: "text", nullable: true),
                    term = table.Column<string>(type: "text", nullable: true),
                    line = table.Column<string>(type: "text", nullable: true),
                    booking = table.Column<string>(type: "text", nullable: true),
                    container_no = table.Column<string>(type: "text", nullable: true),
                    empty_container_transfer = table.Column<string>(type: "text", nullable: true),
                    pol = table.Column<string>(type: "text", nullable: true),
                    pod = table.Column<string>(type: "text", nullable: true),
                    eta = table.Column<DateOnly>(type: "date", nullable: true),
                    cut_off = table.Column<DateOnly>(type: "date", nullable: true),
                    invoice = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    note2 = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_road_transit_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sea_export_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipper = table.Column<string>(type: "text", nullable: true),
                    consignee = table.Column<string>(type: "text", nullable: true),
                    line = table.Column<string>(type: "text", nullable: true),
                    term = table.Column<string>(type: "text", nullable: true),
                    container_kind = table.Column<string>(type: "text", nullable: true),
                    booking = table.Column<string>(type: "text", nullable: true),
                    container_no = table.Column<string>(type: "text", nullable: true),
                    pol = table.Column<string>(type: "text", nullable: true),
                    pod = table.Column<string>(type: "text", nullable: true),
                    cut_off = table.Column<DateOnly>(type: "date", nullable: true),
                    etd = table.Column<DateOnly>(type: "date", nullable: true),
                    eta = table.Column<DateOnly>(type: "date", nullable: true),
                    invoice = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    note2 = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sea_export_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sea_import_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipper = table.Column<string>(type: "text", nullable: true),
                    consignee = table.Column<string>(type: "text", nullable: true),
                    line = table.Column<string>(type: "text", nullable: true),
                    term = table.Column<string>(type: "text", nullable: true),
                    agent = table.Column<string>(type: "text", nullable: true),
                    agent_ref = table.Column<string>(type: "text", nullable: true),
                    container_kind = table.Column<string>(type: "text", nullable: true),
                    booking = table.Column<string>(type: "text", nullable: true),
                    container_no = table.Column<string>(type: "text", nullable: true),
                    pol = table.Column<string>(type: "text", nullable: true),
                    pod = table.Column<string>(type: "text", nullable: true),
                    etd = table.Column<DateOnly>(type: "date", nullable: true),
                    eta = table.Column<DateOnly>(type: "date", nullable: true),
                    invoice = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    note2 = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sea_import_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sea_transit_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shipper = table.Column<string>(type: "text", nullable: true),
                    consignee = table.Column<string>(type: "text", nullable: true),
                    line = table.Column<string>(type: "text", nullable: true),
                    term = table.Column<string>(type: "text", nullable: true),
                    agent = table.Column<string>(type: "text", nullable: true),
                    agent_ref = table.Column<string>(type: "text", nullable: true),
                    container_kind = table.Column<string>(type: "text", nullable: true),
                    import_booking = table.Column<string>(type: "text", nullable: true),
                    container_no = table.Column<string>(type: "text", nullable: true),
                    incoming_container = table.Column<string>(type: "text", nullable: true),
                    pol = table.Column<string>(type: "text", nullable: true),
                    pod = table.Column<string>(type: "text", nullable: true),
                    import_etd = table.Column<DateOnly>(type: "date", nullable: true),
                    import_eta = table.Column<DateOnly>(type: "date", nullable: true),
                    transfer_point = table.Column<string>(type: "text", nullable: true),
                    export_booking = table.Column<string>(type: "text", nullable: true),
                    empty_container_transfer = table.Column<string>(type: "text", nullable: true),
                    cut_off = table.Column<DateOnly>(type: "date", nullable: true),
                    export_etd = table.Column<DateOnly>(type: "date", nullable: true),
                    export_eta = table.Column<DateOnly>(type: "date", nullable: true),
                    invoice = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    note2 = table.Column<string>(type: "text", nullable: true),
                    data_source_id = table.Column<long>(type: "bigint", nullable: false),
                    source_row_key = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    row_index = table.Column<int>(type: "integer", nullable: false),
                    status_text = table.Column<string>(type: "text", nullable: true),
                    risk_level = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    delay_days = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    raw_json = table.Column<string>(type: "jsonb", nullable: false),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sea_transit_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_road_archive_records_data_source_id",
                table: "road_archive_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_road_archive_records_data_source_id_source_row_key",
                table: "road_archive_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_road_load_records_data_source_id",
                table: "road_load_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_road_load_records_data_source_id_source_row_key",
                table: "road_load_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_road_transit_records_data_source_id",
                table: "road_transit_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_road_transit_records_data_source_id_source_row_key",
                table: "road_transit_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_sea_export_records_data_source_id",
                table: "sea_export_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_sea_export_records_data_source_id_source_row_key",
                table: "sea_export_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_sea_import_records_data_source_id",
                table: "sea_import_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_sea_import_records_data_source_id_source_row_key",
                table: "sea_import_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_sea_transit_records_data_source_id",
                table: "sea_transit_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_sea_transit_records_data_source_id_source_row_key",
                table: "sea_transit_records",
                columns: new[] { "data_source_id", "source_row_key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "road_archive_records");

            migrationBuilder.DropTable(
                name: "road_load_records");

            migrationBuilder.DropTable(
                name: "road_transit_records");

            migrationBuilder.DropTable(
                name: "sea_export_records");

            migrationBuilder.DropTable(
                name: "sea_import_records");

            migrationBuilder.DropTable(
                name: "sea_transit_records");

            migrationBuilder.DropColumn(
                name: "target_board",
                table: "data_sources");
        }
    }
}
