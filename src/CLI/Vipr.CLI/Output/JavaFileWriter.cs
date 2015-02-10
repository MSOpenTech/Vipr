using System.IO;
using System.Text;

namespace Vipr.CLI.Output
{
    class JavaFileWriter : IFileWriter
    {
        public void WriteText(string filePath, string text)
        {
            using (var writer = new StreamWriter(filePath, true, Encoding.ASCII))
            {
                writer.Write(filePath);
            }
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
