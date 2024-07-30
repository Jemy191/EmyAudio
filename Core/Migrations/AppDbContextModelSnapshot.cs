﻿// <auto-generated />
using System;
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Core.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("AudioInfoToTagJoin", b =>
                {
                    b.Property<string>("AudioInfoId")
                        .HasColumnType("TEXT");

                    b.Property<string>("TagName")
                        .HasColumnType("TEXT");

                    b.HasKey("AudioInfoId", "TagName");

                    b.HasIndex("TagName");

                    b.ToTable("AudioInfoToTagJoin");
                });

            modelBuilder.Entity("Core.Models.AudioInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("LastPlayed")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Next")
                        .HasColumnType("TEXT");

                    b.Property<string>("Previous")
                        .HasColumnType("TEXT");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<uint>("TimePlayed")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("AudioInfos");
                });

            modelBuilder.Entity("Core.Models.AudioSkip", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AudioInfoId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("End")
                        .HasColumnType("INTEGER");

                    b.Property<long>("Start")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AudioInfoId");

                    b.ToTable("AudioSkips");
                });

            modelBuilder.Entity("Core.Models.Tag", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Downloaded")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Hidden")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Next")
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalPlaylistId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Parent")
                        .HasColumnType("TEXT");

                    b.Property<string>("Previous")
                        .HasColumnType("TEXT");

                    b.Property<int>("Score")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Name");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("AudioInfoToTagJoin", b =>
                {
                    b.HasOne("Core.Models.AudioInfo", null)
                        .WithMany()
                        .HasForeignKey("AudioInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Core.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Core.Models.AudioSkip", b =>
                {
                    b.HasOne("Core.Models.AudioInfo", "AudioInfo")
                        .WithMany("Skips")
                        .HasForeignKey("AudioInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AudioInfo");
                });

            modelBuilder.Entity("Core.Models.AudioInfo", b =>
                {
                    b.Navigation("Skips");
                });
#pragma warning restore 612, 618
        }
    }
}
