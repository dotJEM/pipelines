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
        string Key<TContext>(TContext context) where TContext : IPipelineContext;
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

        public string Key<TContext>(TContext context) where TContext : IPipelineContext
            => keyGenerator.Generate(context);
        public JObject Performance(IPipelineContext context) => perfGenerator.Generate(context);

        public IEnumerable<IPipelineMethod<T>> Nodes(IPipelineContext context)
        {
            return nodes.SelectMany(n => n.For(context));
        }

        public class KeyGenerator
        {
            private readonly Encoding encoding = Encoding.UTF8;
            private readonly SHA256CryptoServiceProvider provider = new();

            private readonly string[] keys;

            public KeyGenerator(string[] keys)
            {
                this.keys = keys;
            }

            public string Generate<TContext>(TContext context) where TContext : IPipelineContext
            {
                if (context == null) throw new ArgumentNullException(nameof(context));
                //TODO: This looks expensive. Perhaps we could cut some corners?
                IEnumerable<byte> bytes = keys
                    .SelectMany(key => context.TryGetValue(key, out object value) ? encoding.GetBytes(value.ToString()) : Array.Empty<byte>());
                byte[] typeBytes = typeof(TContext).GUID.ToByteArray();
                byte[] hash = provider.ComputeHash(bytes.Concat(typeBytes).ToArray());
                return string.Join("", hash.Select(b => b.ToString("X2")));
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
        IPipelineGraph<T> GetGraph<T, TContext>();
    }

    public class PipelineGraphFactory : IPipelineGraphFactory
    {
        private readonly IPipelineHandlerCollection handlers;
        private readonly IPipelineExecutorDelegateFactory factory;

        private readonly ConcurrentDictionary<string, IPipelineGraph> graphs = new();

        public PipelineGraphFactory(IPipelineHandlerCollection handlers, IPipelineExecutorDelegateFactory factory)
        {
            this.handlers = handlers;
            this.factory = factory;
        }

        public IPipelineGraph<T> GetGraph<T, TContext>()
        {

            return (IPipelineGraph<T>)graphs.GetOrAdd($"{typeof(T).GUID:N}-{typeof(TContext).GUID:N}", _ => BuildGraph<T, TContext>(this.handlers));
        }

        private IPipelineGraph BuildGraph<T, TContext>(IPipelineHandlerCollection providers)
        {
            List<IClassNode<T>> groups = new();
            foreach (IPipelineHandlerProvider provider in providers)
            {
                Type type = provider.GetType();
                PipelineFilterAttribute[] selectors = type.GetCustomAttributes<PipelineFilterAttribute>().ToArray();

                List<MethodNode<T>> nodes = new();
                // ReSharper disable once LoopCanBeConvertedToQuery -> Linq will not make this more clear!
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.ReturnType != typeof(Task<T>))
                        continue;
                    
                    PipelineFilterAttribute[] methodSelectors = method.GetCustomAttributes<PipelineFilterAttribute>(true).ToArray();
                    if (!methodSelectors.Any()) continue;

                    MethodNode<T> node = factory.CreateNode<T, TContext>(provider, method, selectors.Concat(methodSelectors).ToArray());
                    nodes.Add(node);
                }
                groups.Add(new ClassNode<T>(nodes));
            }
            return new PipelineGraph<T>(groups);
        }
    }
}