using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ODataReader.v4;
using TemplateWriter;
using Vipr.CLI.Output;
using Vipr.CLI.Strategies;
using Vipr.Core;

namespace Vipr.CLI
{
    public class TemplateProcessorManager : ITemplateProcessorManager
    {
        private readonly IReader _reader;
        private readonly ITemplateTempLocationFileWriter _tempLocationFileWriter;

        public TemplateProcessorManager()
            : this(new Reader(), new TemplateTempLocationFileWriter(new TemplateSourceReader()))
        {
        }

        public TemplateProcessorManager(IReader reader, ITemplateTempLocationFileWriter tempLocationFileWriter)
        {
            _reader = reader;
            _tempLocationFileWriter = tempLocationFileWriter;
        }

        public void Process(IConfigArguments configuration)
        {
            var runnableTemplates = _tempLocationFileWriter.WriteUsing(typeof(CustomHost), configuration.BuilderArguments)
                                                                   .Where(x => !x.IsBase);

            var model = _reader.GenerateOdcmModel(new Dictionary<string, string>
            {
                { "$metadata", File.ReadAllText(configuration.BuilderArguments.InputFile) }
            });

            var javaProcessor = new JavaTemplateProcessor(new JavaFileWriter(model, configuration), model, configuration);

            foreach (var template in runnableTemplates)
            {
                Action<string> action;
                if (javaProcessor.Templates.TryGetValue(template.Name, out action))
                {
                    action(template.Path);
                }
            }
        }
    }
}