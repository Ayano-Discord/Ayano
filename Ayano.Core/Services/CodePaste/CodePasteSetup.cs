using Microsoft.Extensions.DependencyInjection;

namespace Ayano.Core.Services.CodePaste
{
    public static class CodePasteSetup
    {
        public static IServiceCollection AddCodePaste(this IServiceCollection services)
            => services
                .AddSingleton<CodePasteService>()
                .AddSingleton<ICodePasteRepository, MemoryCodePasteRepository>();
    }
}
