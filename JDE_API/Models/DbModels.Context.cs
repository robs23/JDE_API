﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace JDE_API.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DbModel : DbContext
    {
        public DbModel()
            : base("name=DbModel")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<JDE_ActionTypes> JDE_ActionTypes { get; set; }
        public virtual DbSet<JDE_Areas> JDE_Areas { get; set; }
        public virtual DbSet<JDE_Logs> JDE_Logs { get; set; }
        public virtual DbSet<JDE_Sets> JDE_Sets { get; set; }
        public virtual DbSet<JDE_Tenants> JDE_Tenants { get; set; }
        public virtual DbSet<JDE_Errors> JDE_Errors { get; set; }
        public virtual DbSet<JDE_Boms> JDE_Boms { get; set; }
        public virtual DbSet<JDE_Deliveries> JDE_Deliveries { get; set; }
        public virtual DbSet<JDE_DeliveryItems> JDE_DeliveryItems { get; set; }
        public virtual DbSet<JDE_Stocks> JDE_Stocks { get; set; }
        public virtual DbSet<JDE_OrderItems> JDE_OrderItems { get; set; }
        public virtual DbSet<JDE_Orders> JDE_Orders { get; set; }
        public virtual DbSet<JDE_CompanyTypes> JDE_CompanyTypes { get; set; }
        public virtual DbSet<JDE_FileAssigns> JDE_FileAssigns { get; set; }
        public virtual DbSet<JDE_Actions> JDE_Actions { get; set; }
        public virtual DbSet<JDE_PlaceActions> JDE_PlaceActions { get; set; }
        public virtual DbSet<JDE_ProcessActions> JDE_ProcessActions { get; set; }
        public virtual DbSet<JDE_ProcessAssigns> JDE_ProcessAssigns { get; set; }
        public virtual DbSet<JDE_Handlings> JDE_Handlings { get; set; }
        public virtual DbSet<JDE_Companies> JDE_Companies { get; set; }
        public virtual DbSet<JDE_Parts> JDE_Parts { get; set; }
        public virtual DbSet<JDE_Places> JDE_Places { get; set; }
        public virtual DbSet<JDE_Users> JDE_Users { get; set; }
        public virtual DbSet<JDE_PartUsages> JDE_PartUsages { get; set; }
        public virtual DbSet<JDE_Components> JDE_Components { get; set; }
        public virtual DbSet<JDE_Files> JDE_Files { get; set; }
        public virtual DbSet<JDE_PartPrices> JDE_PartPrices { get; set; }
        public virtual DbSet<JDE_StorageBins> JDE_StorageBins { get; set; }
        public virtual DbSet<JDE_StockTakings> JDE_StockTakings { get; set; }
        public virtual DbSet<JDE_AbandonReasons> JDE_AbandonReasons { get; set; }
        public virtual DbSet<JDE_Processes> JDE_Processes { get; set; }
    }
}
