﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Storage.Classes.Models.Canteens;
using Windows.Storage;

namespace Storage.Classes.Contexts.Canteens
{
    public class CanteensDbContext: DbContext
    {
        //--------------------------------------------------------Attributes:-----------------------------------------------------------------\\
        #region --Attributes--
        private static readonly string DB_PATH = Path.Combine(ApplicationData.Current.LocalFolder.Path, "canteens.db");

        public DbSet<Canteen> Canteens { get; set; }
        public DbSet<Location> Locations { get; set; }

        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Price> Prices { get; set; }

        #endregion
        //--------------------------------------------------------Constructor:----------------------------------------------------------------\\
        #region --Constructors--
        public CanteensDbContext()
        {
            // Disable change tracking since we always manually update them and only require them read only:
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            Database.EnsureCreated();
        }

        #endregion
        //--------------------------------------------------------Set-, Get- Methods:---------------------------------------------------------\\
        #region --Set-, Get- Methods--


        #endregion
        //--------------------------------------------------------Misc Methods:---------------------------------------------------------------\\
        #region --Misc Methods (Public)--


        #endregion

        #region --Misc Methods (Private)--


        #endregion

        #region --Misc Methods (Protected)--
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=" + DB_PATH);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Based on: https://entityframeworkcore.com/knowledge-base/37370476/how-to-persist-a-list-of-strings-with-entity-framework-core-
            ValueConverter<List<string>, string> splitStringConverter = new ValueConverter<List<string>, string>(v => string.Join(";", v), v => v.Split(new[] { ';' }).ToList());
            // Make sure we can store a list of strings in the DB:
            modelBuilder.Entity<Dish>().Property(nameof(Dish.Ingredients)).HasConversion(splitStringConverter);
        }

        #endregion
        //--------------------------------------------------------Events:---------------------------------------------------------------------\\
        #region --Events--


        #endregion
    }
}