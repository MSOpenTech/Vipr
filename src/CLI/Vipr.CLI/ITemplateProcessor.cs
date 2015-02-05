using TemplateWriter;

namespace Vipr.CLI
{
    public interface ITemplateProcessor
    {
        void Process(IConfigArguments configArguments);
    }
}