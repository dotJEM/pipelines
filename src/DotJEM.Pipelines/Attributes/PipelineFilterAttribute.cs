using System;

namespace DotJEM.Pipelines.Attributes
{
    public abstract class PipelineFilterAttribute : Attribute
    {
        public abstract string Group { get; }

        public abstract bool Accepts(IPipelineContext context);
    }
}