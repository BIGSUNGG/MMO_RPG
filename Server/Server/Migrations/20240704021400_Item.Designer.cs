﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;

namespace Server.Migrations
{
    [DbContext(typeof(GameDbContext))]
    [Migration("20240704021400_Item")]
    partial class Item
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Server.DB.GameAccountDb", b =>
                {
                    b.Property<int>("GameAccountDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.HasKey("GameAccountDbId");

                    b.HasIndex("AccountDbId")
                        .IsUnique();

                    b.HasIndex("GameAccountDbId")
                        .IsUnique();

                    b.ToTable("GameAccount");
                });

            modelBuilder.Entity("Server.DB.ItemInfoDb", b =>
                {
                    b.Property<int>("ItemInfoDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("Count")
                        .HasColumnType("int");

                    b.Property<int>("PlayerDbId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("ItemInfoDbId");

                    b.HasIndex("ItemInfoDbId")
                        .IsUnique();

                    b.HasIndex("PlayerDbId");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.Property<int>("PlayerDbId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("AccountDbId")
                        .HasColumnType("int");

                    b.Property<int>("Hp")
                        .HasColumnType("int");

                    b.Property<int>("MapId")
                        .HasColumnType("int");

                    b.Property<int>("Money")
                        .HasColumnType("int");

                    b.HasKey("PlayerDbId");

                    b.HasIndex("AccountDbId")
                        .IsUnique();

                    b.HasIndex("PlayerDbId")
                        .IsUnique();

                    b.ToTable("Player");
                });

            modelBuilder.Entity("Server.DB.ItemInfoDb", b =>
                {
                    b.HasOne("Server.DB.PlayerDb", "Player")
                        .WithMany("ItemSlot")
                        .HasForeignKey("PlayerDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Server.DB.PlayerDb", b =>
                {
                    b.HasOne("Server.DB.GameAccountDb", "Account")
                        .WithOne("Player")
                        .HasForeignKey("Server.DB.PlayerDb", "AccountDbId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
