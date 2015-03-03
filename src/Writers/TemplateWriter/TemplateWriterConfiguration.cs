using System.Collections.Generic;

namespace TemplateWriter
{
    public class TemplateWriterConfiguration 
    {
        public static TemplateWriterConfiguration Default = new TemplateWriterConfiguration
        {
            AvailableLanguages = new HashSet<string> { "java", "objectivec" },
            PrimaryNamespaceName = "some.namespace.here",
            NamespacePrefix = "com"
        };

        /// <summary>
        /// Target languages provided via templates.
        /// </summary>
        public HashSet<string> AvailableLanguages { get; set; }

        public string PrimaryNamespaceName { get; set; }

        public string NamespacePrefix { get; set; }

        public IReadOnlyDictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// The code language to be targeted by this template writer instance.
        /// </summary>
        public string TargetLanguage { get; set; }

        public string InputFile { get; set; }

        /// <summary>
        /// Where to write output source code files.
        /// </summary>
        public string OutputDirectory { get; set; }

        public string[] Plugins { get; set; }

        public bool ShowHelp { get; set; }
    }
}
