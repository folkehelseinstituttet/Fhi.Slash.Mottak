using Microsoft.Extensions.Configuration;
using Slash.Public.Common.Configurations;

namespace Slash.Public.APIMessenger.Extensions;

public static class IConfigurationExtensions
{
    public static T GetConfig<T>(this IConfiguration configuration, string? sectionName = null) where T : new()
    {
        if(string.IsNullOrWhiteSpace(sectionName))
        {
            var type = typeof(T);
            var configAttribute = type.GetCustomAttributes(typeof(ConfigAttribute), false).FirstOrDefault() as ConfigAttribute ??
                throw new Exception($"ConfigAttribute is missing on {type.Name}");
            sectionName = configAttribute.SectionName;
        }

        var config = new T();
        configuration.GetSection(sectionName).Bind(config);
        return config;
    }
}
