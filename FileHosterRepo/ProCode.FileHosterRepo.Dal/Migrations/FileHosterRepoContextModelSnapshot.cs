﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ProCode.FileHosterRepo.Dal.DataAccess;

namespace ProCode.FileHosterRepo.Dal.Migrations
{
    [DbContext(typeof(FileHosterRepoContext))]
    partial class FileHosterRepoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.Media", b =>
                {
                    b.Property<int>("MediaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("ReferenceLink")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("MediaId");

                    b.HasIndex("UserId");

                    b.ToTable("Medias");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaLink", b =>
                {
                    b.Property<int>("MediaLinkId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("LinkOrderId")
                        .HasColumnType("int");

                    b.Property<int>("MediaVersionId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("VersionComment")
                        .HasColumnType("text");

                    b.HasKey("MediaLinkId");

                    b.HasIndex("UserId");

                    b.HasIndex("MediaVersionId", "LinkOrderId")
                        .IsUnique();

                    b.ToTable("MediaLinks");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaPart", b =>
                {
                    b.Property<int>("MediaPartId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<int>("Episode")
                        .HasColumnType("int");

                    b.Property<int>("MediaId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("ReferenceLink")
                        .HasColumnType("text");

                    b.Property<int>("Season")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("MediaPartId");

                    b.HasIndex("UserId");

                    b.HasIndex("MediaId", "Season", "Episode")
                        .IsUnique();

                    b.ToTable("MediaParts");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaVersion", b =>
                {
                    b.Property<int>("MediaVersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime");

                    b.Property<int>("MediaPartId")
                        .HasColumnType("int");

                    b.Property<string>("Tags")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("VersionComment")
                        .HasColumnType("text");

                    b.HasKey("MediaVersionId");

                    b.HasIndex("MediaPartId");

                    b.HasIndex("UserId");

                    b.ToTable("MediaVersions");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<bool>("Logged")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Nickname")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Nickname")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.Media", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaLink", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.MediaVersion", "Version")
                        .WithMany()
                        .HasForeignKey("MediaVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");

                    b.Navigation("Version");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaPart", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.Media", "Media")
                        .WithMany()
                        .HasForeignKey("MediaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Media");

                    b.Navigation("User");
                });

            modelBuilder.Entity("ProCode.FileHosterRepo.Dal.Model.MediaVersion", b =>
                {
                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.MediaPart", "MediaPart")
                        .WithMany()
                        .HasForeignKey("MediaPartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ProCode.FileHosterRepo.Dal.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MediaPart");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
