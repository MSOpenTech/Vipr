using TemplateWriter;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Output
{
    public interface IFileWriter
    {
        void WriteText(Template template, string odcmObject, string output);

        void CreateDirectory(string directoryPath);

        bool DirectoryExists(string directoryPath);
    }

    class NullWriter : IFileWriter
    {
        private readonly OdcmModel _model;
        private readonly IConfigArguments _configuration;

        public NullWriter(OdcmModel model, IConfigArguments configuration)
        {
            _model = model;
            _configuration = configuration;
        }

        public void WriteText(Template template, string odcmObject, string output)
        {
        }

        public void CreateDirectory(string directoryPath)
        {
        }

        public bool DirectoryExists(string directoryPath)
        {
            return false;
        }
    }
}