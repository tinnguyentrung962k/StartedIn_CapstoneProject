using Microsoft.EntityFrameworkCore;
using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Context;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Extensions
{
    public class AppSettingManager : IAppSettingManager
    {
        private readonly AppDbContext _context; // Inject DbContext

        public AppSettingManager(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedDefaultSettingsAsync()
        {
            var defaultSettings = new Dictionary<string, SettingsValue>
            {
                { "SignatureType", SettingsValue.SignNow}, // Giá trị mặc định là ký nội bộ
            };

            foreach (var setting in defaultSettings)
            {
                if (!_context.AppSettings.Any(a => a.Id == setting.Key))
                {
                    _context.AppSettings.Add(new AppSetting
                    {
                        Id = setting.Key,
                        SettingValue = setting.Value
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SettingsValue?> GetSettingAsync(string key)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(a => a.Id == key);
            return setting?.SettingValue;
        }

        public async Task SetSettingAsync(string key, SettingsValue value)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(a => a.Id == key);

            if (setting != null)
            {
                setting.SettingValue = value;
            }
            else
            {
                _context.AppSettings.Add(new AppSetting
                {
                    Id = key,
                    SettingValue = value
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
