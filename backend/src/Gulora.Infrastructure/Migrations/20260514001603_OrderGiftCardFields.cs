using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Gulora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrderGiftCardFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "gift_card_message",
                table: "orders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gift_card_occasion",
                table: "orders",
                type: "character varying(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "gift_card_sender_name",
                table: "orders",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gift_card_message",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "gift_card_occasion",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "gift_card_sender_name",
                table: "orders");
        }
    }
}
