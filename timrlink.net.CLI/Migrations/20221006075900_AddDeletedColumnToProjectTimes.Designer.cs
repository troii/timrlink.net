﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using timrlink.net.CLI;

namespace timrlink.net.CLI.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221006075900_AddDeletedColumnToProjectTimes")]
    partial class AddDeletedColumnToProjectTimes
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.29")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("timrlink.net.CLI.Metadata", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key");

                    b.ToTable("Metadata");
                });

            modelBuilder.Entity("timrlink.net.CLI.ProjectTime", b =>
                {
                    b.Property<Guid>("UUID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Billable")
                        .HasColumnType("bit");

                    b.Property<int>("BreakTime")
                        .HasColumnType("int");

                    b.Property<bool>("Changed")
                        .HasColumnType("bit");

                    b.Property<bool>("Closed")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("Deleted")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Duration")
                        .HasColumnType("bigint");

                    b.Property<string>("EndPosition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("LastModifiedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("StartPosition")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Task")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("User")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UUID");

                    b.ToTable("ProjectTimes");
                });
#pragma warning restore 612, 618
        }
    }
}