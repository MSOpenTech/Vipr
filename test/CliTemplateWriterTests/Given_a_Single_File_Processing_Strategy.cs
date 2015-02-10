using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TemplateWriter;
using Vipr.CLI;
using Vipr.CLI.Output;
using Vipr.CLI.Strategies;
using Vipr.Core;

namespace CliTemplateWriterTests
{
    [TestClass]
    public class Given_a_Single_File_Processing_Strategy
    {

        [TestMethod]
        public void When_Instantiated_should_have_a_valid_state()
        {
            var fileWriter = new Mock<IFileWriter>();
            var reader = new Mock<IReader>();
            var configArguments = new Mock<IConfigArguments>();

            var singleFileStrategy = new SingleFileStrategy(fileWriter.Object, reader.Object, configArguments.Object);

            Assert.IsNotNull(singleFileStrategy);
        }

        [TestMethod]
        public void Can_create_a_representation_of_templates_from_an_assembly()
        {
            var arguments = new Mock<IConfigArguments>();
            var reader = new TemplateAssemblyReader(arguments.Object);

            arguments.SetupGet(x => x.BuilderArguments)
                     .Returns(new BuilderArguments { Language = "Java" });

            reader.Read(typeof(CustomHost));
        }

        [TestMethod]
        public void Can_write_template_into_temp_location()
        {
            var reader = new Mock<ITemplateAssemblyReader>();
            var templateWriter = new TemplateTempLocationFileWriter(reader.Object);
            Assert.IsNotNull(templateWriter);
        }
    }
}