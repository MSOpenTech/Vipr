namespace Vipr.CLI.Output
{
    public interface IFileWriter
    {
        void WriteText(string filePath, string text);

        void CreateDirectory(string directoryPath);

        bool DirectoryExists(string directoryPath);
    }
}