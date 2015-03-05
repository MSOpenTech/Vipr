using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TemplateWriter.Settings
{
    public class TemplateWriterSettings 
    {
        //TODO: Differentiate between Java and Obj-C
        public TemplateWriterSettings() {
            // defaults
            AvailableLanguages = new List<string> { "java", "objectivec" };
            PrimaryNamespaceName = "";
            NamespacePrefix = "com";
            Plugins = new List<string>();
            OutputDirectory = @"c:\VIPR.Output";
        }

        /// <summary>
        /// Target languages provided via templates.
        /// </summary>
        public IList<string> AvailableLanguages { get; set; }

        /// <summary>
        /// The code language to be targeted by this template writer instance.
        /// </summary>
        public string TargetLanguage { get; set; }
		
		public IList<string> Plugins { get; set; }

        public string PrimaryNamespaceName { get; set; }

        public string NamespacePrefix { get; set; }

        // TODO: Remove and rely on CLI
        public string OutputDirectory { get; set; }
    }
}
