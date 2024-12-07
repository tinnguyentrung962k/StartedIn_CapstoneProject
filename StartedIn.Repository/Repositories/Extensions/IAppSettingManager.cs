using StartedIn.CrossCutting.Enum;
using StartedIn.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.Repository.Repositories.Extensions
{
    public interface IAppSettingManager
    {
        Task SeedDefaultSettingsAsync(); // Seed các cài đặt mặc định
        Task<SettingsValue?> GetSettingAsync(string key); // Lấy giá trị cài đặt
        Task SetSettingAsync(string key, SettingsValue value); // Cập nhật giá trị cài đặt
    }
}
