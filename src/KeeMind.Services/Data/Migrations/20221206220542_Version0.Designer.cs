﻿// <auto-generated />
using KeeMind.Services.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace KeeMind.Services.Data.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20221206220542_Version0")]
    partial class Version0
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("KeeMind.Services.Data.Attachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContentType")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .IsRequired()
                        .HasColumnType("BLOB");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("Attachments");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Card", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Item", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFavorite")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.ToTable("Items");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("KeeMind.Services.Data.TagEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CardId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CardId");

                    b.HasIndex("TagId");

                    b.ToTable("TagEntries");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Attachment", b =>
                {
                    b.HasOne("KeeMind.Services.Data.Card", "Card")
                        .WithMany("Attachments")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Item", b =>
                {
                    b.HasOne("KeeMind.Services.Data.Card", "Card")
                        .WithMany("Items")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("KeeMind.Services.Data.TagEntry", b =>
                {
                    b.HasOne("KeeMind.Services.Data.Card", "Card")
                        .WithMany("Tags")
                        .HasForeignKey("CardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("KeeMind.Services.Data.Tag", "Tag")
                        .WithMany("Entries")
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");

                    b.Navigation("Tag");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Card", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Items");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("KeeMind.Services.Data.Tag", b =>
                {
                    b.Navigation("Entries");
                });
#pragma warning restore 612, 618
        }
    }
}
