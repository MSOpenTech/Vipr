using System;

namespace Vipr.CLI.Strategies
{
    public interface IStrategy
    {
        void ProcessTemplates();

        String Name { get; }
    }
}