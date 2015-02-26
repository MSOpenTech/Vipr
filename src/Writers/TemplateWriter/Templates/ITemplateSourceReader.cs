using System;
using System.Collections.Generic;

namespace TemplateWriter.Templates
{
    public interface ITemplateSourceReader
    {
        IList<Template> Read(Type targetType, TemplateWriterConfiguration config);
    }
}