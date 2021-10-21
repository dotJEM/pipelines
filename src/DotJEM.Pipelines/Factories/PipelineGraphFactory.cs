using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.Nodes;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Factories
{
    public interface IPipelineGraph
    {
        string Key(IPipelineContext context);
        JObject Performance(IPipelineContext context);
    }

    public interface IPipelineGraph<T> : IPipelineGraph
    {
        IEnumerable<IPipelineMethod<T>> Nodes(IPipelineContext context);
    }

    public class PipelineGraph<T> : IPipelineGraph<T>
    {
        private readonly List<IClassNode<T>> nodes;
        private readonly KeyGenerator keyGenerator;
        private readonly PerfGenerator perfGenerator;

        public PipelineGraph(List<IClassNode<T>> nodes)
        {
            this.nodes = nodes;

            SpyingContext spy = new();
            foreach (IClassNode<T> node in nodes)
                node.Visit(spy);

            keyGenerator = spy.CreateKeyGenerator();
            perfGenerator = spy.CreatePerfGenerator();
        }

        public string Key(IPipelineContext context) => keyGenerator.Generate(context);
        public JObject Performance(IPipelineContext context) => perfGenerator.Generate(context);

        public IEnumerable<IPipelineMethod<T>> Nodes(IPipelineContext context)
        {
            return nodes.SelectMany(n => n.For(context));
        }

        public class KeyGenerator
        {
            private readonly SHA256CryptoServiceProvider provider = new();
            private readonly Encoding encoding = Encoding.UTF8;

            private readonly string[] keys;

            public KeyGenerator(string[] keys)
            {
                this.keys = keys;
            }

            public string Generate(IPipelineContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                //TODO: This looks expensive. Perhaps we could cut some corners?
                IEnumerable<byte> bytes = keys
                    .SelectMany(key => context.TryGetValue(key, out object value) ? encoding.GetBytes(value.ToString()) : Array.Empty<byte>());
                //TODO: The pipelines are dependant on the actual context type at this time, inside the pipeline we should really not care, as such the majority of a pipeline
                //      should be reuseable in case that the return type is the same, and the context just as to inherit from IPipelineContext, but for now that is not the case.
                //byte[] hash = provider.ComputeHash(bytes.ToArray());
                byte[] hash = provider.ComputeHash(bytes.Concat(encoding.GetBytes(context.GetType().FullName)).ToArray());
                string hashkey = string.Join("", hash.Select(b => b.ToString("X2")));
                return hashkey;
            }
        }

        public class PerfGenerator
        {
            
            private readonly string[] keys;

            public PerfGenerator(string[] keys)
            {
                this.keys = keys;
            }

            public JObject Generate(IPipelineContext context)
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                return keys.Aggregate(new JObject() { ["$$context"] = context.GetType().FullName }, (obj, key) =>
                {
                    if (context.TryGetValue(key, out object value))
                        obj[key] = value.ToString();
                    return obj;
                });
            }
        }

        private class SpyingContext : PipelineContext
        {
            private readonly HashSet<string> keys = new();

            public override bool TryGetValue(string key, out object value)
            {
                keys.Add(key);
                value = "";
                return true;
            }

            public KeyGenerator CreateKeyGenerator() => new KeyGenerator(keys.ToArray());

            public PerfGenerator CreatePerfGenerator() => new PerfGenerator(keys.ToArray());
        }
    }

    public interface IPipelineGraphFactory
    {
        IPipelineGraph<T> GetGraph<T>();
    }

    public class PipelineGraphFactory : IPipelineGraphFactory
    {
        private readonly IPipelineHandlerCollection handlers;
        private readonly IPipelineExecutorDelegateFactory factory;

        private readonly ConcurrentDictionary<Type, IPipelineGraph> graphs = new();

        public PipelineGraphFactory(IPipelineHandlerCollection handlers, IPipelineExecutorDelegateFactory factory)
        {
            this.handlers = handlers;
            this.factory = factory;
        }

        public IPipelineGraph<T> GetGraph<T>()
        {
            return (IPipelineGraph<T>)graphs.GetOrAdd(typeof(T), _ => BuildGraph<T>(this.handlers));
        }

        private IPipelineGraph BuildGraph<T>(IPipelineHandlerCollection providers)
        {
            List<IClassNode<T>> groups = new();
            foreach (IPipelineHandlerProvider provider in providers)
            {
                Type type = provider.GetType();
                PipelineFilterAttribute[] selectors = type.GetCustomAttributes(true).OfType<PipelineFilterAttribute>().ToArray();

                List<MethodNode<T>> nodes = new();
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.ReturnType != typeof(Task<T>))
                        continue;

                    PipelineFilterAttribute[] methodSelectors = method.GetCustomAttributes(true).OfType<PipelineFilterAttribute>().ToArray();
                    if (methodSelectors.Any())
                    {
                        MethodNode<T> node = factory.CreateNode<T>(provider, method, selectors.Concat(methodSelectors).ToArray());
                        nodes.Add(node);
                    }
                }
                groups.Add(new ClassNode<T>(nodes));
            }
            return new PipelineGraph<T>(groups);
        }
    }
}