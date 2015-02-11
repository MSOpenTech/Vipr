using System;
using System.Collections.Generic;
using System.Linq;
using TemplateWriter;
using Vipr.CLI.Extensions;

namespace Vipr.CLI
{
    public class TemplateSourceReader : ITemplateSourceReader
    {
        public IList<Template> Read(Type targetType, BuilderArguments arguments)
        {
            var resourceNames = targetType.Assembly.GetManifestResourceNames();
            var baseString = string.Format("{0}.Base", arguments.Language);
            return resourceNames.Select(x =>
            {
                var splits = x.Split('.');
                var name = splits.ElementAt(splits.Count() - 2);
                return new Template(name, x)
                {
                    Name = name,
                    ResourceName = x,
                    IsBase = x.Contains(baseString, StringComparison.InvariantCultureIgnoreCase)
                };
            }).ToList();
        }

    }
}