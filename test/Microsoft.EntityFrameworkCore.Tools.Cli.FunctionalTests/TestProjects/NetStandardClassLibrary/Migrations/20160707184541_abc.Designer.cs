﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NetStandardClassLibrary;

namespace NetStandardClassLibrary.Migrations
{
    [DbContext(typeof(NetStandardContext))]
    [Migration("20160707184541_abc")]
    partial class abc
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-alpha1");

            modelBuilder.Entity("NetStandardClassLibrary.Blog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Title");

                    b.HasKey("Id");

                    b.ToTable("Blogs");
                });
        }
    }
}
