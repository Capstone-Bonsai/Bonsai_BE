using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructures
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext()
        {

        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<SubCategory> SubCategory { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<ProductTag> ProductTag { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Manager> Manager { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Gardener> Gardener { get; set; }
        public DbSet<Address> Address { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<OrderTransaction> OrderTransaction { get; set; }
        public DbSet<DeliveryFee> DeliveryFee { get; set; }
        public DbSet<ContractImage> ContractImage { get; set; }
        public DbSet<AnnualWorkingDay> AnnualWorkingDay { get; set; }
        public DbSet<ServiceOrder> ServiceOrder { get; set; }
        public DbSet<ServiceImage> ServiceImage { get; set; }
        public DbSet<ServiceTransaction> ServiceTransaction { get; set; }
        public DbSet<Complain> Complain { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<Tasks> Task { get; set; }
        public DbSet<BaseTask> BaseTask { get; set; }
        public DbSet<OrderServiceTask> OrderServiceTask { get; set; }
        public DbSet<Holiday> Holiday { get; set; }
        public DbSet<ServiceDay> ServiceDay { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            var cascadeFKs = builder.Model.GetEntityTypes()
        .SelectMany(t => t.GetForeignKeys())
        .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            base.OnModelCreating(builder);
        }

        /*        private string GetConnectionString()
                {
                    IConfiguration config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json", true, true).Build();
                    var strConn = config["ConnectionStrings:Development"];
                    return strConn;
                }*/
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }
    }
}
