﻿// <auto-generated />
using System;
using ComplexPrototypeSystem.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ComplexPrototypeSystem.Server.Migrations.SensorReportDb
{
    [DbContext(typeof(SensorReportDbContext))]
    [Migration("20221020084916_SensorReport")]
    partial class SensorReport
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ComplexPrototypeSystem.Shared.SensorReport", b =>
                {
                    b.Property<Guid>("ReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("SensorGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("TemperatureF")
                        .HasColumnType("int");

                    b.Property<int>("Usage")
                        .HasColumnType("int");

                    b.HasKey("ReportId");

                    b.ToTable("SensorReports");

                    b.HasData(
                        new
                        {
                            ReportId = new Guid("cbf2d3ee-33d4-472e-a46b-7034329bcf45"),
                            DateTime = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            SensorGuid = new Guid("00000000-0000-0000-0000-000000000000"),
                            TemperatureF = 0,
                            Usage = 0
                        });
                });
#pragma warning restore 612, 618
        }
    }
}