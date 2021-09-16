using System.Reflection;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace CS_APP.DataLayer.Models
{
     public class ApplicationContext : DbContext
     {
          public ApplicationContext()
          {
          }

          public ApplicationContext(DbContextOptions<ApplicationContext> options)
              : base(options)
          {
          }

          public virtual DbSet<AuditPolicy> AuditPolicies { get; set; }

          protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
          {
               if (!optionsBuilder.IsConfigured)
               {
                    optionsBuilder.UseSqlServer("Data Source=ANDY; Initial Catalog=CsApp;Integrated Security=SSPI");
               }
          }

          protected override void OnModelCreating(ModelBuilder modelBuilder)
          {
               modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

               modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(ApplicationContext)));

               base.OnModelCreating(modelBuilder);

          }

     }
}
