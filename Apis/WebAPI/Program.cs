using Application.Commons;
using Application.Hubs;
using Domain.Entities;
using Infrastructures;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebAPI;
using WebAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

//builder.Environment.EnvironmentName = "Staging"; //for branch develop
//builder.Environment.EnvironmentName = "Production"; //for branch domain 
builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", false, true)
    .AddUserSecrets<Program>(true, false)
    .Build();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod();
                      });
});

// parse the configuration in appsettings
var configuration = builder.Configuration.Get<AppConfiguration>();
builder.Services.AddInfrastructuresService(builder.Configuration, builder.Environment);
builder.Services.AddWebAPIService();
builder.Services.AddSignalR();
builder.Services.AddSingleton(configuration);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecrectKey"]))

    };
});
var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Initialise and seed database
using (var scope = app.Services.CreateScope())
{
    var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var managerRole = new IdentityRole("Manager");

    if (_roleManager.Roles.All(r => r.Name != managerRole.Name))
    {
        await _roleManager.CreateAsync(managerRole);
    }

    // staff roles
    var staffRole = new IdentityRole("Staff");

    if (_roleManager.Roles.All(r => r.Name != staffRole.Name))
    {
        await _roleManager.CreateAsync(staffRole);
    }   

    // customer roles
    var customerRole = new IdentityRole("Customer");

    if (_roleManager.Roles.All(r => r.Name != customerRole.Name))
    {
        await _roleManager.CreateAsync(customerRole);
    }

    // sale roles
    var gardenerRole = new IdentityRole("Gardener");

    if (_roleManager.Roles.All(r => r.Name != gardenerRole.Name))
    {
        await _roleManager.CreateAsync(gardenerRole);
    }


}

using (var scope = app.Services.CreateScope())
{
    var manager = new ApplicationUser { UserName = "Manager@localhost", Email = "Manager@localhost", Fullname = "Manager", AvatarUrl = "(null)", IsRegister = true, EmailConfirmed= true };
    var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    if (_userManager.Users.All(u => u.UserName != manager.UserName))
    {
        await _userManager.CreateAsync(manager, "Manager@123");
        if (!string.IsNullOrWhiteSpace("Manager"))
        {
            await _userManager.AddToRolesAsync(manager, new[] { "Manager" });
        }
    }
}

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{*/
    app.UseSwagger();
    app.UseSwaggerUI();
/*}*/
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<PerformanceMiddleware>();
app.MapHealthChecks("/healthchecks");
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("notification-hub");

app.Run();

// this line tell intergrasion test
// https://stackoverflow.com/questions/69991983/deps-file-missing-for-dotnet-6-integration-tests
public partial class Program { }

