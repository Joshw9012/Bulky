
using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Bulky.Utility;
using Stripe;
using Bulky.DataAccess.DbInitializer;
using Microsoft.EntityFrameworkCore.Internal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//DB connections
builder.Services.AddDbContext<ApplicationDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//register the Stripe Keys,  it will auto-inject the two keys to the StripeSettings class (name in the setting must == to the name in the class)
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

//mapping User to role
builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();



builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});


//Face book auth
builder.Services.AddAuthentication().AddFacebook(option =>
{
    option.AppId = "1084908509260438";
    option.AppSecret = "63600030b6636462e394b5192e8e043f";

});

//Face book auth
builder.Services.AddAuthentication().AddMicrosoftAccount(option =>
{
    option.ClientId = "af7ac37f-d46c-49a8-884e-d86fcfe74973";
    option.ClientSecret = "MJL8Q~sBv4apICCaxNsN38fOYMHQTesfIBunjbXv";

});

//session storage for Shopping cart items
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout= TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


//Initialize DB service
builder.Services.AddScoped<IDbInitializer,DbInitializer>();

builder.Services.AddRazorPages();

//CRUD methods injections
builder.Services.AddScoped<ICategoryRepository,CategoryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//this UseStaticFiles() will configure the files under wwwwroot folder
//and all the static files will be made accessable in our app
app.UseStaticFiles();

StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

//session for shopping cart
app.UseSession();
seedDatebase();
app.MapRazorPages();
app.MapControllerRoute(
    //if nothing is defined in the route, app will go to the following page
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();


//seed database
void seedDatebase()
{
    using(var scope = app.Services.CreateScope())
    {
        var dbInitializer =  scope.ServiceProvider.GetRequiredService<IDbInitializer>();

        dbInitializer.Initialize();
    }
}