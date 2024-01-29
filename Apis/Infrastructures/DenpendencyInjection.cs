using Application;
using Application.Interfaces;
using Application.Repositories;
using Application.Services;
using Domain.Entities;
using Infrastructures.Mappers;
using Infrastructures.Repositories;
using Infrastructures.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructures
{
    public static class DenpendencyInjection
    {
        public static IServiceCollection AddInfrastructuresService(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ISubCategoryService, SubCategoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IOrderTransactionService, OrderTransactionService>();
            services.AddScoped<IProductImageService, ProductImageService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<IProductTagService, ProductTagService>();
            services.AddScoped<IDeliveryFeeService, DeliveryFeeService>();



            services.AddScoped<IGardenerRepository, GardenerRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderTransactionRepository, OrderTransactionRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITagRepository, TagRepository>();
            services.AddScoped<IProductTagRepository, ProductTagRepository>();
            services.AddScoped<IDeliveryFeeRepository, DeliveryFeeRepository>();


            services.AddScoped<IUnitOfWork, UnitOfWork>();


            services.AddSingleton<ICurrentTime, CurrentTime>();
            services.AddHttpClient<IDeliveryFeeService, DeliveryFeeService>();

            // ATTENTION: if you do migration please check file README.md
            /*if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("mentor_v1Db"));
            }
            else
            {
                
            }*/
            services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(GetConnection(configuration, env),
                        builder => builder.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
            }).AddDefaultTokenProviders().AddEntityFrameworkStores<AppDbContext>();
            // this configuration just use in-memory for fast develop
            //services.AddDbContext<AppDbContext>(option => option.UseInMemoryDatabase("test"));

            services.AddAutoMapper(typeof(MapperConfigurationsProfile).Assembly);
            services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedEmail = true);


            return services;
        }

        private static string GetConnection(IConfiguration configuration, IWebHostEnvironment env)
        {
#if DEVELOPMENT
        return configuration.GetConnectionString("DefaultConnection") 
            ?? throw new Exception("DefaultConnection not found");
#else
            return configuration[$"ConnectionStrings:{env.EnvironmentName}"]
                ?? throw new Exception($"ConnectionStrings:{env.EnvironmentName} not found");
#endif
        }
    }


}
