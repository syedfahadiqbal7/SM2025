using AFFZ_Customer.Models;
using Microsoft.Extensions.Options;

namespace AFFZ_Customer.Utils
{
    public interface IAppSettingsService
    {
        string GetBaseIpAddress();
        string GetPublicDomain();
        string GetApiHttpsPort();
        string GetMerchantHttpsPort();
        string GetCustomerHttpsPort();
        string GetAdminHttpsPort();
    }

    public class AppSettingsService : IAppSettingsService
    {
        private readonly AppSettings _settings;

        public AppSettingsService(IOptions<AppSettings> options)
        {
            _settings = options.Value;
        }

        public string GetAdminHttpsPort()
        {
            return _settings.AdminHttpsPort;
        }

        public string GetApiHttpsPort()
        {
            return _settings.ApiHttpsPort;
        }

        public string GetBaseIpAddress()
        {
            return _settings.BaseIpAddress;
        }

        public string GetCustomerHttpsPort()
        {
            return _settings.CustomerHttpsPort;
        }

        public string GetMerchantHttpsPort()
        {
            return _settings.MerchantHttpsPort;
        }

        public string GetPublicDomain()
        {
            return _settings.PublicDomain;
        }

    }
}
