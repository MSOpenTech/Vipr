using System.IO;
using System.Linq;
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
			var @namespace = CreateNamespace(template.FolderName).ToLower();
            var pathFromNamespace = CreatePathFromNamespace(@namespace);
            
            var identifier = FileName(template, fileName);

            var fullPath = Path.Combine(destPath, pathFromNamespace);
            var filePath = Path.Combine(fullPath, string.Format("{0}{1}", identifier, Configuration.BuilderArguments.FileExtension));

            using (var writer = new StreamWriter(filePath, false, Encoding.ASCII))
            {
                writer.Write(text);
            }
        }

		private string CreateNamespace(string folderName)
		{
			var @namespace = Model.GetNamespace();
			var prefix = Configuration.TemplateConfiguration.NamespacePrefix;

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
    }
}