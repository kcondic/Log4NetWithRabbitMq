using System.Data.Entity;
using Log4NetAppender.ExceptionStructure;

namespace Log4NetAppender.ExceptionDatabase
{
    public class ExceptionContext : DbContext
    {
        public ExceptionContext() : base("ExceptionDatabase")
        {
            Database.SetInitializer(new CreateDatabaseIfNotExists<ExceptionContext>());           
        }

        public virtual DbSet<QueueException> QueueExceptions { get; set; }
        public virtual DbSet<TransformException> TransformExceptions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QueueException>()
                .HasKey(x => x.ExceptionId);
            modelBuilder.Entity<TransformException>()
                .HasKey(x => new {x.ExceptionId, x.Order});
            base.OnModelCreating(modelBuilder);
        }
    }
}
