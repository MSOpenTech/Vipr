using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TemplateWriter;
using Vipr.CLI;

namespace CliTemplateWriterTests
{
    [TestClass]
    public class Given_a_Template_Processor
    {
        [TestMethod]
        public void When_calling_Process_on_Processor()
        {
            var strategy = new Mock<IStrategy>();
            var registry = new Mock<IStrategyResgistry>();
            var arguments = new Mock<IConfigArguments>();
            registry.Setup(x => x.GetStrategy(arguments.Object)).Returns(strategy.Object);

            var templateProcessor = new TemplateProcessor(registry.Object);
            templateProcessor.Process(arguments.Object);

            registry.Verify(x => x.GetStrategy(arguments.Object), Times.Once());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void When_calling_Process_on_Process_should_throw_exception_on_null_strategy()
        {
            var registry = new Mock<IStrategyResgistry>();
            var arguments = new Mock<IConfigArguments>();
            registry.Setup(x => x.GetStrategy(arguments.Object));

            var templateProcessor = new TemplateProcessor(registry.Object);
            templateProcessor.Process(arguments.Object);

            registry.Verify(x => x.GetStrategy(arguments.Object), Times.Once());
        }
    }
}
