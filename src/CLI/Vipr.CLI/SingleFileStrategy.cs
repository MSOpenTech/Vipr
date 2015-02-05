using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TextTemplating;
using ODataReader.v4;
using TemplateWriter;
using Vipr.Core;
using Vipr.Core.CodeModel;

namespace Vipr.CLI
{
    public class SingleFileStrategy : IStrategy
    {
        private readonly Engine _engine;
        private readonly IFileWriter _fileWriter;
        private readonly IReader _reader;
        private readonly IConfigArguments _arguments;

        public IDictionary<string, Action<string>> Templates;

        public SingleFileStrategy(IConfigArguments arguments)
            : this(new JavaFileWriter(), new Reader(), arguments)
        {

        }

        public SingleFileStrategy(IFileWriter fileWriter, IReader reader,
                                  IConfigArguments arguments)
        {
            _fileWriter = fileWriter;
            _reader = reader;
            _arguments = arguments;
            _engine = new Engine();

            BaseTemplatePath = Path.Combine(_arguments.BuilderArguments.TemplatesDir, Name);

            Templates = new Dictionary<string, Action<string>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"ComplexType", ComplexTypes}
            };

            Model = _reader.GenerateOdcmModel(new Dictionary<string, string>
            {
                { "$metadata", _arguments.BuilderArguments.InputFile }
            });
        }

        public string BaseTemplatePath { get; set; }

        public OdcmModel Model { get; set; }

        public void ComplexTypes(string path)
        {
            var complexTypes = Model.GetComplexTypes();
            foreach (var complexType in complexTypes)
            {
                var host = new CustomHost(Name, complexType) //TODO: v3? How?
                {
                    TemplateFile = "",
                };

                var templateContent = File.ReadAllText(host.TemplateFile);
                var output = _engine.ProcessTemplate(templateContent, host);
                _fileWriter.Write(output);
            }
        }

        public void ProcessTemplates()
        {
            var resources = typeof (CustomHost).Assembly.GetManifestResourceNames();
            var templateNames = resources.Select(x =>
            {
                var splits = x.Split('.');
                return splits.ElementAt(splits.Count() - 2);
            });


            foreach (var template in templateNames)
            {
                if (Templates.Keys.Contains(template))
                {
                    ComplexTypes(template);
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