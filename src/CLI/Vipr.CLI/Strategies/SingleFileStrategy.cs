using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ODataReader.v4;
using TemplateWriter;
using Vipr.CLI.Output;
using Vipr.Core;

namespace Vipr.CLI.Strategies
{
    public class SingleFileStrategy : IStrategy
    {
        private readonly IReader _reader;
        private readonly IConfigArguments _arguments;
        private readonly TemplateTempLocationFileWriter _templateTempLocationFileWriter;

        public SingleFileStrategy(IConfigArguments arguments)
            : this(new Reader(), arguments)
        {
            _templateTempLocationFileWriter = new TemplateTempLocationFileWriter(new TemplateAssemblyReader(arguments));
        }

        public SingleFileStrategy(IReader reader, IConfigArguments arguments)
        {
            _reader = reader;
            _arguments = arguments;
        }

        public void ProcessTemplates()
        {
            var runnableTemplates = _templateTempLocationFileWriter.WriteUsing(typeof(CustomHost))
                                                                   .Where(x => !x.IsBase);

            var model = _reader.GenerateOdcmModel(new Dictionary<string, string>
            {
                { "$metadata", File.ReadAllText(_arguments.BuilderArguments.InputFile) }
            });

            var javaProcessor = new JavaTemplateProcessor(model, new JavaFileWriter(), _arguments);

            foreach (var template in runnableTemplates)
            {
                Action<string> action;
                if (javaProcessor.Templates.TryGetValue(template.Name, out action))
                {
                    action(template.Path);
                }
            }
        }

        public string Name
        {
            get { return JavaStrategyName; }
        }

        public const String JavaStrategyName = "Java";
    }
}