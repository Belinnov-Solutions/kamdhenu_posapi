//using Asp.Versioning.ApiExplorer;
//using Microsoft.Extensions.Options;
//using Microsoft.OpenApi.Models;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace BELEPOS
//{
//    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
//    {
//        private readonly IApiVersionDescriptionProvider _provider;

//        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
//        {
//            _provider = provider;
//        }

//        public void Configure(SwaggerGenOptions options)
//        {
//            foreach (var description in _provider.ApiVersionDescriptions)
//            {
//                options.SwaggerDoc(
//                    description.GroupName,
//                    new OpenApiInfo()
//                    {
//                        Title = $"BELEPOS API {description.ApiVersion}",
//                        Version = description.ApiVersion.ToString()
//                    });
//            }

//            // Optional: resolve conflicts
//            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
//        }
//    }
//}
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Asp.Versioning.ApiExplorer;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // 🔹 Add a Swagger doc for each API version
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo()
            {
                Title = $"BELEPOS API {description.ApiVersion}",
                Version = description.ApiVersion.ToString()
            });
        }

        // 🔐 Add JWT Bearer Auth to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Enter your JWT token here (with 'Bearer ' prefix)",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
