using System;
using TemplateWriter;

namespace Vipr.CLI.Strategies
{
    public class ObjectiveCStrategy : BaseStrategy, IStrategy
    {
        private readonly IConfigArguments _configArguments;

        public ObjectiveCStrategy(IConfigArguments configArguments):base(configArguments)
        {
            _configArguments = configArguments;
        }

        public override void ProcessTemplates()
        {
            throw new NotImplementedException();
        }

        public override string Name
        {
            get { return ObjectiveCStrategyName; }
        }

        public const String ObjectiveCStrategyName = "ObjectiveC";
    }
}