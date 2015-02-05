using System;
using System.Diagnostics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TemplateWriter;
using Vipr.CLI;

namespace CliTemplateWriterTests
{
    [TestClass]
    public class Given_a_Single_File_Processing_Strategy
    {

        [TestMethod]
        public void CanCreateSingleFileProcessing()
        {
            var fileWriter = new Mock<IFileWriter>();
            var configArguments = new Mock<IConfigArguments>();
            var singleFileStrategy = new SingleFileStrategy(configArguments.Object);
            singleFileStrategy.ProcessTemplates();
        }
    }
}