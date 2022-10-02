// using Ayano.Services.Config;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace Ayano.Core.Services.Config;
//
// public static class ConfigurationExtensions
// {
//     /// <summary>
//     /// An extension method to get a <see cref="AyanoConfigurationOptions" /> instance from the
//     /// Configuration by Section Key
//     /// </summary>
//     /// <param name="config">the configuration</param>
//     /// <returns>an instance of the AyanoConfigurationOptions class, or null if not found</returns>
//     public static AyanoConfigurationOptions GetAyanoConfigurationOptions(this IConfiguration config)
//         => config.GetSection(AyanoConfigurationOptions.SectionKey).Get<AyanoConfigurationOptions>()!;
//
//     /// <summary>
//     /// An extension method to add and bind <see cref="AyanoConfigurationOptions" /> to IOptions
//     /// configuration for appSettings.json and UserSecrets configuration.
//     /// </summary>
//     /// <param name="services">the service collection to use for configuration</param>
//     /// <param name="configuration">the configuration used to get the configuration options section from</param>
//     /// <seealso href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options"/>
//     public static IServiceCollection AddAyanoConfigurationOptions(this IServiceCollection services, IConfiguration configuration)
//         => services.Configure<AyanoConfigurationOptions>(configuration.GetSection(AyanoConfigurationOptions.SectionKey));
// }