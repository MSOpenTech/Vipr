using System;
using TemplateWriter;
using Vipr.CLI.Strategies;

namespace Vipr.CLI
{
    public class TemplateProcessor : ITemplateProcessor
    {
        private readonly IStrategyResgistry _registry;

        public TemplateProcessor(IStrategyResgistry registry)
        {
            _registry = registry;
        }

        public TemplateProcessor()
            : this(new StrategyRegistry())
        {
        }

        public void Process(IConfigArguments configArguments)
        {
            var strategy = _registry.GetStrategy(configArguments);
            if (strategy == null) throw new InvalidOperationException("strategy");

            strategy.ProcessTemplates();
        }
    }
}