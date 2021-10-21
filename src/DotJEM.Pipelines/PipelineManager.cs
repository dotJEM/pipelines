using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotJEM.Diagnostic;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.Nodes;

namespace DotJEM.Pipelines
{
    public interface IPipelines
    {
        ICompiledPipeline<T> For<TContext, T>(TContext context, Func<TContext, Task<T>> final) where TContext : class, IPipelineContext;
    }

    public class PipelineManager : IPipelines
    {
        private readonly ILogger performance;
        private readonly IPipelineGraphFactory factory;
        private readonly ConcurrentDictionary<string, object> cache = new();

        public PipelineManager(ILogger performance, IPipelineGraphFactory factory)
        {
            this.performance = performance;
            this.factory = factory;
        }

        public ICompiledPipeline<T> For<TContext, T>(TContext context, Func<TContext, Task<T>> completion) where TContext : class, IPipelineContext
        {
            IPipelineContextCarrier<T> carrier = new PipelineContextCarrier<TContext, T>(context, completion);
            IUnboundPipeline<T> unbound = LookupPipeline<TContext, T>(context);
            return new CompiledPipeline<T>(unbound, carrier);
        }

        public IUnboundPipeline<T> LookupPipeline<TContext, T>(TContext context) where TContext : class, IPipelineContext
        {
            IPipelineGraph<T> graph = factory.GetGraph<T>();
            return (IUnboundPipeline<T>)cache.GetOrAdd(graph.Key(context), key =>
            {
                IEnumerable<IPipelineMethod<T>> matchingNodes = graph.Nodes(context);
                return new UnboundPipeline<T>(performance, graph, matchingNodes);
            });
        }
    }

}