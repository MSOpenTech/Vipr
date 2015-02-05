using System;
using System.IO;
using TemplateWriter;

namespace Vipr.CLI
{
    public class JavaStrategy : BaseStrategy, IStrategy
    {
        private readonly IConfigArguments _configArguments;

        public JavaStrategy(IConfigArguments configArguments)
            : base(configArguments)
        {
            _configArguments = configArguments;
        }

        protected override void ProcessModelTemplates()
        {
            var templates = Directory.GetFiles(Path.Combine(BaseTemplatePath, "models"));
            ProcessTemplate(templates, _configArguments.BuilderArguments.FileExtension,
                                       _configArguments.TemplateConfiguration.PrimaryNamespaceName.Replace(".", "\\") + "\\models");
        }

        protected override void ProcessODataTemplates()
        {
            var templates = Directory.GetFiles(Path.Combine(BaseTemplatePath, "odata"));
            ProcessTemplate(templates, _configArguments.BuilderArguments.FileExtension,
                                       _configArguments.TemplateConfiguration.PrimaryNamespaceName.Replace(".", "\\") + "\\odata");
        }

        public override void ProcessTemplates()
        {
            ProcessModelTemplates();
            ProcessODataTemplates();
        }


        public override string Name
        {
            get { return JavaStrategyName; }
        }

        public const String JavaStrategyName = "Java";
    }
}