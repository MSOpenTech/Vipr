﻿using TemplateWriter.Output;
using Vipr.Core.CodeModel;
using TemplateWriter.Settings;

namespace TemplateWriter.TemplateProcessors
{
    public class JavaTemplateProcessor : BaseTemplateProcessor
    {
        public JavaTemplateProcessor(IFileWriter fileWriter, OdcmModel model, string baseFilePath) : base(fileWriter, model, baseFilePath)
        {
            StrategyName = "Java";
        }
    }
}