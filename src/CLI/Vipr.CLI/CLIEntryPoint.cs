using TemplateWriter;
using TemplateWriter.TemplateProcessors;

namespace Vipr.CLI
{
    public class CLIEntryPoint
    {
        private readonly TemplateWriterConfiguration _config;
        private readonly ITemplateProcessorManager _processor;

        public CLIEntryPoint(TemplateWriterConfiguration config, ITemplateProcessorManager processor)
        {
            _config = config;
            _processor = processor;
        }

        public void Process()
        {
            _processor.Process(_config);
        }
    }
}