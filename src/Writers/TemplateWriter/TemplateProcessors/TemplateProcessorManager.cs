using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ODataReader.v4;
using TemplateWriter.Output;
using TemplateWriter.Strategies;
using Vipr.Core;
using Vipr.Core.CodeModel;

namespace TemplateWriter
{
    public class TemplateProcessorManager : ITemplateProcessorManager
    {
        private readonly IOdcmReader _reader;  // TODO: should be in main CLI, not in writer
        private readonly ITemplateTempLocationFileWriter _tempLocationFileWriter;
        private readonly Dictionary<string, Func<OdcmModel, TemplateWriterConfiguration, string /* path to base template */, ITemplateProcessor>> _processors;

        public TemplateProcessorManager()
            : this(new OdcmReader(), new TemplateTempLocationFileWriter(new TemplateSourceReader()))
        {
        }

        public TemplateProcessorManager(IOdcmReader reader, ITemplateTempLocationFileWriter tempLocationFileWriter)
        {
            _reader = reader;
            _tempLocationFileWriter = tempLocationFileWriter;
            _processors = new Dictionary<string, Func<OdcmModel, TemplateWriterConfiguration, string, ITemplateProcessor>>
            {
                {"java", (model, config, baseFilePath) => 
                    new JavaTemplateProcessor(new JavaFileWriter(model, config), model, baseFilePath)},
                {"objectivec", (model, config ,baseFilePath) =>
					new ObjectiveCTemplateProcessor(new ObjectiveCFileWriter(model, config), model, baseFilePath )}
            };
        }

        public void Process(TemplateWriterConfiguration configuration)
        {
            ConfigurationService.Initialize(configuration);

            var runnableTemplates = _tempLocationFileWriter.WriteUsing(typeof(CustomHost), configuration)
                                                           .Where(x => !x.IsBase &&
                                                                        x.IsForLanguage(configuration.TargetLanguage));

            var baseTemplate = _tempLocationFileWriter.WriteUsing(typeof(CustomHost), configuration)
                                                           .Single(x => x.IsBase && x.IsForLanguage(configuration.TargetLanguage));

            //TODO: model should come from CLI
            var model = _reader.GenerateOdcmModel(new Dictionary<string, string>
            {
                { "$metadata", File.ReadAllText(configuration.InputFile) }
            });

            var processor = _processors[configuration.TargetLanguage]
                                .Invoke(model, configuration, baseTemplate.Path);

            foreach (var template in runnableTemplates)
            {
                Action<Template> action;
                if (processor.Templates.TryGetValue(template.Name, out action))
                {
                    action(template);
                }
            }
        }
    }
}