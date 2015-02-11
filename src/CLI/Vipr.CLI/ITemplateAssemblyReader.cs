using System;
using System.Collections.Generic;
using TemplateWriter;

namespace Vipr.CLI
{
    public interface ITemplateAssemblyReader
    {
        IList<Template> Read(Type targetType, BuilderArguments arguments);
    }
}