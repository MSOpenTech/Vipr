using Vipr.Core.CodeModel;

namespace Vipr.CLI.Output
{
    public interface IFileWriter
    {
        void WriteText(string odcmObject, string output);

        void CreateDirectory(string directoryPath);

        bool DirectoryExists(string directoryPath);
    }
}