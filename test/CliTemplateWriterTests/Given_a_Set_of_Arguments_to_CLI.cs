using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TemplateWriter;
using Vipr.CLI;
using Vipr.CLI.Configuration;

namespace CliTemplateWriterTests
{
    [TestClass]
    public class Given_a_Set_of_Arguments_to_CLI
    {
        [TestMethod]
        public void When_the_CLI_receives_a_set_of_arguments()
        {
            var configArguments = new Mock<IConfigArguments>();
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processor = new Mock<ITemplateProcessor>();

            configBuilder.Setup(x => x.WithArguments(It.IsAny<string>()));
            configBuilder.Setup(x => x.WithJsonConfig());
            configBuilder.Setup(x => x.Build()).Returns(configArguments.Object);

            new CLIEntryPoint(configBuilder.Object, processor.Object).Should().NotBe(null);
        }

        [TestMethod]
        public void When_the_CLI_has_arguments_should_call_processor()
        {
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processor = new Mock<ITemplateProcessor>();
            var configArguments = new Mock<IConfigArguments>();

            configBuilder.Setup(x => x.Build())
                         .Returns(configArguments.Object);
            processor.Setup(x => x.Process(configArguments.Object));

            var entryPoint = new CLIEntryPoint(configBuilder.Object, processor.Object);
            entryPoint.Process();

            configBuilder.VerifyAll();
            processor.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_the_CLI_has_no_arguments_should_throw_exception()
        {
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processor = new Mock<ITemplateProcessor>();
            var configArguments = new Mock<IConfigArguments>();

            configBuilder.Setup(x => x.Build());
            processor.Setup(x => x.Process(configArguments.Object));

            var entryPoint = new CLIEntryPoint(configBuilder.Object, processor.Object);
            entryPoint.Process();

            configBuilder.VerifyAll();
            processor.VerifyAll();
        }

        [TestMethod]
        public void When_passing_specific_Arguments_should_procces_templates()
        {
            var args = "--language=java --inputFile=Metadata\\Exchange.edmx.xml --outputDir=Out".Split(' ');
            var builder = new ConfigurationBuilder().WithArguments(args);
            var entrypoint = new CLIEntryPoint(builder, new TemplateProcessor());
            entrypoint.Process();
        }
    }
}
