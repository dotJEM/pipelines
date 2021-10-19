using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using DotJEM.Diagnostic;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.NextHandlers;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Benchmarks
{
    public class Configuration : ManualConfig
    {
    }

    [SimpleJob(RuntimeMoniker.CoreRt50),SimpleJob(RuntimeMoniker.Net48)]
    public class PipelineExecutionBenchmarks
    {
        private readonly IPipelines pipelines;
        private readonly PipelineContext context;

        public PipelineExecutionBenchmarks()
        {
            IPipelineHandlerProvider[] providersArr = {
                new LegacyAdapter()
            };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            pipelines = new PipelineManager(new NullLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));

            LegacyPrebound = new Lazy<ICompiledPipeline<JObject>>(() => Build("LEGACY"));
            PurePrebound = new Lazy<ICompiledPipeline<JObject>>(() => Build("PURE"));
        }
        
        private Lazy<ICompiledPipeline<JObject>> LegacyPrebound;
        private Lazy<ICompiledPipeline<JObject>> PurePrebound;

        private ICompiledPipeline<JObject> Build(string type)
        {
            IPipelineContext context = new PipelineContext()
                .Set("contentType", "none")
                .Set("id", Guid.Empty)
                .Set("method", "GET")
                .Set("type", type);

            ICompiledPipeline<JObject> pipeline = pipelines
                .For(context, ctx => Task.FromResult(new JObject()));

            return pipeline;
        }

        [Benchmark]
        public void LegacyPipelineAdapter()
        {
            ICompiledPipeline<JObject> pipeline = Build("LEGACY");
            pipeline.Invoke();
        }

        [Benchmark]
        public void PurePipelineAdapter()
        {
            ICompiledPipeline<JObject> pipeline = Build("PURE");
            pipeline.Invoke();
        }


        [Benchmark]
        public void PreboundLegacyPipelineAdapter()
        {
            LegacyPrebound.Value.Invoke();
        }
        [Benchmark]
        public void PreboundPurePipelineAdapter()
        {
            PurePrebound.Value.Invoke();
        }

        [Benchmark]
        public void EmptyPipeline()
        {
            ICompiledPipeline<JObject> pipeline = pipelines
                .For<IPipelineContext, JObject>(new PipelineContext(), ctx => Task.FromResult(new JObject()));
            pipeline.Invoke();
        }
    }


    public class NullLogger : ILogger
    {
        public Task LogAsync(string type, object customData = null) => Task.CompletedTask;
    }

    [PropertyFilter("type", "PURE")]
    public class PureHandler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(IPipelineContext context, INext<JObject, string, Guid> next)
        {
            return await next.Invoke();
        }
    }

    [PropertyFilter("type", "LEGACY")]
    public class LegacyAdapter : PipelineHandler
    {
        public override JObject AfterGet(JObject entity, string contentType)
        {
            return base.AfterGet(entity, contentType);
        }

        public override JObject BeforePost(JObject entity, string contentType)
        {
            return base.BeforePost(entity, contentType);
        }

        public override JObject AfterPost(JObject entity, string contentType)
        {
            return base.AfterPost(entity, contentType);
        }

        public override JObject BeforePut(JObject entity, JObject previous, string contentType)
        {
            return base.BeforePut(entity, previous, contentType);
        }

        public override JObject AfterPut(JObject entity, JObject previous, string contentType)
        {
            return base.AfterPut(entity, previous, contentType);
        }

        public override JObject BeforeDelete(JObject entity, string contentType)
        {
            return base.BeforeDelete(entity, contentType);
        }

        public override JObject AfterDelete(JObject entity, string contentType)
        {
            return base.AfterDelete(entity, contentType);
        }

        public override JObject BeforeRevert(JObject entity, JObject current, string contentType)
        {
            return base.BeforeRevert(entity, current, contentType);
        }

        public override JObject AfterRevert(JObject entity, JObject current, string contentType)
        {
            return base.AfterRevert(entity, current, contentType);
        }
    }
}
