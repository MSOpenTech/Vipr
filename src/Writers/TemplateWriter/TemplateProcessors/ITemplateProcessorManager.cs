using TemplateWriter.Settings;

namespace TemplateWriter.TemplateProcessors
{
    public interface ITemplateProcessorManager
    {
        void Process(TemplateWriterSettings configuration);
    }
}