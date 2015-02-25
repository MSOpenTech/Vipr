using System.Diagnostics;
using Mono.Options;
using TemplateWriter;

namespace Vipr.CLI.Configuration
{
    /// <summary>
    /// This class builds writer and writer instance configs based on command-line parameters.
    /// These configs will be passed to TemplateProcessors to parameterize generation of code.
    /// </summary>
    public class ConfigurationBuilder : IConfigurationBuilder
    {
        private TemplateWriterConfiguration _configuration;
        private OptionSet optionSet;

        public ConfigurationBuilder()
        {
            _configuration = new TemplateWriterConfiguration();

            optionSet = new OptionSet {
                {"h|help", "Shows help", v => _configuration.ShowHelp = v != null},
                {"lang|language=", string.Format("Lang to generate (required). Available langs: {0}", _configuration.AvailableLanguages),
                    v => _configuration.TargetLanguage = v
                },
                {"in|inputFile=", "API metadata file", v => _configuration.InputFile = v},
                {"out|outputDir=", "Directory in which to save the generated files (required).", v => _configuration.OutputDirectory = v},
                {"p|plugins=", "Alternative configurations (optional).", v => _configuration.Plugins = v.Split(',')},
            };
        }

        public IConfigurationBuilder WithArguments(params string[] args)
        {
            optionSet.Parse(args);
            return this;
        }


        public IConfigurationBuilder WithConfiguration(TemplateWriterConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        public IConfigurationBuilder WithJsonConfig()
        {
            return this;
        }

        public IConfigurationBuilder WithDefaultConfig()
        {
            return this;
        }

        public TemplateWriterConfiguration Build()
        {
            _configuration.PrimaryNamespaceName = _configuration.PrimaryNamespaceName ?? TemplateWriterConfiguration.Default.PrimaryNamespaceName;
            _configuration.NamespacePrefix = _configuration.NamespacePrefix ?? TemplateWriterConfiguration.Default.NamespacePrefix;
            return _configuration;
        }
    }
}