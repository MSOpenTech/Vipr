﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vipr.Core.CodeModel;

namespace TemplateWriter.Extensions
{
    public static class OdcmModelExtensions
    {
        public static bool IsCollection(this OdcmProperty odcmProperty)
        {
            return odcmProperty.IsCollection;
        }


        private static OdcmNamespace GetOdcmNamespace(OdcmModel model)
        {
            OdcmNamespace namespaceFound;
            var filtered = model.Namespaces.Where(x => !x.Name.Equals("Edm", StringComparison.InvariantCultureIgnoreCase))
                                           .ToList();
            if (filtered.Count() == 1)
            {
                namespaceFound = filtered.Single();
            }
            else
            {
                namespaceFound =
                    model.Namespaces.Find(x => String.Equals(x.Name, ConfigurationService.PrimaryNamespaceName,
                        StringComparison.InvariantCultureIgnoreCase));
            }

            if (namespaceFound == null)
            {
                throw new InvalidOperationException("Multiple namespaces defined in metadata and no matches." +
                                                    "\nPlease check 'PrimaryNamespace' Setting in 'config.json'");
            }
            return namespaceFound;
        }

        public static IEnumerable<OdcmClass> GetComplexTypes(this OdcmModel model)
        {
            var @namespace = GetOdcmNamespace(model);
            return @namespace.Classes.Where(x => x.Kind == OdcmClassKind.Complex);
        }

        public static IEnumerable<OdcmClass> GetEntityTypes(this OdcmModel model)
        {
            var @namespace = GetOdcmNamespace(model);
            return @namespace.Classes.Where(x => x.Kind == OdcmClassKind.Entity || x.Kind == OdcmClassKind.MediaEntity);
        }

        public static IEnumerable<OdcmEnum> GetEnumTypes(this OdcmModel model)
        {
            var @namespace = GetOdcmNamespace(model);
            return @namespace.Types.OfType<OdcmEnum>();
        }

        public static IEnumerable<OdcmProperty> NavigationProperties(this OdcmClass odcmClass)
        {
            return odcmClass.Properties.WhereIsNavigation();
        }

        public static IEnumerable<OdcmProperty> WhereIsNavigation(this IEnumerable<OdcmProperty> odcmProperties,
            bool isNavigation = true)
        {
            return odcmProperties.Where(p => isNavigation == (p.Type is OdcmClass
                                                              && ((OdcmClass)p.Type).Kind == OdcmClassKind.Entity));
        }

        public static bool HasActions(this OdcmClass odcmClass)
        {
            return odcmClass.Methods.Any();
        }

        public static IEnumerable<OdcmMethod> Actions(this OdcmClass odcmClass)
        {
            return odcmClass.Methods;
        }

        public static bool IsFunction(this OdcmMethod method)
        {
            return method.IsComposable; //TODO:REVIEW
        }

        public static string GetNamespace(this OdcmModel model)
        {
            var @namespace = GetOdcmNamespace(model);
            return @namespace.Name;
        }

        public static OdcmClass AsOdcmClass(this OdcmObject odcmObject)
        {
            return odcmObject as OdcmClass;
        }

        public static OdcmEnum AsOdcmEnum(this OdcmObject odcmObject)
        {
            return odcmObject as OdcmEnum;
        }

        public static string NamespaceName(this OdcmModel model)
        {
            var @namespace = GetOdcmNamespace(model).Name;
            var name = string.Format("{0}.{1}", ConfigurationService.Configuration.NamespacePrefix, @namespace);
            return name.ToLower();
        }

        public static string ODataPackageNamespace(this OdcmModel model)
        {
            var @namespace = NamespaceName(model);
            var package = string.Format("{0}.{1}", @namespace, "odata");
            return package.ToLower();
        }

        public static string GetEntityContainer(this OdcmModel model)
        {
            return model.EntityContainer.Name;
        }
    }
}
