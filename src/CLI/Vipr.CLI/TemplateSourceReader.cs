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

            return resourceNames.Select(resource =>
            {
                var splits = resource.Split('.');
                var name = splits.ElementAt(splits.Count() - 2);

                return new Template(name, resource)
                {
                    FolderName = FolderName(resource, arguments),
                    Name = name,
                    ResourceName = resource,
                    IsBase = resource.Contains(baseString, StringComparison.InvariantCultureIgnoreCase)
                };
            }).ToList();
        }

        private string FolderName(string resourceName, BuilderArguments arguments)
        {
            var modelLocation = string.Format("{0}.Models", arguments.Language);
            var odataLocation = string.Format("{0}.OData", arguments.Language);

            if (resourceName.Contains(modelLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                return "model";
            }
            if (resourceName.Contains(odataLocation, StringComparison.InvariantCultureIgnoreCase))
            {
                return "odata";
            }
            return string.Empty;
        }

    }
}