using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;
using TemplateWriter;
using Vipr.CLI.Output;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Strategies
{
    public class JavaTemplateProcessor
    {
        public const string ComplexType = "ComplexType";
        public const string EntityType = "EntityType";
        public const String JavaStrategyName = "Java";

        private readonly IFileWriter _fileWriter;
        private readonly IConfigArguments _arguments;
        private readonly Engine _engine;
        private readonly OdcmModel _model;

        public Dictionary<string, Action<string>> Templates { get; set; }

        public string Name
        {
            get { return JavaStrategyName; }
        }

        public JavaTemplateProcessor(OdcmModel model, IFileWriter fileWriter, IConfigArguments arguments)
        {
            _model = model;
            _fileWriter = fileWriter;
            _arguments = arguments;
            _engine = new Engine();

            Templates = new Dictionary<string, Action<string>>(StringComparer.InvariantCultureIgnoreCase)
            {
                {ComplexType, ComplexTypes},
                {EntityType, EntityTypes}
            };
        }

        public Action<string> GetStrategy(string templateName)
        {
            Action<string> action;
            Templates.TryGetValue(templateName, out action);
            Debug.Assert(action != null, "strategy != null");
            return action;
        }

        public void ComplexTypes(string templateFile)
        {
            var complexTypes = _model.GetComplexTypes();
            ProcessingAction(complexTypes, templateFile);
        }

        public void EntityTypes(string templateFile)
        {
            var entityTypes = _model.GetEntityTypes();
            ProcessingAction(entityTypes, templateFile);
        }

        public void ProcessingAction(IEnumerable<OdcmObject> source, string templateFile)
        {
            foreach (var complexType in source)
            {
                var host = new CustomHost(JavaStrategyName, complexType) //TODO: v3? How?
                {
                    TemplateFile = templateFile,
                };

                var templateContent = File.ReadAllText(host.TemplateFile);
                var output = _engine.ProcessTemplate(templateContent, host);

                if (host.Errors != null && host.Errors.HasErrors)
                {
                    var errors = LogErrors(host);
                    throw new InvalidOperationException(errors);
                }
                _fileWriter.WriteText(_arguments.BuilderArguments.OutputDir, output);
            }
        }

        protected static string LogErrors(CustomHost host)
        {
            var sb = new StringBuilder();
            if (host.Errors == null || host.Errors.Count <= 0) return sb.ToString();
            foreach (CompilerError error in host.Errors)
            {
                sb.AppendLine("Error template" + host.TemplateFile);
                sb.AppendLine(error.ErrorText);
                sb.AppendLine("In line: " + error.Line);
                sb.AppendLine(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
