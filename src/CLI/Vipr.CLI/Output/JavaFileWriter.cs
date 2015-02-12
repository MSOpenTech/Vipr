using System.IO;
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
            var @namespace = _model.GetNamespace() + "." + template.FolderName;
            var pathFromNamespace = CreatePathFromNamespace(@namespace);
            
            var identifier = FileName(template, fileName);

            var fullPath = Path.Combine(destPath, pathFromNamespace);
            var filePath = Path.Combine(fullPath, string.Format("{0}{1}", identifier, _configuration.BuilderArguments.FileExtension));

            using (var writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                writer.Write(text);
            }
        }

        private string CreatePathFromNamespace(string @namespace)
        {
            var splittedPaths = @namespace.Split('.');
            var fullPath = string.Empty;

            foreach (var path in splittedPaths)
            {
                fullPath += string.Format("{0}{1}", Path.DirectorySeparatorChar, path);
                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                }
            }
            return fullPath;
        }

        /// <summary>
        /// Creates a directory structure based on the given parameter
        /// </summary>
        /// <param name="directoryPath"></param>
        public void CreateDirectory(string directoryPath)
        {
            var splittedPaths = directoryPath.Split('\\');
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

        public bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
    }
}
