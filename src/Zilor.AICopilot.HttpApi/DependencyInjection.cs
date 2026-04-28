using System.Reflection;
using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Zilor.AICopilot.AiGatewayService;
using Zilor.AICopilot.DataAnalysisService;
using Zilor.AICopilot.HttpApi.Infrastructure;
using Zilor.AICopilot.Infrastructure.Authentication;
using Zilor.AICopilot.McpService;
using Zilor.AICopilot.RagService;
using Zilor.AICopilot.Services.Common.Behaviors;
using Zilor.AICopilot.Services.Common.Contracts;

namespace Zilor.AICopilot.HttpApi;

public static class DependencyInjection
{
    extension(IHostApplicationBuilder builder)
    {
        public void AddApplicationService()
        {
            builder.Services.AddMediatR(cfg =>
            {
                cfg.LicenseKey = Environment.GetEnvironmentVariable("MEDIATR_LICENSE_KEY");

                cfg.RegisterServicesFromAssembly(Assembly.GetAssembly(typeof(IdentityService.DependencyInjection))!);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
            });

            builder.AddAiGatewayService();

            builder.AddDataAnalysisService();

            builder.AddRagService();

            builder.AddMcpService();
        }

        public void AddWebServices()
        {
            var configurationSection = builder.Configuration.GetSection("JwtSettings");
            var jwtSettings = configurationSection.Get<JwtSettings>();
            if (jwtSettings is null) throw new NullReferenceException(nameof(jwtSettings));

            builder.Services.Configure<JwtSettings>(configurationSection);

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)
                        ),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddScoped<ICurrentUser, CurrentUser>();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddExceptionHandler<UseCaseExceptionHandler>();

            builder.Services.AddProblemDetails();
        }
    }
}
