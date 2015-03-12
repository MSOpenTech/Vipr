using System.IO;
using Vipr;
using Xunit;

namespace CliTemplateWriterTests
{
    public class Java_Output_Integration_Tests
    {
        [Fact]
        public void When_passing_specific_Arguments_should_procces_exchange_metadata()
        {
            var args = string.Format("Metadata{0}outlook.xml --writer=TemplateWriter", Path.DirectorySeparatorChar).Split(' ');
            var boostraper = new Bootstrapper();
            boostraper.Start(args);
        }
    }
}
