using StartedIn.Repository.Repositories.Extensions;

namespace StartedIn.API.Configuration
{
    public static class AppSettingsConfiguration
    {
        public static IApplicationBuilder SeedSettings(this IApplicationBuilder builder)
        {
            using (var scope = builder.ApplicationServices.CreateScope())
            {
                var appSettingManager = scope.ServiceProvider.GetRequiredService<IAppSettingManager>();
                appSettingManager.SeedDefaultSettingsAsync().Wait(); // Seed cài đặt mặc định
            }

            return builder;
        }
    }
}
