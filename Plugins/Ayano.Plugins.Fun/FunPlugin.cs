using Ayano.Core.Services.CodePaste;
using Ayano.Plugins.Fun;
using Ayano.Plugins.Fun.Commands;
using Microsoft.Extensions.DependencyInjection;
using Remora.Commands.Extensions;
using Remora.Plugins.Abstractions;
using Remora.Plugins.Abstractions.Attributes;
using RemoraResult = Remora.Results.Result;
using xFunc.Maths;

[assembly: RemoraPlugin(typeof(FunPlugin))]

namespace Ayano.Plugins.Fun;

public sealed class FunPlugin : PluginDescriptor
{
    public override string Name => "Fun";
    
    public override string Description => "Contains fun commands for Ayano.";

    public override RemoraResult ConfigureServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<CodePasteService>();
        serviceCollection.AddSingleton(new Processor());

        serviceCollection.AddCommandTree()
            .WithCommandGroup<TranslateCommand>()
            .WithCommandGroup<HackCommand>()
            .WithCommandGroup<ReplModule>()
            .WithCommandGroup<MathsCommands>();

        return RemoraResult.FromSuccess();
    }
}