using Microsoft.OpenApi.Models;
using Shared.ConfigurationKeys;

namespace MyCustomApp.API.Extensions
{
    public static class SwaggerExtension
    {
        public static void SetupSwagger(this IServiceCollection services, string title)
        {
            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc(ConfigurationKeys.Swagger.Version, new OpenApiInfo
                {
                    Title = title,
                    Version = ConfigurationKeys.Swagger.Version
                });
                opt.AddSecurityDefinition(ConfigurationKeys.Swagger.SecuritySchemeName, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = ConfigurationKeys.Swagger.TokenDescription,
                    Name = ConfigurationKeys.Swagger.AuthorizationHeader,
                    Type = SecuritySchemeType.Http,
                    BearerFormat = ConfigurationKeys.Swagger.BearerFormat,
                    Scheme = ConfigurationKeys.Swagger.Scheme
                });

                opt.CustomSchemaIds(type => type.ToString());

                opt.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ConfigurationKeys.Swagger.SecuritySchemeName
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }
    }
}
