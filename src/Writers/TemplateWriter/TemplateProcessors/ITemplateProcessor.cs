using System;
using System.Collections.Generic;
using TemplateWriter.Templates;

namespace TemplateWriter.TemplateProcessors
{
    interface ITemplateProcessor
    {
        Dictionary<string, Action<Template>> Templates { get; set; }
    }
}
