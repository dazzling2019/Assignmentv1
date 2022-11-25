using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Assignmentv1.Data;
using Newtonsoft.Json;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<Assignmentv1Context>();
// Add services to the container.
builder.Services.AddControllers()
.AddNewtonsoftJson(x =>
{
    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
#region Add CORS
builder.Services.AddCors(options => options.AddPolicy("Cors", builder =>
{
    builder
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader();
}));
#endregion
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<Assignmentv1Context>()
.AddDefaultTokenProviders();
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
    new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
        ValidAudience = builder.Configuration["JWT:ValidAudience"],
        IssuerSigningKey =
    JwtSecurityKey.Create(builder.Configuration["JWT:Secret"]),
        ClockSkew = TimeSpan.Zero
    };
});
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("Cors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    RoleManager<IdentityRole> roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    CreateInitialRolesAndUsersAsync(userManager, roleManager)
    .Wait();
}
app.Run();
async Task CreateInitialRolesAndUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
{
    try
    {
        string adminRoleName = Roles.Admin;
        if (!await roleManager.RoleExistsAsync(adminRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        }
        string staffRoleName = Roles.Student;
        if (!await roleManager.RoleExistsAsync(staffRoleName))
        {
            await roleManager.CreateAsync(new IdentityRole(staffRoleName));
        }
        var user = new ApplicationUser();
        user.UserName = "admin@Assignmentv0DatabaseXamarinApp.bolton.ac.uk";
        user.Email = user.UserName;
        user.Firstname = "Jen";
        user.Lastname = "Erik";
        string password = "Pa$$w0rd!";
        if (await userManager.FindByNameAsync(user.UserName) == null)
        {
            var createSuccess = await userManager.CreateAsync(user, password);
            if (createSuccess.Succeeded)
            {
                await userManager.AddToRoleAsync(user, adminRoleName);
                await userManager.SetLockoutEnabledAsync(user, false);
            }
            else
            {
                throw new Exception(createSuccess.Errors.FirstOrDefault().ToString());
            }
        }
    }
    catch (Exception e)
    {
        throw new Exception(e.Message);
    }
}