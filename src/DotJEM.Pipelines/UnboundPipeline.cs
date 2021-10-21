using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotJEM.Diagnostic;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.Nodes;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines
{

    public interface IPipelineContextCarrier<T>
    {
        IPipelineContext Context { get; }
        Task<T> Complete(IPipelineContext context);
        IPipelineContextCarrier<T> Modify(Func<IPipelineContext, IPipelineContext> func);
    }

    public class PipelineContextCarrier<TContext, T> : IPipelineContextCarrier<T> where TContext : class, IPipelineContext
    {
        private TContext context;
        private readonly Func<TContext, Task<T>> completion;

        public IPipelineContext Context => context;

        public PipelineContextCarrier(TContext context, Func<TContext, Task<T>> completion)
        {
            this.context = context;
            this.completion = completion;
        }

        public Task<T> Complete(IPipelineContext context)
        {
            //TODO: Ideally contexts should have an option to be immuteable, this uses the initial context .
            return completion(context as TContext);
        }

        public IPipelineContextCarrier<T> Modify(Func<IPipelineContext, IPipelineContext> func)
        {
            //TODO: Ideally contexts should have an option to be immuteable.
            this.context = (TContext)func(context);
            return this;
        }
    }

    public interface IUnboundPipeline<T>
    {
        Task<T> Invoke(IPipelineContextCarrier<T> context);
    }

    public class UnboundPipeline<T> : IUnboundPipeline<T>
    {
        private readonly IPrivateNode target;

        public UnboundPipeline(ILogger logger, IPipelineGraph graph, IEnumerable<IPipelineMethod<T>> nodes)
        {
            if (logger is not NullLogger)
            {
                this.target = nodes.Reverse()
                    .Aggregate(
                        (IPrivateNode)new LoggingFinalNode(logger, graph.Performance),
                        (node, methodNode) => new LoggingNode(logger, graph.Performance, methodNode, node));
            }
            else
            {
                this.target = nodes.Reverse()
                    .Aggregate(
                        (IPrivateNode)new FinalNode(),
                        (node, methodNode) => new Node(methodNode, node));
            }
        }

        public Task<T> Invoke(IPipelineContextCarrier<T> context)
        {
            return target.Invoke(context);
        }

        public override string ToString()
        {
            IPrivateNode node = this.target;
            StringBuilder builder = new ();
            do
            {
                builder.AppendLine($" -> {node}");
            } while ((node = node.Next) != null);
            return builder.ToString();
        }

        private interface IPrivateNode : INode<T>
        {
            IPrivateNode Next { get; }
        }

        private class LoggingNode : IPrivateNode
        {
            private readonly IPrivateNode next;
            private readonly PipelineExecutorDelegate<T> target;
            private readonly NextFactoryDelegate<T> factory;
            private readonly ILogger logger;
            private readonly Func<IPipelineContext, JObject> perfGenerator;
            private readonly string signature;

            public IPrivateNode Next => next;

            public LoggingNode(ILogger logger, Func<IPipelineContext, JObject> perfGenerator, IPipelineMethod<T> method, IPrivateNode next)
            {
                this.logger = logger;
                this.perfGenerator = perfGenerator;
                this.next = next;
                this.factory = method.NextFactory;
                this.target = method.Target;
                this.signature = method.Signature;
            }

            public async Task<T> Invoke(IPipelineContextCarrier<T> carrier)
            {
                //TODO: Here we generate the same JObject again and again, however it may be faster than reusing and clearing correctly.
                JObject info = perfGenerator(carrier.Context);
                info["$$handler"] = signature;
                using (logger.Track("pipeline", info))
                    return await target(carrier.Context, factory(carrier, next));
            }

            public override string ToString()
            {
                return $"{signature} (Performance)";
            }
        }

        private class LoggingFinalNode : IPrivateNode
        {
            private readonly ILogger logger;
            private readonly Func<IPipelineContext, JObject> perfGenerator;

            public IPrivateNode Next => null;
         
            public LoggingFinalNode(ILogger logger, Func<IPipelineContext, JObject> perfGenerator)
            {
                this.logger = logger;
                this.perfGenerator = perfGenerator;
            }

            public async Task<T> Invoke(IPipelineContextCarrier<T> carrier)
            {
                //TODO: Here we generate the same JObject again and again, however it may be faster than reusing and clearing correctly.
                JObject info = perfGenerator(carrier.Context);
                info["$$handler"] = $"(LoggingFinalNode)";
                using (logger.Track("pipeline", info))
                    return await carrier.Complete(carrier.Context);
            }

            public override string ToString() => "(LoggingFinalNode)";
        }

        private class Node : IPrivateNode
        {
            private readonly IPrivateNode next;
            private readonly PipelineExecutorDelegate<T> target;
            private readonly NextFactoryDelegate<T> factory;
            private readonly string signature;

            public IPrivateNode Next => next;

            public Node(IPipelineMethod<T> method, IPrivateNode next)
            {
                this.next = next;
                this.factory = method.NextFactory;
                this.target = method.Target;
                this.signature = method.Signature;
            }

            public Task<T> Invoke(IPipelineContextCarrier<T> carrier)
            {
                return target(carrier.Context, factory(carrier, next));
            }

            public override string ToString()
            {
                return $"{signature} (Normal)";
            }
        }

        private class FinalNode : IPrivateNode
        {
            public IPrivateNode Next => null;
         
            public Task<T> Invoke(IPipelineContextCarrier<T> carrier) => carrier.Complete(carrier.Context);

            public override string ToString() => "(FinalNode)";
        }
    }
}