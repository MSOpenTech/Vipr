using System.IO;
using System.Linq;
using System.Text;
using TemplateWriter;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Output
{
    class JavaFileWriter : IFileWriter
    {
        private readonly OdcmModel _model;
        private readonly IConfigArguments _configuration;

        public JavaFileWriter(OdcmModel model, IConfigArguments configuration)
        {
            _model = model;
            _configuration = configuration;
        }

        private static string FileName(Template template, string identifier)
        {
            return template.FolderName == "odata" ? template.Name.Replace("Entity", identifier)
                                                  : identifier;
        }

        public void WriteText(Template template, string fileName, string text)
        {
            var destPath = string.Format("{0}{1}", Path.DirectorySeparatorChar, _configuration.BuilderArguments.OutputDir);
            var @namespace = CreateNamespace(template.FolderName).ToLower();
            var pathFromNamespace = CreatePathFromNamespace(@namespace);

            var identifier = FileName(template, fileName);

            var fullPath = Path.Combine(destPath, pathFromNamespace);
            var filePath = Path.Combine(fullPath, string.Format("{0}{1}", identifier, _configuration.BuilderArguments.FileExtension));

            using (var writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                writer.Write(text);
            }
        }

        private string CreateNamespace(string folderName)
        {
            var @namespace = _model.GetNamespace();
            var prefix = _configuration.TemplateConfiguration.NamespacePrefix;
            return string.IsNullOrEmpty(prefix) ? string.Format("{0}.{1}", @namespace, folderName)
                                                : string.Format("{0}.{1}.{2}", prefix, @namespace, folderName);
        }

        private string CreatePathFromNamespace(string @namespace)
        {
            var splittedPaths = @namespace.Split('.');
            var destinationPath = splittedPaths.Aggregate(string.Empty, (current, path) =>
                current + string.Format("{0}{1}", Path.DirectorySeparatorChar, path));

            if (!DirectoryExists(destinationPath))
            {
                CreateDirectory(destinationPath);
            }
            return destinationPath;
        }

        public void CreateDirectory(string directoryPath)
        {
            Directory.CreateDirectory(directoryPath);
        }

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
    }
}
