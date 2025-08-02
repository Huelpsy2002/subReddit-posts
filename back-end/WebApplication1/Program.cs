using Microsoft.IdentityModel.Tokens;
using System.Runtime;
using WebApplication1.Data;
using System.Text;
using WebApplication1.busnessLogic;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                
                context.Token = context.HttpContext.Request.Cookies["token"];
                return Task.CompletedTask;
            }
        };


        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true, 

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("jwtSecretKey")))
        };
        options.MapInboundClaims = false;

    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<DataAccess>();
builder.Services.AddScoped<AuthLogic>();
builder.Services.AddScoped<DataBase>();


builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection("client_settings"));


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            
            policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            
            policy.WithOrigins("http://subredditposts.duckdns.org", "https://subredditposts.duckdns.org")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();
app.UseCors("AllowReactDev");

app.UseDefaultFiles(); // Look for index.html
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
