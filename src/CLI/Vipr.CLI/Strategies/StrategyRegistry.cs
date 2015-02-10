using System;
using System.Collections.Generic;
using System.Diagnostics;
using TemplateWriter;

namespace Vipr.CLI.Strategies
{
    public class StrategyRegistry : IStrategyResgistry
    {
        public StrategyRegistry()
        {
            Strategies =
                new Dictionary<string, Func<IConfigArguments, IStrategy>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    //{JavaStrategy.JavaStrategyName, x => new JavaStrategy(x)},
                    {ObjectiveCStrategy.ObjectiveCStrategyName, x => new ObjectiveCStrategy(x)},
                    {"Java", x => new SingleFileStrategy(x)}
                };
        }

        public Dictionary<string, Func<IConfigArguments, IStrategy>> Strategies { get; private set; }

        public IStrategy GetStrategy(IConfigArguments arguments)
        {
            Func<IConfigArguments, IStrategy> strategy;
            Strategies.TryGetValue(arguments.BuilderArguments.Language, out strategy);
            Debug.Assert(strategy != null, "strategy != null");
            return strategy(arguments);
        }
    }
}