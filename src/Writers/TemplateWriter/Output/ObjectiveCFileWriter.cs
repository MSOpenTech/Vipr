using System.IO;
using System.Text;
using Vipr.Core.CodeModel;

namespace TemplateWriter.Output
{
	class ObjectiveCFileWriter : BaseFileWriter
	{
		public ObjectiveCFileWriter(OdcmModel model, IConfigArguments configuration) : base(model, configuration)
		{
		}

		public new string FileExtension { get; set; }

		public override void WriteText(Template template, string fileName, string text)
		{
			var destPath = string.Format("{0}{1}", Path.DirectorySeparatorChar, Configuration.BuilderArguments.OutputDir);

			var identifier = FileName(template, fileName);

			FileExtension = template.ResourceName.Contains("header") ? ".h" : ".m";

			var fullPath = Path.Combine(destPath, destPath);

			if (!DirectoryExists(fullPath))
				CreateDirectory(fullPath);

			fullPath = Path.Combine(fullPath, template.FolderName);

			if (!DirectoryExists(fullPath))
				CreateDirectory(fullPath);

			var filePath = Path.Combine(fullPath, string.Format("{0}{1}", identifier, FileExtension));

			using (var writer = new StreamWriter(filePath, false, Encoding.ASCII))
			{
				writer.Write(text);
			}
		}
	}
}	