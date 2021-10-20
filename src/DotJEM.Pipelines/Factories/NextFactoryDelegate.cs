using DotJEM.Pipelines.NextHandlers;
using DotJEM.Pipelines.Nodes;

namespace DotJEM.Pipelines.Factories
{
    public delegate INext<T> NextFactoryDelegate<T>(IPipelineContextCarrier<T> context, INode<T> node);
}