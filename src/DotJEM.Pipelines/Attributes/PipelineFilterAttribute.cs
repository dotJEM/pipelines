using System;

namespace DotJEM.Pipelines.Attributes
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public abstract class PipelineFilterAttribute : Attribute
    {
        public abstract bool Accepts(IPipelineContext context);
    }
}