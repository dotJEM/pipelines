using System;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
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

    [SimpleJob(RuntimeMoniker.Net50), SimpleJob(RuntimeMoniker.Net48)]
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


    [SimpleJob(RuntimeMoniker.Net50), SimpleJob(RuntimeMoniker.Net48)]
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
}