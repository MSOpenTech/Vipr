using System;
using TemplateWriter;

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