using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.NextHandlers;

namespace DotJEM.Pipelines.Nodes
{

    public interface INode { }

    public interface INode<T> : INode
    {
        Task<T> Invoke(IPipelineContextCarrier<T> carrier);
    }

    public interface IPipelineMethod<T>
    {
        PipelineExecutorDelegate<T> Target { get; }
        NextFactoryDelegate<T> NextFactory { get; }
        string Signature { get; }
    } 

    public interface IClassNode<T>
    {
        void Visit(IPipelineContext context);
        IEnumerable<IPipelineMethod<T>> For(IPipelineContext context);
    }

    public class ClassNode<T> : IClassNode<T>
    {
        private List<MethodNode<T>> nodes { get; }

        public ClassNode(List<MethodNode<T>> nodes)
        {
            this.nodes = nodes;
        }

        public IEnumerable<IPipelineMethod<T>> For(IPipelineContext context)
        {
            return nodes.Where(n => n.Accepts(context));
        }

        public void Visit(IPipelineContext context)
        {
            foreach (MethodNode<T> node in nodes)
                node.Visit(context);
        }
    }


    public class MethodNode<T> : IPipelineMethod<T>
    {
        private readonly FilterGroup[] filters;
        public string Signature { get; }

        public PipelineExecutorDelegate<T> Target { get; }
        public NextFactoryDelegate<T> NextFactory { get; }

        public MethodNode(PipelineFilterAttribute[] filters, PipelineExecutorDelegate<T> target, NextFactoryDelegate<T> nextFactory, string signature)
        {
            this.filters = filters
                .GroupBy(f => f.Group)
                .Select(g => new FilterGroup(g.Key, g.ToArray()))
                .ToArray();

            this.Signature = signature;
            this.Target = target;
            NextFactory = nextFactory;
        }

        public void Visit(IPipelineContext context)
        {
            foreach (FilterGroup filterGroup in filters)
                filterGroup.Visit(context);
        }

        public bool Accepts(IPipelineContext context)
        {
            return filters.All(selector => selector.Accepts(context));
        }

        private class FilterGroup
        {
            private readonly string key;
            private readonly PipelineFilterAttribute[] filters;

            public FilterGroup(string key, PipelineFilterAttribute[] filters)
            {
                this.key = key;
                this.filters = filters;
            }
            
            public void Visit(IPipelineContext context)
            {
                foreach (PipelineFilterAttribute filter in filters)
                    filter.Accepts(context);
            }

            public bool Accepts(IPipelineContext context)
            {
                return filters.Any(selector => selector.Accepts(context));
            }
        }
    }

}