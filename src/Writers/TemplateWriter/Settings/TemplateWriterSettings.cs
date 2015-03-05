using System.Collections.Generic;

namespace TemplateWriter.Settings
{
    public class TemplateWriterSettings 
    {
        public static TemplateWriterSettings Default = new TemplateWriterSettings
        {
            AvailableLanguages = new List<string> { "java", "objectivec" },

        };

        /// <summary>
        /// Target languages provided via templates.
        /// </summary>
        public IList<string> AvailableLanguages { get; set; }

        /// <summary>
        /// The code language to be targeted by this template writer instance.
        /// </summary>
        public string TargetLanguage { get; set; }

        public string PrimaryNamespaceName { get; set; }

        public string NamespacePrefix { get; set; }

        // TODO: Remove and rely on CLI
        public string OutputDirectory { get; set; }

    }
}
