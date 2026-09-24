using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers_web.Migrations
{
    /// <inheritdoc />
    public partial class AddDonationCertificates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CertificateIssuedAtUtc",
                table: "Donations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CertificateNumber",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CertificateIssuedAtUtc",
                table: "Donations");

            migrationBuilder.DropColumn(
                name: "CertificateNumber",
                table: "Donations");
        }
    }
}
