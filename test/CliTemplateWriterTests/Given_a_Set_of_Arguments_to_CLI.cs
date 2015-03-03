using System;
using Moq;
using TemplateWriter;
using TemplateWriter.TemplateProcessors;
using Vipr.CLI;
using Vipr.CLI.Configuration;
using Xunit;

namespace CliTemplateWriterTests
{
    public class Given_a_Set_of_Arguments_to_CLI
    {
        [Fact]
        public void When_the_CLI_receives_a_set_of_arguments()
        {
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processorManager = new Mock<ITemplateProcessorManager>();

            configBuilder.Setup(x => x.WithArguments(It.IsAny<string>()));
            configBuilder.Setup(x => x.WithJsonConfig());

            var entryPoint = new CLIEntryPoint(processorManager.Object, new TemplateWriterConfiguration());
            Assert.NotNull(entryPoint);
        }

        [Fact]
        public void When_the_CLI_has_arguments_should_call_processor()
        {
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processorManager = new Mock<ITemplateProcessorManager>();

            var entryPoint = new CLIEntryPoint(processorManager.Object, new TemplateWriterConfiguration());
            entryPoint.Process();

            configBuilder.VerifyAll();
            processorManager.VerifyAll();
        }

        [Fact]
        public void When_the_CLI_has_no_arguments_should_throw_exception()
        {
            var configBuilder = new Mock<IConfigurationBuilder>();
            var processorManager = new Mock<ITemplateProcessorManager>();

            configBuilder.Setup(x => x.Build());

            var entryPoint = new CLIEntryPoint(processorManager.Object, new TemplateWriterConfiguration());
            Assert.Throws<InvalidOperationException>(() => entryPoint.Process());
        }


        [Fact]
        public void When_passing_specific_Arguments_should_procces_templates_objc()
        {
            var args = "--language=objectivec --inputFile=Metadata\\Exchange.edmx.xml --outputDir=Out".Split(' ');
            var builder = new ConfigurationBuilder().WithArguments(args);
            var entrypoint = new CLIEntryPoint(new TemplateProcessorManager(), new TemplateWriterConfiguration());
            entrypoint.Process();
        }

        [Fact]
        public void When_passing_specific_Arguments_should_procces_files_templates_objc()
        {
            var args = "--language=objectivec --inputFile=Metadata\\files.xml --outputDir=Out".Split(' ');
            var entrypoint = new CLIEntryPoint(new TemplateProcessorManager(), new TemplateWriterConfiguration());
            entrypoint.Process();
        }
    }
}
