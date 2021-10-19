﻿using System;
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
    public static class LoggerExtensions
    {
        //TODO: We need to have null logger as part of dotjem.diagnostics instead so we can do this correctly.
        public static bool IsEnabled(this ILogger self) => !(self.GetType().Name.Equals("NullLogger"));
    }

    public interface IUnboundPipeline<T>
    {
        Task<T> Invoke(IPipelineContext context);
    }
    public class UnboundPipeline<T> : IUnboundPipeline<T>
    {
        private readonly IPrivateNode<T> target;

        public UnboundPipeline(ILogger performance, IPipelineGraph graph, IEnumerable<MethodNode<T>> nodes, Func<IPipelineContext, Task<T>> final)
        {
            if (performance.IsEnabled())
            {
                this.target = nodes.Reverse()
                    .Aggregate(
                        (IPrivateNode<T>)new PerformanceNode<T>(performance, graph.Performance, new TerminationMethod<T>((context, _) => final(context)), null),
                        (node, methodNode) => new PerformanceNode<T>(performance, graph.Performance, methodNode, node));
            }
            else
            {
                this.target = nodes.Reverse()
                    .Aggregate(
                        (IPrivateNode<T>)new Node<T>(new TerminationMethod<T>((context, _) => final(context)), null),
                        (node, methodNode) => new Node<T>(methodNode, node));
            }
        }

        public Task<T> Invoke(IPipelineContext context)
        {
            return target.Invoke(context);
        }

        public override string ToString()
        {
            IPrivateNode<T> node = this.target;
            StringBuilder builder = new ();
            do
            {
                builder.AppendLine($" -> {node}");
            } while ((node = node.Next) != null);
            return builder.ToString();
        }

        private interface IPrivateNode<T> : INode<T>
        {
            IPrivateNode<T> Next { get; }
        }

        private class PerformanceNode<T> : IPrivateNode<T>
        {
            private readonly IPrivateNode<T> next;
            private readonly PipelineExecutorDelegate<T> target;
            private readonly NextFactoryDelegate<T> factory;
            private readonly ILogger performance;
            private readonly Func<IPipelineContext, JObject> perfGenerator;
            private readonly string signature;

            public IPrivateNode<T> Next => next;

            public PerformanceNode(ILogger performance, Func<IPipelineContext, JObject> perfGenerator, IPipelineMethod<T> method, IPrivateNode<T> next)
            {
                this.performance = performance;
                this.perfGenerator = perfGenerator;
                this.next = next;
                this.factory = method.NextFactory;
                this.target = method.Target;
                this.signature = method.Signature;
            }

            public async Task<T> Invoke(IPipelineContext context)
            {
                //TODO: Here we generate the same JObject again and again, however it may be faster than reusing and clearing correctly.
                JObject info = perfGenerator(context);
                info["$$handler"] = signature;
                using (performance.Track("pipeline", info))
                    return await target(context, factory(context, next));
            }

            public override string ToString()
            {
                return $"{signature} (Performance)";
            }
        }

        private class Node<T> : IPrivateNode<T>
        {
            private readonly IPrivateNode<T> next;
            private readonly PipelineExecutorDelegate<T> target;
            private readonly NextFactoryDelegate<T> factory;
            private readonly string signature;

            public IPrivateNode<T> Next => next;

            public Node(IPipelineMethod<T> method, IPrivateNode<T> next)
            {
                this.next = next;
                this.factory = method.NextFactory;
                this.target = method.Target;
                this.signature = method.Signature;
            }

            public Task<T> Invoke(IPipelineContext context)
            {
                return target(context, factory(context, next));
            }

            public override string ToString()
            {
                return $"{signature} (Normal)";
            }

        }
    }
}