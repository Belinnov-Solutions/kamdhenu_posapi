
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using BELEPOS.DataModel;
using BELEPOS.Entity;
using BELEPOS.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using BELEPOS.Helper;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;


namespace BELEPOS
{
    public class Program
    {
        public static void Main(string[] args)
        {
         
            var builder = WebApplication.CreateBuilder(args);

            //RazorTemplating
            builder.Services.AddRazorTemplating();
            


            // Add services to the container.

            builder.Services.AddDbContext<BeleposContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.WriteIndented = true;
            });


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .SetIsOriginAllowed(origin => true)
                        .AllowCredentials()
                        //.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            builder.Services.AddApiVersioning(options =>
            {
               options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.ReportApiVersions = true;
 
            }).AddMvc(options =>
            {
                options.Conventions.Add(new VersionByNamespaceConvention());
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();



            // Bind JWT settings
            builder.Services.Configure<JwtSettingsDto>(builder.Configuration.GetSection("JwtSettings"));
            var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettingsDto>();

            // Add authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });
           builder.Services.AddScoped<EPoshelper>();

            

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, CustomAuthorizationPolicyProvider>();
            builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            builder.Services.AddHostedService<RepairOrderSyncService>();
            builder.Services.AddScoped<JwtService>();
            builder.Services.AddScoped<IClaimsTransformation, RoleHierarchyClaimsTransformer>();
            var app = builder.Build();
            app.UseDeveloperExceptionPage();

            // Configure the HTTP request pipeline.
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger();
            /*app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                                            description.GroupName.ToUpperInvariant());
                }
            });*/


            app.UseSwaggerUI(options =>
            {
                //var basePath = app.Environment.IsDevelopment() ? string.Empty : "https://eposapi.belinnov.in"; // adjust as needed
                var basePath = app.Environment.IsDevelopment() ? string.Empty : "http://localhost/BELEPOSAPI"; // adjust as needed

                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"{basePath}/swagger/{description.GroupName}/swagger.json",
                                            description.GroupName.ToUpperInvariant());
                }

                options.RoutePrefix = "swagger"; // or "" for root
            });


            app.UseCors("AllowAll");
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
