using System.ComponentModel.DataAnnotations;
using Entities.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Configuration;

public static class ConfigurationExtensions
{
    public static void VadaliteIdInRange(this int id)
    {
        if (!(id > 0 && id <= 1000))
            throw new ArgumentOutOfRangeException("1-1000");
    }

    public static void UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes
                        .Status500InternalServerError;

                context.Response.ContentType = "application/json";

                var contextFeature = context
                    .Features
                    .Get<IExceptionHandlerFeature>();

                if (contextFeature is not null)
                {
                    context.Response.StatusCode = contextFeature.Error switch
                    {
                        NotFoundException => StatusCodes.Status404NotFound,
                        ValidationException => StatusCodes.Status422UnprocessableEntity,
                        ArgumentOutOfRangeException => StatusCodes.Status400BadRequest,
                        ArgumentException => StatusCodes.Status400BadRequest,
                        _ => StatusCodes.Status500InternalServerError,
                    };

                    await context.Response.WriteAsync(new ErrorDetails()
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = contextFeature.Error.Message,
                    }.ToString()
                    );
                }
            });
        });
    }

    public static IServiceCollection AddCustomCors(this IServiceCollection services)
    {

        services.AddCors(options =>
        {
            // all
            options.AddPolicy("all", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });

            // special
            options.AddPolicy("special", builder =>
            {
                builder.WithOrigins("https://localhost:3000")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            });
        });
        return services;
    }

    public static IServiceCollection AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new()
            {
                Title = "Book API",
                Version = "v1",
                Description = "Virtual Campus",
                License = new(),
                TermsOfService = new("https://www.samsun.edu.tr"),
                Contact = new()
                {
                    Email = "zcomert@samsun.edu.tr",
                    Name = "Zafer Cömert",
                    Url = new Uri("https://www.youtube.com/@virtual.campus")
                }
            });
        });
        return services;
    }


}