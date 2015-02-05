﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Validation;
using Vipr.Core;
using Vipr.Core.CodeModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace ODataReader.v3
{
    public class Reader : IReader
    {
        public OdcmModel GenerateOdcmModel(IReadOnlyDictionary<string, string> serviceMetadata)
        {
            var daemon = new ReaderDaemon();
            return daemon.GenerateOdcmModel(serviceMetadata);
        }

        private class ReaderDaemon
        {
            private const string MetadataKey = "$metadata";

            private IEdmModel _edmModel = null;
            private OdcmModel _odcmModel;

            public OdcmModel GenerateOdcmModel(IReadOnlyDictionary<string, string> serviceMetadata)
            {
                if (serviceMetadata == null)
                    throw new ArgumentNullException("serviceMetadata");

                if (!serviceMetadata.ContainsKey(MetadataKey))
                    throw new ArgumentException("Argument must contain value for key \"$metadata\"", "serviceMetadata");

                var edmx = XDocument.Parse(serviceMetadata[MetadataKey]);

                IEnumerable<EdmError> errors;
                if (!EdmxReader.TryParse(edmx.CreateReader(ReaderOptions.None), out _edmModel, out errors))
                {
                    Debug.Assert(errors != null, "errors != null");

                    if (errors.FirstOrDefault() == null)
                    {
                        throw new InvalidOperationException();
                    }

                    throw new InvalidOperationException(errors.FirstOrDefault().ErrorMessage);
                }

                _odcmModel = new OdcmModel(serviceMetadata);

                WriteNamespaces();

                return _odcmModel;
            }

            private void WriteNamespaces()
            {
                var declaredNamespaces = (from se in _edmModel.SchemaElements
                    select se.Namespace).Distinct();
                foreach (var declaredNamespace in declaredNamespaces)
                {
                    WriteNamespace(_edmModel, declaredNamespace);
                }
            }

            private void WriteNamespace(IEdmModel edmModel, string @namespace)
            {
                var namespaceElements = from se in edmModel.SchemaElements
                    where string.Equals(se.Namespace, @namespace)
                    select se;

                var types = from se in namespaceElements
                    where se.SchemaElementKind == EdmSchemaElementKind.TypeDefinition
                    select se as IEdmType;

                var complexTypes = from se in types
                    where se.TypeKind == EdmTypeKind.Complex
                    select se as IEdmComplexType;

                var entityTypes = from se in types
                    where se.TypeKind == EdmTypeKind.Entity
                    select se as IEdmEntityType;

                var enumTypes = from se in types
                    where se.TypeKind == EdmTypeKind.Enum
                    select se as IEdmEnumType;

                var entityContainers = from se in namespaceElements
                    where se.SchemaElementKind == EdmSchemaElementKind.EntityContainer
                    select se as IEdmEntityContainer;

                var boundFunctions = from se in
                    (from ec in entityContainers
                        select ec.Elements).SelectMany(v => v)
                    where
                        se.ContainerElementKind == EdmContainerElementKind.FunctionImport &&
                        ((IEdmFunctionImport) se).IsBindable
                    select se as IEdmFunctionImport;

                Console.WriteLine(@namespace);

                foreach (var enumType in enumTypes)
                {
                    OdcmEnum odcmEnum;
                    if (!_odcmModel.TryResolveType(enumType.Name, enumType.Namespace, out odcmEnum))
                    {
                        odcmEnum = new OdcmEnum(enumType.Name, enumType.Namespace);
                        _odcmModel.AddType(odcmEnum);
                    }

                    odcmEnum.UnderlyingType =
                        (OdcmPrimitiveType)
                            ResolveType(enumType.UnderlyingType.Name, enumType.UnderlyingType.Namespace,
                                TypeKind.Primitive);
                    odcmEnum.IsFlags = enumType.IsFlags;

                    foreach (var enumMember in enumType.Members)
                    {
                        odcmEnum.Members.Add(new OdcmEnumMember(enumMember.Name)
                        {
                            Value = ((EdmIntegerConstant) enumMember.Value).Value
                        });
                    }
                }

                foreach (var complexType in complexTypes)
                {
                    OdcmClass odcmClass;
                    if (!_odcmModel.TryResolveType(complexType.Name, complexType.Namespace, out odcmClass))
                    {
                        odcmClass = new OdcmClass(complexType.Name, complexType.Namespace, OdcmClassKind.Complex);
                        _odcmModel.AddType(odcmClass);
                    }

                    odcmClass.IsAbstract = complexType.IsAbstract;
                    odcmClass.IsOpen = complexType.IsOpen;

                    if (complexType.BaseType != null)
                    {
                        OdcmClass baseClass;
                        if (
                            !_odcmModel.TryResolveType(((IEdmSchemaElement) complexType.BaseType).Name,
                                ((IEdmSchemaElement) complexType.BaseType).Namespace, out baseClass))
                        {
                            baseClass = new OdcmClass(((IEdmSchemaElement) complexType.BaseType).Name,
                                ((IEdmSchemaElement) complexType.BaseType).Namespace, OdcmClassKind.Complex);
                            _odcmModel.AddType(baseClass);
                        }
                        odcmClass.Base = baseClass;
                        if (!baseClass.Derived.Contains(odcmClass))
                            baseClass.Derived.Add(odcmClass);
                    }

                    var structuralProperties = from element in complexType.Properties()
                        where element.PropertyKind == EdmPropertyKind.Structural
                        select element as IEdmStructuralProperty;
                    foreach (var structuralProperty in structuralProperties)
                    {
                        OdcmField odcmField = WriteStructuralField(odcmClass, structuralProperty);
                        WriteStructuralProperty(odcmClass, odcmField, structuralProperty);
                    }
                }

                foreach (var entityType in entityTypes)
                {
                    OdcmClass odcmClass;
                    if (!_odcmModel.TryResolveType(entityType.Name, entityType.Namespace, out odcmClass))
                    {
                        odcmClass = new OdcmClass(entityType.Name, entityType.Namespace, OdcmClassKind.Entity);
                        _odcmModel.AddType(odcmClass);
                    }

                    odcmClass.IsAbstract = entityType.IsAbstract;
                    odcmClass.IsOpen = entityType.IsOpen;

                    if (entityType.BaseType != null)
                    {
                        OdcmClass baseClass;
                        if (
                            !_odcmModel.TryResolveType(((IEdmSchemaElement) entityType.BaseType).Name,
                                ((IEdmSchemaElement) entityType.BaseType).Namespace, out baseClass))
                        {
                            baseClass = new OdcmClass(((IEdmSchemaElement) entityType.BaseType).Name,
                                ((IEdmSchemaElement) entityType.BaseType).Namespace, OdcmClassKind.Entity);
                            _odcmModel.AddType(baseClass);
                        }

                        odcmClass.Base = baseClass;

                        if (!baseClass.Derived.Contains(odcmClass))
                            baseClass.Derived.Add(odcmClass);
                    }

                    var structuralProperties = from element in entityType.Properties()
                        where element.PropertyKind == EdmPropertyKind.Structural
                        select element as IEdmStructuralProperty;
                    foreach (var structuralProperty in structuralProperties)
                    {
                        OdcmField odcmField = WriteStructuralField(odcmClass, structuralProperty);
                        WriteStructuralProperty(odcmClass, odcmField, structuralProperty);
                    }

                    var navigationProperties = from element in entityType.Properties()
                        where element.PropertyKind == EdmPropertyKind.Navigation
                        select element as IEdmNavigationProperty;
                    foreach (var navigationProperty in navigationProperties)
                    {
                        OdcmField odcmField = WriteNavigationField(odcmClass, navigationProperty);
                        WriteNavigationProperty(odcmClass, odcmField, navigationProperty);
                    }

                    foreach (IEdmStructuralProperty keyProperty in entityType.Key())
                    {
                        var property = FindProperty(odcmClass, keyProperty);
                        if (property != null)
                        {
                            // The properties that compose the key MUST be non-nullable.
                            property.IsNullable = false;
                        }

                        odcmClass.Key.Add(FindField(odcmClass, keyProperty));
                    }

                    var entityTypeFunctions = from se in boundFunctions
                        where IsFunctionBound(se, entityType)
                        select se;
                    foreach (var function in entityTypeFunctions)
                    {
                        WriteMethod(odcmClass, function);
                    }
                }

                foreach (var entityContainer in entityContainers)
                {
                    OdcmClass odcmClass;
                    if (!_odcmModel.TryResolveType(entityContainer.Name, entityContainer.Namespace, out odcmClass))
                    {
                        odcmClass = new OdcmClass(entityContainer.Name, entityContainer.Namespace, OdcmClassKind.Service);
                        _odcmModel.AddType(odcmClass);
                    }

                    var entitySets = from se in entityContainer.Elements
                        where se.ContainerElementKind == EdmContainerElementKind.EntitySet
                        select se as IEdmEntitySet;
                    foreach (var entitySet in entitySets)
                    {
                        OdcmField odcmField = WriteField(odcmClass, entitySet);
                        WriteProperty(odcmClass, odcmField, entitySet);
                    }

                    var functionImports = from se in entityContainer.Elements
                        where
                            se.ContainerElementKind == EdmContainerElementKind.FunctionImport &&
                            !((IEdmFunctionImport) se).IsBindable
                        select se as IEdmFunctionImport;
                    foreach (var functionImport in functionImports)
                    {
                        WriteMethod(odcmClass, functionImport);
                    }
                }
            }

            private bool IsFunctionBound(IEdmFunctionImport function, IEdmEntityType entityType)
            {
                var bindingParameterType = function.Parameters.First().Type;

                return (bindingParameterType.Definition == entityType) ||
                       (bindingParameterType.IsCollection() &&
                        bindingParameterType.AsCollection().ElementType().Definition == entityType);
            }

            private void WriteProperty(OdcmClass odcmClass, OdcmField odcmField, IEdmEntitySet entitySet)
            {
                OdcmProperty odcmProperty = new OdcmProperty(entitySet.Name);
                odcmProperty.Class = odcmClass;
                odcmClass.Properties.Add(odcmProperty);

                odcmProperty.Field = odcmField;
                odcmProperty.Type = odcmField.Type;
            }

            private OdcmField WriteField(OdcmClass odcmClass, IEdmEntitySet entitySet)
            {
                OdcmField odcmField = new OdcmField("_" + entitySet.Name);
                odcmField.Class = odcmClass;
                odcmClass.Fields.Add(odcmField);

                odcmField.Type = ResolveType(entitySet.ElementType.Name, entitySet.ElementType.Namespace,
                    TypeKind.Entity);

                odcmField.IsCollection = true;
                odcmField.IsLink = true;

                return odcmField;
            }

            private OdcmField FindField(OdcmClass odcmClass, IEdmStructuralProperty keyProperty)
            {
                foreach (OdcmField field in odcmClass.Fields)
                {
                    if (field.Name.Equals(keyProperty.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return field;
                    }
                }

                return null;
            }

            private OdcmProperty FindProperty(OdcmClass odcmClass, IEdmStructuralProperty keyProperty)
            {
                foreach (OdcmProperty property in odcmClass.Properties)
                {
                    if (property.Name.Equals(keyProperty.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return property;
                    }
                }

                return null;
            }

            private void WriteMethod(OdcmClass odcmClass, IEdmFunctionImport operation)
            {
                IEnumerable<IEdmFunctionParameter> parameters = operation.IsBindable
                    ? (from parameter in operation.Parameters
                        where parameter != operation.Parameters.First()
                        select parameter)
                    : (operation.Parameters);

                bool isBoundToCollection = operation.IsBindable && operation.Parameters.First().Type.IsCollection();

                var odcmMethod = new OdcmMethod(operation.Name)
                {
                    Verbs = operation.IsSideEffecting ? OdcmAllowedVerbs.Post : OdcmAllowedVerbs.Any,
                    IsBoundToCollection = isBoundToCollection,
                    IsComposable = operation.IsComposable,
                    Class = odcmClass
                };

                odcmClass.Methods.Add(odcmMethod);

                if (operation.ReturnType != null)
                {
                    odcmMethod.ReturnType = ResolveType(operation.ReturnType);
                    odcmMethod.IsCollection = operation.ReturnType.IsCollection();
                }

                var callingConvention =
                    operation.IsSideEffecting
                        ? OdcmCallingConvention.InHttpMessageBody
                        : OdcmCallingConvention.InHttpRequestUri;

                foreach (var parameter in parameters)
                {
                    odcmMethod.Parameters.Add(new OdcmParameter(parameter.Name)
                    {
                        CallingConvention = callingConvention,
                        Type = ResolveType(parameter.Type),
                        IsCollection = parameter.Type.IsCollection(),
                        IsNullable = parameter.Type.IsNullable
                    });
                }
            }

            private void WriteNavigationProperty(OdcmClass odcmClass, OdcmField odcmField,
                IEdmNavigationProperty navigationProperty)
            {
                WriteProperty(odcmClass, odcmField, navigationProperty);
            }

            private void WriteStructuralProperty(OdcmClass odcmClass, OdcmField odcmField,
                IEdmStructuralProperty structuralProperty)
            {
                WriteProperty(odcmClass, odcmField, structuralProperty);
            }

            private void WriteProperty(OdcmClass odcmClass, OdcmField odcmField, IEdmProperty property)
            {
                OdcmProperty odcmProperty = new OdcmProperty(property.Name);
                odcmProperty.Class = odcmClass;
                odcmProperty.IsNullable = property.Type.IsNullable;
                odcmClass.Properties.Add(odcmProperty);

                odcmProperty.Field = odcmField;
            }

            private OdcmField WriteNavigationField(OdcmClass odcmClass, IEdmNavigationProperty navigationProperty)
            {
                OdcmField odcmField = WriteField(odcmClass, navigationProperty);

                odcmField.ContainsTarget = navigationProperty.ContainsTarget;
                odcmField.IsLink = true;

                return odcmField;
            }

            private OdcmField WriteStructuralField(OdcmClass odcmClass, IEdmStructuralProperty structuralProperty)
            {
                return WriteField(odcmClass, structuralProperty);
            }

            private OdcmField WriteField(OdcmClass odcmClass, IEdmProperty property)
            {
                OdcmField odcmField = new OdcmField(property.Name);
                odcmField.Class = odcmClass;
                odcmClass.Fields.Add(odcmField);

                odcmField.Type = ResolveType(property.Type);
                odcmField.IsCollection = property.Type.IsCollection();

                return odcmField;
            }

            private OdcmType ResolveType(IEdmTypeReference realizedType)
            {
                if (realizedType.IsCollection())
                {
                    realizedType = realizedType.AsCollection().ElementType();
                }

                if (realizedType.IsComplex())
                    return ResolveType(realizedType.Definition as IEdmSchemaElement, TypeKind.Complex);

                if (realizedType.IsEntity())
                    return ResolveType(realizedType.Definition as IEdmSchemaElement, TypeKind.Entity);

                if (realizedType.IsEnum())
                    return ResolveType(realizedType.Definition as IEdmSchemaElement, TypeKind.Enum);

                return ResolveType(realizedType.Definition as IEdmSchemaElement, TypeKind.Primitive);
            }

            private OdcmType ResolveType(IEdmSchemaElement realizedSchemaElement, TypeKind kind)
            {
                return ResolveType(realizedSchemaElement.Name, realizedSchemaElement.Namespace, kind);
            }

            private OdcmType ResolveType(string name, string @namespace, TypeKind kind)
            {
                OdcmType type;

                if (!_odcmModel.TryResolveType(name, @namespace, out type))
                {
                    switch (kind)
                    {
                        case TypeKind.Complex:
                            type = new OdcmClass(name, @namespace, OdcmClassKind.Complex);
                            break;
                        case TypeKind.Entity:
                            type = new OdcmClass(name, @namespace, OdcmClassKind.Entity);
                            break;
                        case TypeKind.Enum:
                            type = new OdcmEnum(name, @namespace);
                            break;
                        case TypeKind.Primitive:
                            type = new OdcmPrimitiveType(name, @namespace);
                            break;
                    }

                    _odcmModel.AddType(type);
                }

                return type;
            }
        }
    }

    public static class Extensions
    {
        public static string FullTypeName(this IEdmTypeReference typeReference, string @namespace = "")
        {
            string result = (typeReference.IsCollection())
                ? "Collection(" + typeReference.AsCollection().ElementType().FullName() + ")"
                : typeReference.FullName();

            if (!string.IsNullOrEmpty(@namespace))
            {
                result = result.Replace(@namespace + ".", "");
            }

            return result;
        }
    }
}
