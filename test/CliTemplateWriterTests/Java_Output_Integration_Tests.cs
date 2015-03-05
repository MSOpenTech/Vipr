using Vipr;
using Xunit;

namespace CliTemplateWriterTests
{
    public class Java_Output_Integration_Tests
    {
        [Fact]
        public void When_passing_specific_Arguments_should_procces_exchange_metadata()
        {
            var args = @"C:\Users\developer\src\forks\msot\Vipr\test\CliTemplateWriterTests\Metadata\outlook.xml --writer=TemplateWriter".Split(' ');
            var boostraper = new Bootstrapper();
            boostraper.Start(args);
        }
    }
}
