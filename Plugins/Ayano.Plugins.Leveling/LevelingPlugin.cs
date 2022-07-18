using Ayano.Plugins.Leveling;
using Ayano.Plugins.Leveling.Commands;
using Ayano.Plugins.Leveling.Services.ProfileService;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly: RemoraPlugin(typeof(LevelingPlugin))]

namespace Ayano.Plugins.Leveling;

public sealed class LevelingPlugin : PluginDescriptor
{
    public override string Name => "Leveling";
    
    public override string Description => "Contains Leveling related commands for Ayan.";

    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<HttpClientHelper>();
        serviceCollection.AddSingleton<ImageGenerator>();
        
        serviceCollection.AddCommandTree()
            .WithCommandGroup<ProfileSet>()
            .WithCommandGroup<ProfileCommand>();

        return Result.FromSuccess();
    }
}