using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using DotJEM.Diagnostic;
using DotJEM.Pipelines;
using DotJEM.Pipelines.Benchmarks;
using DotJEM.Pipelines.Factories;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Benchmarks
{

    public class JsonPipelineContext : PipelineContext
    {
        public JObject ToJson()
        {
            return JObject.FromObject(base.parameters);
        }
    }

    public class FixedJsonPipelineContext : PipelineContext
    {
        private string contentType;
        public string ContentType
        {
            get => contentType;
            set
            {
                contentType = value;
                Set("contentType", value);
            }
        }

        private Guid id;
        public Guid Id
        {
            get => id;
            set
            {
                id = value;
                Set("id", value);
            }
        }

        private string method;
        public string Method
        {
            get => method;
            set
            {
                method = value;
                Set("method", value);
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                Set("type", value);
            }
        }


        public FixedJsonPipelineContext(string contentType, Guid id, string method, string type)
        {
            this.ContentType = contentType;
            this.Id = id;
            this.Method = method;
            this.Type = type;

            //Set("contentType", contentType);
            //Set("id", id);
            //Set("method", method);
            //Set("type", type);

            //Bind("contentType", contentType, x => this.contentType = c);
        }
        public JObject ToJson()
        {
            return JObject.FromObject(base.parameters);
        }
    }


    [SimpleJob(RuntimeMoniker.Net60), SimpleJob(RuntimeMoniker.Net50), SimpleJob(RuntimeMoniker.Net48)]
    public class PipelineExecutionWithLoggerBenchmarks
    {
        private readonly IPipelines pipelines;

        public PipelineExecutionWithLoggerBenchmarks()
        {
            IPipelineHandlerProvider[] providersArr =
            {
                new LegacyAdapter(),
                new PureHandler(),
                new Pure1Handler(),
                new Pure2Handler(),
                new Pure3Handler()
            };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            pipelines = new PipelineManager(new EmptyLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));
        }


        private ICompiledPipeline<JObject> Build(string type) => Build(type, x => x);

        private ICompiledPipeline<JObject> Build(string type, Func<IPipelineContext, IPipelineContext> ctx)
        {
            IPipelineContext context = ctx(new JsonPipelineContext()
                .Set("contentType", "none")
                .Set("id", Guid.Empty)
                .Set("method", "GET")
                .Set("type", type));

            ICompiledPipeline<JObject> pipeline = pipelines
                .For((JsonPipelineContext)context, ctx => Task.FromResult(ctx.ToJson()));

#if DEBUG
            Console.WriteLine(pipeline);
#endif
            return pipeline;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            Console.WriteLine("Pipeline:");
            Console.WriteLine(pipeline);
            Console.WriteLine("");
            Console.WriteLine("Result:");
            Console.WriteLine(result);
        }

        private JObject result;
        private ICompiledPipeline<JObject> pipeline;

        [Benchmark]
        public void LegacyPipelineAdapter()
        {
            pipeline = Build("LEGACY");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void PurePipelineAdapter()
        {
            pipeline = Build("PURE");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure1PipelineAdapter()
        {
            pipeline = Build("PURE1");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure2PipelineAdapter()
        {
            pipeline = Build("PURE2");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure3PipelineAdapter()
        {
            pipeline = Build("PURE3");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void EmptyPipeline()
        {
            pipeline = pipelines.For<IPipelineContext, JObject>(new PipelineContext(), ctx => Task.FromResult(new JObject()));
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }


    [SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net50, targetCount: 10, warmupCount:5)]
    [SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net60, targetCount: 10, warmupCount:5)]
    [SimpleJob(RunStrategy.ColdStart, RuntimeMoniker.Net48, targetCount: 10, warmupCount:5)]
    public class PipelineExecutionWithoutLoggerBenchmarks
    {
        private readonly IPipelines pipelines;

        public PipelineExecutionWithoutLoggerBenchmarks()
        {
            IPipelineHandlerProvider[] providersArr =
            {
                new LegacyAdapter(),
                new PureHandler(),
                new Pure1Handler(),
                new Pure2Handler(),
                new Pure3Handler()
            };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            pipelines = new PipelineManager(new NullLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));
        }

        private ICompiledPipeline<JObject> Build(string type) => Build(type, x => x);

        private ICompiledPipeline<JObject> Build(string type, Func<IPipelineContext, IPipelineContext> ctx)
        {
            IPipelineContext context = ctx(new JsonPipelineContext()
                .Set("contentType", "none")
                .Set("id", Guid.Empty)
                .Set("method", "GET")
                .Set("type", type));

            ICompiledPipeline<JObject> pipeline = pipelines
                .For((JsonPipelineContext)context, ctx => Task.FromResult(ctx.ToJson()));

#if DEBUG
            Console.WriteLine(pipeline);
#endif

            return pipeline;
        }
        private ICompiledPipeline<JObject> BuildFixed(string type) => BuildFixed(type, x => x);
        private ICompiledPipeline<JObject> BuildFixed(string type, Func<IPipelineContext, IPipelineContext> ctx)
        {
            IPipelineContext context = ctx(new FixedJsonPipelineContext( "none", Guid.Empty, "GET", type));
            ICompiledPipeline<JObject> pipeline = pipelines
                .For<FixedJsonPipelineContext,JObject>((FixedJsonPipelineContext)context, ctx => Task.FromResult(ctx.ToJson()));

#if DEBUG
            Console.WriteLine(pipeline);
#endif

            return pipeline;
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            Console.WriteLine("Pipeline:");
            Console.WriteLine(pipeline);
            Console.WriteLine("");
            Console.WriteLine("Result:");
            Console.WriteLine(result);
        }

        private JObject result;
        private ICompiledPipeline<JObject> pipeline;

        [Benchmark]
        public void LegacyPipelineAdapter()
        {
            pipeline = Build("LEGACY");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void PurePipelineAdapter()
        {
            pipeline = Build("PURE");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure1PipelineAdapter()
        {
            pipeline = Build("PURE1");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure2PipelineAdapter()
        {
            pipeline = Build("PURE2");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure3PipelineAdapter()
        {
            pipeline = Build("PURE3");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void EmptyPipeline()
        {
            pipeline = pipelines.For<IPipelineContext, JObject>(new PipelineContext(), ctx => Task.FromResult(new JObject()));
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        
        [Benchmark]
        public void LegacyPipelineAdapterFixed()
        {
            pipeline = BuildFixed("LEGACY");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void PurePipelineAdapterFixed()
        {
            pipeline = BuildFixed("PURE");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure1PipelineAdapterFixed()
        {
            pipeline = BuildFixed("PURE1");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure2PipelineAdapterFixed()
        {
            pipeline = BuildFixed("PURE2");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void Pure3PipelineAdapterFixed()
        {
            pipeline = BuildFixed("PURE3");
            result = pipeline.Invoke().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}