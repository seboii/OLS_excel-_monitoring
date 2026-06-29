using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ols.ControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlaboraAirBoards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "air_daily_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: true),
                    mawb_no = table.Column<string>(type: "text", nullable: true),
                    hawb_no = table.Column<string>(type: "text", nullable: true),
                    airport = table.Column<string>(type: "text", nullable: true),
                    destination = table.Column<string>(type: "text", nullable: true),
                    piece_count = table.Column<string>(type: "text", nullable: true),
                    kgs = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    incoterm = table.Column<string>(type: "text", nullable: true),
                    sender = table.Column<string>(type: "text", nullable: true),
                    flight = table.Column<string>(type: "text", nullable: true),
                    warehouse_entry = table.Column<DateOnly>(type: "date", nullable: true),
                    option_date = table.Column<DateOnly>(type: "date", nullable: true),
                    option_time = table.Column<string>(type: "text", nullable: true),
                    airline = table.Column<string>(type: "text", nullable: true),
                    warehouse = table.Column<string>(type: "text", nullable: true),
                    reference_number = table.Column<string>(type: "text", nullable: true),
                    carrier = table.Column<string>(type: "text", nullable: true),
                    flag = table.Column<string>(type: "text", nullable: true),
                    warehouse_code = table.Column<string>(type: "text", nullable: true),
                    authorized = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    shipment_number = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_air_daily_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "air_operation_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sender = table.Column<string>(type: "text", nullable: true),
                    flight = table.Column<string>(type: "text", nullable: true),
                    warehouse_entry = table.Column<DateOnly>(type: "date", nullable: true),
                    option_date = table.Column<DateOnly>(type: "date", nullable: true),
                    option_time = table.Column<string>(type: "text", nullable: true),
                    airline = table.Column<string>(type: "text", nullable: true),
                    warehouse = table.Column<string>(type: "text", nullable: true),
                    reference_number = table.Column<string>(type: "text", nullable: true),
                    sn = table.Column<string>(type: "text", nullable: true),
                    archive = table.Column<string>(type: "text", nullable: true),
                    col_a = table.Column<string>(type: "text", nullable: true),
                    col_s = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_air_operation_records", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alabora_finance_records",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    no = table.Column<string>(type: "text", nullable: true),
                    ft_date = table.Column<DateOnly>(type: "date", nullable: true),
                    company_title = table.Column<string>(type: "text", nullable: true),
                    voyage = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<string>(type: "text", nullable: true),
                    currency = table.Column<string>(type: "text", nullable: true),
                    cargo_status = table.Column<string>(type: "text", nullable: true),
                    docs_readiness = table.Column<string>(type: "text", nullable: true),
                    invoice_marked = table.Column<string>(type: "text", nullable: true),
                    transport_docs = table.Column<string>(type: "text", nullable: true),
                    order_contract = table.Column<string>(type: "text", nullable: true),
                    comment_osh = table.Column<string>(type: "text", nullable: true),
                    comment_ols = table.Column<string>(type: "text", nullable: true),
                    incoming_payments = table.Column<string>(type: "text", nullable: true),
                    collection = table.Column<string>(type: "text", nullable: true),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: true),
                    rub = table.Column<string>(type: "text", nullable: true),
                    loading_type = table.Column<string>(type: "text", nullable: true),
                    loading_details = table.Column<string>(type: "text", nullable: true),
                    customer_rep = table.Column<string>(type: "text", nullable: true),
                    rate_net = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("pk_alabora_finance_records", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_air_daily_records_data_source_id",
                table: "air_daily_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_air_daily_records_data_source_id_source_row_key",
                table: "air_daily_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_air_operation_records_data_source_id",
                table: "air_operation_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_air_operation_records_data_source_id_source_row_key",
                table: "air_operation_records",
                columns: new[] { "data_source_id", "source_row_key" });

            migrationBuilder.CreateIndex(
                name: "ix_alabora_finance_records_data_source_id",
                table: "alabora_finance_records",
                column: "data_source_id");

            migrationBuilder.CreateIndex(
                name: "ix_alabora_finance_records_data_source_id_source_row_key",
                table: "alabora_finance_records",
                columns: new[] { "data_source_id", "source_row_key" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "air_daily_records");

            migrationBuilder.DropTable(
                name: "air_operation_records");

            migrationBuilder.DropTable(
                name: "alabora_finance_records");
        }
    }
}
