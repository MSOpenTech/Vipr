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
    public abstract class BaseStrategy : IStrategy
    {
        protected OdcmModel Model;
        protected Engine Engine;
        protected IConfigArguments Arguments;
        protected string BaseTemplatePath;

        private readonly IReader _reader = new Reader();

        protected BaseStrategy(IConfigArguments configArguments)
        {
            Arguments = configArguments;
            Engine = new Engine();
            BaseTemplatePath = Path.Combine(Arguments.BuilderArguments.TemplatesDir, Name);
        }

        protected virtual void ProcessModelTemplates()
        {
            var templates = Directory.GetFiles(Path.Combine(BaseTemplatePath, "models"));
            ProcessTemplate(templates, Arguments.BuilderArguments.FileExtension, "models");
        }

        protected virtual void ProcessODataTemplates()
        {
            var templates = Directory.GetFiles(Path.Combine(BaseTemplatePath, "odata"));
            ProcessTemplate(templates, Arguments.BuilderArguments.FileExtension, "odata");
        }

        protected virtual void ProcessTemplate(IEnumerable<string> templates, string extension, string path)
        {
            Model = _reader.GenerateOdcmModel(new Dictionary<string, string>
            {
                { "$metadata", Arguments.BuilderArguments.InputFile }
            });

            foreach (var template in templates)
            {
                foreach (var complexType in Model.GetComplexTypes())
                {
                    var host = new CustomHost(Name, complexType) //TODO: v3? How?
                    {
                        TemplateFile = template,
                        Model = Model
                    };

                    var templateContent = File.ReadAllText(host.TemplateFile);
                    var output = Engine.ProcessTemplate(templateContent, host);

                    if (host.Errors != null && host.Errors.HasErrors)
                    {
                        var errors = LogErrors(host);
                        throw new InvalidOperationException(errors);
                    }
                    //CreateFiles(output, extension, path);
                }
                

                //TODO:Better error handling.
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

        protected virtual void CreateFiles(string content, string extension, string path)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (extension == null) throw new ArgumentNullException("extension");
            if (path == null) throw new ArgumentNullException("path");

            var files = content.Split(new[] { "EndOfFile" }, StringSplitOptions.None);

            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file)) return;
                var fileContentenByName = file.Split(new[] { "/n" }, StringSplitOptions.None);

                if (fileContentenByName.Count() == 2)
                {
                    SaveFile(new OutputFileData
                    {
                        Output = fileContentenByName[0],
                        Name = fileContentenByName[1].Trim(),
                        OutputDir = Path.Combine(Arguments.BuilderArguments.OutputDir, path),
                        Extension = extension
                    });
                }
                else
                {
                    SaveFile(new OutputFileData
                    {
                        Output = fileContentenByName[0],
                        Name = "FileName",
                        OutputDir = Path.Combine(Arguments.BuilderArguments.OutputDir, path),
                        Extension = extension
                    });
                }
            }
        }

        protected virtual void SaveFile(OutputFileData fileDtoToSave)
        {
            CreateDirectoryIfItIsNeeded(fileDtoToSave.OutputDir);

            if (!Directory.Exists(fileDtoToSave.OutputDir))
            {
                Directory.CreateDirectory(fileDtoToSave.OutputDir);
            }

            var filePath = Path.Combine(fileDtoToSave.OutputDir, fileDtoToSave.Name + fileDtoToSave.Extension);
            using (var writer = new StreamWriter(filePath, true, Encoding.ASCII))
            {
                writer.Write(fileDtoToSave.Output);
            }
        }

        static void CreateDirectoryIfItIsNeeded(string outputDir)
        {
            var splittedPaths = outputDir.Split('\\');
            var fullPath = string.Empty;

            foreach (var path in splittedPaths)
            {
                fullPath += string.Format("\\{0}", path);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
        }

        public abstract void ProcessTemplates();
        public abstract string Name { get; }
    }
}