using System;

namespace Vipr.CLI
{
    public interface IStrategy
    {
        void ProcessTemplates();

        String Name { get; }
    }
}