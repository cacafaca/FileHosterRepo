﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProCode.FileHosterRepo.Dal.DataAccess;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    [DbContext(typeof(FileHosterContext))]
    partial class FileHosterContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.Media", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Medias");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaLink", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasMaxLength(2000)
                        .HasColumnType("varchar(2000)");

                    b.Property<int>("LinkOrder")
                        .HasColumnType("int");

                    b.Property<int?>("MediaVersionId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MediaVersionId");

                    b.ToTable("MediaLinks");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaVersion", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<int?>("MediaId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("MediaId");

                    b.HasIndex("UserId");

                    b.ToTable("MediaVersions");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("Nickname")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.UserToken", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.HasKey("UserId");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.Media", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaLink", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.MediaVersion", "MediaVersion")
                        .WithMany()
                        .HasForeignKey("MediaVersionId");

                    b.Navigation("MediaVersion");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaVersion", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.Media", "Media")
                        .WithMany()
                        .HasForeignKey("MediaId");

                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Media");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.UserToken", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
