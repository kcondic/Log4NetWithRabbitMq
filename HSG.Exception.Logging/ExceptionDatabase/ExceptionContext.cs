﻿using System.Data.Entity;
using HSG.Exception.Logging.ExceptionStructure;

namespace HSG.Exception.Logging.ExceptionDatabase
{
    public class ExceptionContext : DbContext
    {
        public ExceptionContext() : base("ExceptionDatabase")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ExceptionContext>());           
        }

        public virtual DbSet<QueueException> QueueExceptions { get; set; }
        public virtual DbSet<TransformException> TransformExceptions { get; set; }
        public virtual DbSet<DirtyQueueException> DirtyQueueExceptions { get; set; }
        public virtual DbSet<DirtyTransformException> DirtyTransformExceptions { get; set; }
        public virtual DbSet<HistoricalQueueException> HistoricalQueueExceptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueException>()
                .HasKey(x => x.ExceptionId);
            modelBuilder.Entity<TransformException>()
                .HasKey(x => new {x.ExceptionId, x.Order});
            modelBuilder.Entity<DirtyQueueException>()
                .HasKey(x => x.ExceptionId);
            modelBuilder.Entity<DirtyTransformException>()
                .HasKey(x => new {x.ExceptionId, x.Order});
            modelBuilder.Entity<HistoricalQueueException>()
                .HasRequired(x => x.QueueException)
                .WithMany(x => x.HistoricalExceptions)
                .HasForeignKey(x => x.QueueExceptionId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
