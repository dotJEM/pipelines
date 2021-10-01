using DotJEM.Pipelines.NextHandlers;
using DotJEM.Pipelines.Nodes;

namespace DotJEM.Pipelines.Factories
{
    public delegate INext<T> NextFactoryDelegate<T>(IPipelineContext context, INode<T> node);
}