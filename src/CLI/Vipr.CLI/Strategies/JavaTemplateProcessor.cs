using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TextTemplating;
using TemplateWriter;
using Vipr.CLI.Output;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Strategies
{
    public class JavaTemplateProcessor
    {
        public const string ComplexType = "ComplexType";
        public const string EntityType = "EntityType";
        public const string EnumType = "EnumType";
        public const string ODataBaseEntity = "ODataBaseEntity";
        public const string EntityCollectionOperation = "EntityCollectionOperation";
        public const string EntityFetcher = "EntityFetcher";
        public const string EntityOperations = "EntityOperations";
        public const string EntryPoint = "EntryPoint";

        public const String JavaStrategyName = "Java";

        private readonly IFileWriter _fileWriter;
        private readonly Engine _engine;
        private readonly OdcmModel _model;

        public Dictionary<string, Action<Template>> Templates { get; set; }

        public JavaTemplateProcessor(IFileWriter fileWriter, OdcmModel model)
        {
            _model = model;
            _fileWriter = fileWriter;
            _engine = new Engine();

            Templates = new Dictionary<string, Action<Template>>(StringComparer.InvariantCultureIgnoreCase)
            {
                //Model
                {EntityType, EntityTypes},
                {ComplexType, ComplexTypes},
                {EnumType, EnumTypes},
                {ODataBaseEntity, BaseEntity},
                //OData
                {EntityCollectionOperation, EntityTypes},
                {EntityFetcher, EntityTypes},
                {EntityOperations, EntityTypes},
                //EntityContainer
                {EntryPoint, CreateEntryPoint},
            };
        }

        private void CreateEntryPoint(Template template)
        {
            var container = _model.EntityContainer;
            ProcessTemplate(template, container);
        }

        private void BaseEntity(Template template)
        {
            var host = new CustomHost(JavaStrategyName, null)
            {
                TemplateFile = template.Path,
                Model = _model
            };

            var templateContent = File.ReadAllText(host.TemplateFile);
            var output = _engine.ProcessTemplate(templateContent, host);

            if (host.Errors != null && host.Errors.HasErrors)
            {
                var errors = LogErrors(host, template);
                throw new InvalidOperationException(errors);
            }

            _fileWriter.WriteText(template, _model.EntityContainer.Name, output);
        }

        private void EnumTypes(Template template)
        {
            var enums = _model.GetEnumTypes();
            ProcessingAction(enums, template);
        }

        public void ComplexTypes(Template template)
        {
            var complexTypes = _model.GetComplexTypes();
            ProcessingAction(complexTypes, template);
        }

        public void EntityTypes(Template template)
        {
            var entityTypes = _model.GetEntityTypes();
            ProcessingAction(entityTypes, template);
        }

        public void ProcessingAction(IEnumerable<OdcmObject> source, Template template)
        {
            foreach (var complexType in source)
            {
                ProcessTemplate(template, complexType);
            }
        }

        private void ProcessTemplate(Template template, OdcmObject odcmObject)
        {
            var host = new CustomHost(JavaStrategyName, odcmObject)
            {
                TemplateFile = template.Path,
                Model = _model
            };

            var templateContent = File.ReadAllText(host.TemplateFile);
            var output = _engine.ProcessTemplate(templateContent, host);

            if (host.Errors != null && host.Errors.HasErrors)
            {
                var errors = LogErrors(host, template);
                throw new InvalidOperationException(errors);
            }
            _fileWriter.WriteText(template, odcmObject.Name, output);
        }

        protected static string LogErrors(CustomHost host, Template template)
        {
            var sb = new StringBuilder();
            if (host.Errors == null || host.Errors.Count <= 0) return sb.ToString();
            foreach (CompilerError error in host.Errors)
            {
                sb.AppendLine("Error template name: " + template.Name);
                sb.AppendLine("Error template" + host.TemplateFile);
                sb.AppendLine(error.ErrorText);
                sb.AppendLine("In line: " + error.Line);
                sb.AppendLine(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
