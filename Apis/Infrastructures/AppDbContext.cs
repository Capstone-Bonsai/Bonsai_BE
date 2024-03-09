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
        public DbSet<Bonsai> Bonsai { get; set; }
        public DbSet<BonsaiImage> BonsaiImage { get; set; }
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Manager> Manager { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Gardener> Gardener { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<OrderTransaction> OrderTransaction { get; set; }
        public DbSet<DeliveryFee> DeliveryFee { get; set; }

        public DbSet<CustomerBonsai> CustomerBonsai { get; set; }
        public DbSet<CustomerGarden> CustomerGarden { get; set; }
        public DbSet<CustomerGardenImage> CustomerGardenImage { get; set; }
        public DbSet<CategoryExpectedPrice> CategoryExpectedPrice { get; set; }
        public DbSet<CareStep> CareStep { get; set; }
        public DbSet<Contract> Contract { get; set; }
        public DbSet<ContractImage> ContractImage { get; set; }
        public DbSet<BonsaiCareStep> BonsaiCareStep { get; set; }
        public DbSet<ContractGardener> ContractGardener { get; set; }
        public DbSet<ContractTransaction> ContractTransaction { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<BaseTask> BaseTask { get; set; }
        public DbSet<GardenCareTask> GardenCareTask { get; set; }
        public DbSet<ServiceBaseTask> ServiceBaseTask { get; set; }
        public DbSet<ServiceSurcharge> ServiceSurcharge { get; set; }
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
