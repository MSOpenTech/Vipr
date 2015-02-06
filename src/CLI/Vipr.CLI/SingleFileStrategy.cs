using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
                { "$metadata", File.ReadAllText(_arguments.BuilderArguments.InputFile) }
            });
        }

        public string BaseTemplatePath { get; set; }

        public OdcmModel Model { get; set; }

        public void ComplexTypes(string templateFilePath)
        {
            var complexTypes = Model.GetComplexTypes();

            foreach (var complexType in complexTypes)
            {
                var host = new CustomHost(Name, complexType) //TODO: v3? How?
                {
                    TemplateFile = templateFilePath,
                };

                var templateContent = File.ReadAllText(host.TemplateFile);
                var output = _engine.ProcessTemplate(templateContent, host);

                if (host.Errors != null && host.Errors.HasErrors)
                {
                    var errors = LogErrors(host);
                    throw new InvalidOperationException(errors);
                }
                _fileWriter.Write(output);
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

        public void ProcessTemplates()
        {
            var resources = typeof(CustomHost).Assembly.GetManifestResourceNames();
            var templateNames = resources.Select(x =>
            {
                var splits = x.Split('.');
                return new Tuple<string, string>(x, splits.ElementAt(splits.Count() - 2));
            });

            var dict = new Dictionary<string, string>();

            foreach (var templateName in templateNames)
            {
                var path = GetTempFilePathWithExtension(templateName);
                dict.Add(templateName.Item2, path);

                Action<string> action;
                if (Templates.TryGetValue(templateName.Item2, out action))
                {
                    string s;
                    dict.TryGetValue(templateName.Item2, out s);
                    action(s);
                }
            }
        }

        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        public string GetTempFilePathWithExtension(Tuple<string, string> templateName)
        {
            var fullpath = CreatePath(templateName.Item2 + ".tt");
            CreateTempTemplateFile(templateName.Item1, fullpath);
            return fullpath;
        }

        private void CreateTempTemplateFile(string resourceName, string fullpath)
        {
            if (!File.Exists(fullpath))
            {
                File.Delete(fullpath);
            }

            using (var stream = typeof(CustomHost).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    CopyStream(stream, fullpath);
                }
            }
        }

        private static string CreatePath(string fileName)
        {
            var path = Path.GetTempPath();
            var fullpath = Path.Combine(path, fileName);
            return fullpath;
        }

        public string Name
        {
            get { return JavaStrategyName; }
        }

        public const String JavaStrategyName = "Java";
    }
}