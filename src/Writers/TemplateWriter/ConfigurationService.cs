using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplateWriter {
    public static class ConfigurationService {
        private static TemplateWriterConfiguration _configuration;

        public static void Initialize(TemplateWriterConfiguration configuration) {
            _configuration = configuration;
        }

        public static string PrimaryNamespaceName { get { return _configuration.PrimaryNamespaceName; } }

        public static TemplateWriterConfiguration Configuration {
            get { return _configuration; }
        }
    }
}