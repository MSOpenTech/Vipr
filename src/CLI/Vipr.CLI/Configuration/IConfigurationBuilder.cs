using Mono.Options;
using TemplateWriter;

namespace Vipr.CLI.Configuration
{
    public interface IConfigurationBuilder
    {
        TemplateWriterConfiguration Build();
        IConfigurationBuilder WithArguments(params string[] args);
        IConfigurationBuilder WithJsonConfig();
    }
}