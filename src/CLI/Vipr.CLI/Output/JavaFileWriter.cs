using System.IO;
using System.Text;
using TemplateWriter;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Output
{
    public class JavaFileWriter : BaseFileWriter
    {
        public JavaFileWriter(OdcmModel model, IConfigArguments configuration) : base(model,configuration)
        {
        }

        public override void WriteText(Template template, string fileName, string text)
        {
            var destPath = string.Format("{0}{1}", Path.DirectorySeparatorChar, Configuration.BuilderArguments.OutputDir);
            var @namespace = Model.GetNamespace() + "." + template.FolderName;
            var pathFromNamespace = CreatePathFromNamespace(@namespace);
            
            var identifier = FileName(template, fileName);

            var fullPath = Path.Combine(destPath, pathFromNamespace);
            var filePath = Path.Combine(fullPath, string.Format("{0}{1}", identifier, Configuration.BuilderArguments.FileExtension));

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
    }
}