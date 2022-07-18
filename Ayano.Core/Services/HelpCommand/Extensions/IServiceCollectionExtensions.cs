using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Remora.Commands.Extensions;
using Remora.Discord.Commands.Extensions;
using Ayano.Services.HelpCommand.Services;

namespace Ayano.Services.HelpCommand.Extensions;

[ExcludeFromCodeCoverage]
public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddHelpSystem(this IServiceCollection services, string? treeName = null,
        bool addHelpCommand = true)
    {
        services.Configure<HelpSystemOptions>(o => o.TreeName = treeName);

        if (addHelpCommand)
            services
                .AddDiscordCommands()
                .AddCommandTree(treeName)
                .WithCommandGroup<HelpCommand>()
                .Finish();

        services.TryAddScoped<TreeWalker>();

        services.TryAddScoped<IHelpFormatter, DefaultHelpFormatter>();
        services.TryAddScoped<ICommandHelpService, CommandHelpService>();

        return services;
    }
}