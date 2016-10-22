using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Tables;
using HyprsoftProjectsMobileService.DataObjects;

namespace HyprsoftProjectsMobileService.Models
{
    public class MobileServiceContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to alter your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
        //
        // To enable Entity Framework migrations in the cloud, please ensure that the 
        // service name, set by the 'MS_MobileServiceName' AppSettings in the local 
        // Web.config, is the same as the service name when hosted in Azure.

        public MobileServiceContext() : base(HyprsoftProjectsCommon.Constants.MobileServiceDatabaseConnectionString)
        {
        }

        public DbSet<Project> Projects { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Add(new AttributeToColumnAnnotationConvention<TableColumnAttribute, string>("ServiceTableColumn", (property, attributes) => attributes.Single().ColumnType.ToString()));

            modelBuilder.Entity<Project>().Property(p => p.Title).IsRequired();
            modelBuilder.Entity<Project>().Property(p => p.Title).HasMaxLength(100);

            modelBuilder.Entity<Project>().Property(p => p.Description).IsRequired();
            modelBuilder.Entity<Project>().Property(p => p.Description).HasMaxLength(255);

            modelBuilder.Entity<Project>().Property(p => p.ImageUri).IsRequired();
            modelBuilder.Entity<Project>().Property(p => p.ImageUri).HasMaxLength(255);

            modelBuilder.Entity<Project>().Property(p => p.LinkUri).IsRequired();
            modelBuilder.Entity<Project>().Property(p => p.LinkUri).HasMaxLength(255);
        }
    }
}
