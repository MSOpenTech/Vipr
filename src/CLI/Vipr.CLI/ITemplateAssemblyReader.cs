using System;
using System.Collections.Generic;

namespace Vipr.CLI
{
    public interface ITemplateAssemblyReader
    {
        IList<Template> Read(Type targetType);
    }
}