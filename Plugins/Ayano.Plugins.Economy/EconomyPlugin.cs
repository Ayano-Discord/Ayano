using Ayano.Plugins.Economy;
using Ayano.Plugins.Economy.Commands;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using Remora.Results;
using Ayano.Core.Services;

[assembly: RemoraPlugin(typeof(EconomyPlugin))]

namespace Ayano.Plugins.Economy;

public sealed class EconomyPlugin : PluginDescriptor
{
    public override string Name => "Economy";

    public override string Description => "Commands to make some money.";

    public override Result ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<Database>();

        serviceCollection.AddCommandTree()
            .WithCommandGroup<WorkCommand>()
            .WithCommandGroup<BalanceCommand>();

        return Result.FromSuccess();
    }
}