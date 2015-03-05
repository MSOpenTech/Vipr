using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vipr.Core;
using TemplateWriter.Settings;

namespace TemplateWriter
{
    public static class ConfigurationService
    {
        private static IConfigurationProvider s_configurationProvider;

        public static void Initialize(IConfigurationProvider configuration)
        {
            s_configurationProvider = configuration;
        }

        public static TemplateWriterSettings Settings
        {
            get
            {
                return s_configurationProvider != null
                    ? s_configurationProvider.GetConfiguration<TemplateWriterSettings>()
                    : new TemplateWriterSettings();
            } 
        }
    }
}