using System;
using System.Collections.Generic;

namespace TemplateWriter.Templates
{
    public interface ITemplateTempLocationFileWriter
    {
        IList<Template> WriteUsing(Type sourceType, TemplateWriterConfiguration config);
    }
}