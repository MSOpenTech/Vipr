using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TextTemplating;
using TemplateWriter;
using Vipr.CLI.Output;
using Vipr.Core.CodeModel;

namespace Vipr.CLI.Strategies
{
    public class BaseTemplateProcessor : ITemplateProcessor
    {
        public const string ComplexType = "ComplexType";
        public const string EntityType = "EntityType";
        public const string EnumType = "EnumType";
        public const string ODataBaseEntity = "ODataBaseEntity";
        public const string EntityCollectionOperation = "EntityCollectionOperation";
        public const string EntityFetcher = "EntityFetcher";
        public const string EntityOperations = "EntityOperations";
        public const string EntryPoint = "EntryPoint";
        public string _baseFilePath;

        public String StrategyName = "default";

        protected readonly IFileWriter _fileWriter;
        protected readonly Engine _engine;
        protected readonly OdcmModel _model;

        public Dictionary<string, Action<Template>> Templates { get; set; }

        public BaseTemplateProcessor(IFileWriter fileWriter, OdcmModel model, string baseFilePath)
        {
            _model = model;
            _fileWriter = fileWriter;
            _engine = new Engine();
            _baseFilePath = baseFilePath;

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
            var host = new CustomHost(StrategyName, null)
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

        private static CustomHost HostIntance;

        private CustomHost GetCustomHost(Template template, OdcmObject odcmObject)
        {
            if(HostIntance == null)
                HostIntance = new CustomHost(StrategyName, odcmObject);

            HostIntance.BaseTemplatePath = _baseFilePath;
            HostIntance.TemplateFile = template.Path;
            HostIntance.Model = _model;
            HostIntance.OdcmType = odcmObject;

            return HostIntance;
        }

        private void ProcessTemplate(Template template, OdcmObject odcmObject)
        {
            var host = GetCustomHost(template, odcmObject);

            var templateContent = File.ReadAllText(host.TemplateFile);
            var output = _engine.ProcessTemplate(templateContent, host);

            if (host.Errors != null && host.Errors.HasErrors)
            {
                var errors = LogErrors(host, template);
                throw new InvalidOperationException(errors);
            }
            _fileWriter.WriteText(template, odcmObject.Name, output);
        }

        public string LogErrors(CustomHost host, Template template)
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