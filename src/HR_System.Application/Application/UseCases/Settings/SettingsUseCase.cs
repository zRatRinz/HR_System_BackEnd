using HR_System.Application.DTOs.Settings;

namespace HR_System.Application.UseCases.Settings;

public class SettingsUseCase
{
    private static SettingsDto _settings = new()
    {
        CompanyName = "PulsePoint HR",
        LogoUrl = "/logo.png",
        Theme = "light",
        Language = "en",
        DateFormat = "yyyy-MM-dd",
        Currency = "USD"
    };

    public Task<SettingsDto> GetAsync()
    {
        return Task.FromResult(_settings);
    }

    public Task<SettingsDto> UpdateAsync(SettingsDto settings)
    {
        _settings = settings;
        return Task.FromResult(_settings);
    }
}
