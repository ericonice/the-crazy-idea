using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Earthquakes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "earthquakes");

            migrationBuilder.DropTable(name: "ephemeris_entries");

            migrationBuilder.DropTable(name: "sun_spots");
        }

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "earthquakes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTimeOffset>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    depth = table.Column<string>(type: "text", nullable: true),
                    magnitude = table.Column<decimal>(type: "numeric", nullable: false),
                    magnitude_type = table.Column<string>(type: "text", nullable: false),
                    magnitude_error = table.Column<decimal>(type: "numeric", nullable: true),
                    magnitude_source = table.Column<string>(type: "text", nullable: false),
                    place = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_earthquakes", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "ephemeris_entries",
                columns: table => new
                {
                    center_body = table.Column<int>(type: "integer", nullable: false),
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    target_body = table.Column<int>(type: "integer", nullable: false),
                    minimum = table.Column<bool>(type: "boolean", nullable: false),
                    offside_minimum = table.Column<bool>(type: "boolean", nullable: false),
                    onside_minimum = table.Column<bool>(type: "boolean", nullable: false),
                    sot_angle = table.Column<decimal>(type: "numeric", nullable: false),
                    sot_minimum = table.Column<bool>(type: "boolean", nullable: false),
                    sto_angle = table.Column<decimal>(type: "numeric", nullable: false),
                    sto_minimum = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "pk_ephemeris_entries",
                        x => new
                        {
                            x.day,
                            x.center_body,
                            x.target_body
                        }
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "sun_spots",
                columns: table => new
                {
                    day = table.Column<DateOnly>(type: "date", nullable: false),
                    number_of_sun_spots = table.Column<int>(type: "integer", nullable: false),
                    number_of_observations = table.Column<int>(type: "integer", nullable: false),
                    standard_deviation = table.Column<decimal>(type: "numeric", nullable: false),
                    provisional = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sun_spots", x => x.day);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ephemeris_entries_center_body_target_body_minimum",
                table: "ephemeris_entries",
                columns: new[] { "center_body", "target_body", "minimum" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ephemeris_entries_center_body_target_body_offside_minimum",
                table: "ephemeris_entries",
                columns: new[] { "center_body", "target_body", "offside_minimum" }
            );

            migrationBuilder.CreateIndex(
                name: "ix_ephemeris_entries_center_body_target_body_onside_minimum",
                table: "ephemeris_entries",
                columns: new[] { "center_body", "target_body", "onside_minimum" }
            );
        }
    }
}
