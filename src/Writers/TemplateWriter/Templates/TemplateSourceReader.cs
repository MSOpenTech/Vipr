﻿using System;
using System.Collections.Generic;
using System.Linq;
using TemplateWriter.Extensions;

namespace TemplateWriter
{
    public class TemplateSourceReader : ITemplateSourceReader
    {
        public IList<Template> Read(Type targetType, TemplateWriterConfiguration config)
        {
            var resourceNames = targetType.Assembly.GetManifestResourceNames();
            var baseString = string.Format("{0}.Base", config.TargetLanguage);

            return resourceNames.Select(resource =>
            {
                var splits = resource.Split('.');
                var name = splits.ElementAt(splits.Count() - 2);
                var folderName = FolderName(resource, config);

                return new Template(name, resource)
                {
                    FolderName = folderName,
                    Name = name,
                    ResourceName = resource,
                    IsBase = resource.Contains(baseString, StringComparison.InvariantCultureIgnoreCase),
                    TemplateType = ResolveTemplateType(folderName)
                };
            }).ToList();
        }

        public TemplateType ResolveTemplateType(string name)
        {
            if (name.Equals("model", StringComparison.InvariantCultureIgnoreCase))
            {
                return TemplateType.Model;
            }

            if (name.Equals("odata", StringComparison.InvariantCultureIgnoreCase))
            {
                return TemplateType.OData;
            }

            return TemplateType.Other;
        }

        private string FolderName(string resourceName, TemplateWriterConfiguration config)
        {
            var modelLocation = string.Format("{0}.Models", config.TargetLanguage);
            var odataLocation = string.Format("{0}.OData", config.TargetLanguage);

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