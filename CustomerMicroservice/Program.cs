using Google.Api.Gax;
using Google.Cloud.Firestore;
using Google.Cloud.PubSub.V1;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OrderMicroservice.Data;
using OrderMicroservice.Repositories;
using OrderMicroservice.Services;
using System;
using System.IO;
using System.Text;
using VideoCatalogueMicroservice.Services;

var builder = WebApplication.CreateBuilder(args);

// Set the Google Application Credentials environment variable programmatically (for debugging)
string relativePath = @"dphome-424621-19806e674912.json";  // Assuming this file is directly in the project directory
string basePath = AppContext.BaseDirectory;  // Get the base directory of the application
string credentialPath = Path.Combine(basePath, relativePath);

// Print the resolved path for debugging
Console.WriteLine($"Resolved credential path: {credentialPath}");

// Verify the file exists
if (!File.Exists(credentialPath))
{
    throw new FileNotFoundException("Credential file not found", credentialPath);
}

// Set the environment variable
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.AddControllersWithViews();

// Configure Data Protection to use file system for key storage
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(basePath, "keys")));

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IWatchlistRepository, WatchlistRepository>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();

builder.Services.AddSingleton<IVideoCatalogueService, VideoCatalogueService>();
builder.Services.AddLogging();

builder.Services.AddDbContext<OrderContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<SubscriberServiceApiClient>(sp =>
{
    var subscriberService = new SubscriberServiceApiClientBuilder
    {
        EmulatorDetection = EmulatorDetection.EmulatorOrProduction
    }.Build();
    return subscriberService;
});

//builder.Services.AddHostedService<OrderConfirmationSubscriber>();
builder.Services.AddHostedService<UpcomingVideoSubscriber>();

// Configure Firestore for dphomedb
builder.Services.AddSingleton(sp =>
{
    var projectId = builder.Configuration["GoogleCloud:ProjectId"];
    Console.WriteLine($"Initializing Firestore for Project: {projectId}, Database: dphomedb");
    return new FirestoreDbBuilder
    {
        ProjectId = projectId,
        DatabaseId = builder.Configuration["GoogleCloud:Firestore:DatabaseId"]  // Explicitly set the database ID
    }.Build();
});

// Use the same Firestore instance for orders collection
builder.Services.AddSingleton<IOrderRepository, OrderRepository>(sp =>
{
    var firestoreDb = sp.GetRequiredService<FirestoreDb>();
    var logger = sp.GetRequiredService<ILogger<OrderRepository>>();
    return new OrderRepository(firestoreDb, logger);
});

// Configure JWT Authentication
var key = System.Text.Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not found in configuration."));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"]
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IUserRepository, UserRepository>();
//builder.Services.AddScoped<IEventService, EventService>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
