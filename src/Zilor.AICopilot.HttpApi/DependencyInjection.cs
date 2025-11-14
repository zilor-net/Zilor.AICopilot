using System.Reflection;
using Zilor.AICopilot.EntityFrameworkCore;
using Zilor.AICopilot.IdentityService;

namespace Zilor.AICopilot.HttpApi;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public void AddInfrastructures()
        {
            services.AddIdentityEfCore();
        }

        public void AddServiceUseCase()
        {
            services.AddMediatR(cfg =>
            {
                cfg.LicenseKey =
                    "eyJhbGciOiJSUzI1NiIsImtpZCI6Ikx1Y2t5UGVubnlTb2Z0d2FyZUxpY2Vuc2VLZXkvYmJiMTNhY2I1OTkwNGQ4OWI0Y2IxYzg1ZjA4OGNjZjkiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2x1Y2t5cGVubnlzb2Z0d2FyZS5jb20iLCJhdWQiOiJMdWNreVBlbm55U29mdHdhcmUiLCJleHAiOiIxNzg0NzY0ODAwIiwiaWF0IjoiMTc1MzIzNjIyMSIsImFjY291bnRfaWQiOiIwMTk4MzUwNDdkNGI3ZmU5YmZlMzdhMWQ2MDQwMzM4NSIsImN1c3RvbWVyX2lkIjoiY3RtXzAxazB0Z2JiNHZ2N2tnZDlyMXBtc2pmNG4xIiwic3ViX2lkIjoiLSIsImVkaXRpb24iOiIwIiwidHlwZSI6IjIifQ.s5kG1QZdtbY_jtqsxQpdOQUSoXFb5MwFGp6AP1rBqPycBn03RUsmFVHaAQJKBMOHvTUGLLJPts1q7TaY7pV2Dut5n0LtnNXaq4r8AZ5rOSQWOAcLfuMUFMLDwhR9BGuPODvNje74evts-4zB6qJKwxcdk8a-DrN1qGEQcB3Zksh1Su02jIDBiUjvAG07wjUdt-n8AdMF2kM-hPAMdxBV4Wr_cqJV_EbimBAiEeMUpey7G4qaLPcsJo0lKu7T6KRjc3YNpiZ9hGh9Tf_JWHMS__ed9wpueK6kvFjwQuBAGjpFb51FFdQDUh2Uuuuo7ldvofBSaX6xIfXjLL1hqQ0MEQ";

                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            });
            services.AddIdentityService();
        }
    }

    public static void AddAspire(this WebApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.AddNpgsqlDbContext<AiCopilotDbContext>("ai-copilot");
    }
}