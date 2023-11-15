using MCS.Data;
using MCS.Services.UserServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var _configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

string? encodedMySqlConString = _configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrEmpty(encodedMySqlConString))
{
    string MySqlConString = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(encodedMySqlConString));

    var serverVersion = new MySqlServerVersion(new Version(5, 7, 42)); 

    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseMySql(MySqlConString, serverVersion); 
    });
}
else
{
    throw new ArgumentNullException("DefaultConnection", "Connection string is null or empty.");
}

builder.Services.AddHttpContextAccessor();


builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        string? encodedjwtKey = _configuration.GetSection("JWT:Key").Value;

        if (encodedjwtKey != null)
        {
            try
            {
                string JWTKey = Encoding.UTF8.GetString(Convert.FromBase64String(encodedjwtKey));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JWTKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            }
            catch (Exception ex)
            {
                Log.Error("Invalid JWT Key configuration: " + ex.Message);
            }
        }
        else
        {
            Log.Error("JWT:Key is missing or null. Please configure it properly.");
        }
    });

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/mcs-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
