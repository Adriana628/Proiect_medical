using Microsoft.EntityFrameworkCore;
using Proiect_medical.Data;
using Microsoft.AspNetCore.Identity;
using Proiect_medical.Models;
using Proiect_medical.Hubs;
using Proiect_medical.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with roles
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Add support for roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DoctorPolicy", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("PatientPolicy", policy => policy.RequireRole("Patient"));
});

builder.Services.AddHttpClient<NewsService>();

var app = builder.Build();

// Seed roles and users
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // Create roles if they don't exist
    string[] roles = { "Doctor", "Patient" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Create a sample Doctor user
    var doctorUser = await userManager.FindByEmailAsync("doctor@example.com");
    if (doctorUser == null)
    {
        var newDoctor = new IdentityUser { UserName = "doctor@example.com", Email = "doctor@example.com", EmailConfirmed = true };
        await userManager.CreateAsync(newDoctor, "Password123!");
        await userManager.AddToRoleAsync(newDoctor, "Doctor");
    }

    // Create a sample Patient user
    var patientUser = await userManager.FindByEmailAsync("patient@example.com");
    if (patientUser == null)
    {
        var newPatient = new IdentityUser { UserName = "patient@example.com", Email = "patient@example.com", EmailConfirmed = true };
        await userManager.CreateAsync(newPatient, "Password123!");
        await userManager.AddToRoleAsync(newPatient, "Patient");
    }
   
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Ensure Razor Pages are mapped

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/Chat"); // Maparea Hub-ului

app.MapFallbackToFile("index.html");
app.MapRazorPages();
app.Run();
