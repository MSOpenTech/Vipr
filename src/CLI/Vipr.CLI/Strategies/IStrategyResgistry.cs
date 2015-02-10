using System;
using System.Collections.Generic;
using TemplateWriter;

namespace Vipr.CLI.Strategies
{
    public interface IStrategyResgistry
    {
        Dictionary<string, Func<IConfigArguments, IStrategy>> Strategies { get; }
        IStrategy GetStrategy(IConfigArguments arguments);
    }
}