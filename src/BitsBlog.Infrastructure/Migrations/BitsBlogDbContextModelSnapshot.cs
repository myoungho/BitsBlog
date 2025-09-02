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

            modelBuilder.Entity("BitsBlog.Domain.Entities.Comment", b =>
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

                b.Property<int>("PostId")
                    .HasColumnType("int");

                b.HasKey("Id");

                b.HasIndex("PostId");

                b.ToTable("Comments");
            });

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

            modelBuilder.Entity("BitsBlog.Domain.Entities.Comment", b =>
            {
                b.HasOne("BitsBlog.Domain.Entities.Post", "Post")
                    .WithMany("Comments")
                    .HasForeignKey("PostId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Post");
            });

            modelBuilder.Entity("BitsBlog.Domain.Entities.Post", b =>
            {
                b.Navigation("Comments");
            });
        }
    }
}

