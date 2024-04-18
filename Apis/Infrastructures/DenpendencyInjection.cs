using Application;
using Application.Hubs;
using Application.Interfaces;
using Application.Repositories;
using Application.Services;
using Application.Utils;
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
            services.AddScoped<IOrderDetailService, OrderDetailService>();
            services.AddScoped<IOrderTransactionService, OrderTransactionService>();
            services.AddScoped<IBonsaiService, BonsaiService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IStyleService, StyleService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<ICustomerGardenService, CustomerGardenService>();
            services.AddScoped<ICareStepService, CareStepService>();
            services.AddScoped<IBaseTaskService, BaseTaskService>();
            services.AddScoped<IServiceBaseTaskService, ServiceBaseTaskService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IServiceOrderService, ServiceOrderService>();
            services.AddScoped<ITaskService, TaskService>();
            services.AddScoped<IServiceOrderGardenerService, ServiceOrderGardenerService>();
            services.AddScoped<ICustomerBonsaiService, CustomerBonsaiService>();
            services.AddScoped<IComplaintService, ComplaintService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<IUserConnectionService, UserConnectionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IDashBoardService, DashBoardService>();
            

            services.AddScoped<IDeliveryFeeRepository, DeliveryFeeRepository>();
            services.AddScoped<IStaffRepository, StaffRepository>();
            services.AddScoped<IGardenerRepository, GardenerRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IBonsaiRepository, BonsaiRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IOrderTransactionRepository, OrderTransactionRepository>();
            services.AddScoped<IBonsaiImageRepository, BonsaiImageRepository>();
            services.AddScoped<IStyleRepository, StyleRepository>();
            services.AddScoped<ICustomerGardenRepository, CustomerGardenRepository>();
            services.AddScoped<ICustomerGardenImageRepository, CustomerGardenImageRepository>();
            services.AddScoped<ICustomerBonsaiRepository, CustomerBonsaiRepository>();
            services.AddScoped<ICareStepRepository, CareStepRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IBaseTaskRepository, BaseTaskRepository>();
            services.AddScoped<IServiceBaseTaskRepository, ServiceBaseTaskRepository>();
            services.AddScoped<IServiceOrderRepository, ServiceOrderRepository>();
            services.AddScoped<IBonsaiCareStepRepository, BonsaiCareStepRepository>();
            services.AddScoped<IGardenCareTaskRepository, GardenCareTaskRepository>();
            services.AddScoped<IServiceOrderGardenerRepository, ServiceOrderGardenerRepository>();
            services.AddScoped<IServiceOrderTransactionRepository, ServiceOrderTransactionRepository>();
            services.AddScoped<IComplaintRepository, ComplaintRepository>();
            services.AddScoped<IComplaintImageRepository, ComplaintImageRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IDeliveryImageRepository, DeliveryImageRepository>();
            services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
            

            services.AddScoped<IFirebaseService, FirebaseService>();
            services.AddScoped<NotificationHub>();
            services.AddSingleton<FirebaseService>();
            services.AddScoped<IdUtil>();
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
