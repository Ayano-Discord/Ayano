using Ayano.Plugins.Dev;
using Ayano.Plugins.Dev.Commands;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;

[assembly: RemoraPlugin(typeof(DevPlugin))]

namespace Ayano.Plugins.Dev;

public sealed class DevPlugin : PluginDescriptor
{
    public override string Name => "Fun";
    
    public override string Description => "Contains fun commands for Ayano.";

    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddCommandTree()
            .WithCommandGroup<EvalCommand>();

        return Result.FromSuccess();
    }
}