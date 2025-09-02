using System;
using BitsBlog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BitsBlog.Infrastructure.Migrations
{
    [DbContext(typeof(BitsBlogDbContext))]
    partial class BitsBlogDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("BitsBlog.Domain.Entities.Post", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int")
                    .HasAnnotation("SqlServer:Identity", "1, 1");

                b.Property<string>("Content")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("Created")
                    .HasColumnType("datetime2");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.ToTable("Posts");
            });
        }
    }
}

