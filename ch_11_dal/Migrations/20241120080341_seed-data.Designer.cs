﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Repositories;

#nullable disable

namespace ch_11_dal.Migrations
{
    [DbContext(typeof(RepositoryContext))]
    [Migration("20241120080341_seed-data")]
    partial class seeddata
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Price")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasMaxLength(25)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Books");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Price = 20.00m,
                            Title = "Devlet"
                        },
                        new
                        {
                            Id = 2,
                            Price = 15.50m,
                            Title = "Ateşten Gömlek"
                        },
                        new
                        {
                            Id = 3,
                            Price = 18.75m,
                            Title = "Huzur"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
