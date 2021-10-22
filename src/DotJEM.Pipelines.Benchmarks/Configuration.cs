using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using DotJEM.Diagnostic;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.NextHandlers;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Benchmarks
{
    public class EmptyLogger : ILogger
    {
        public Task LogAsync(string type, object customData = null) => Task.CompletedTask;
    }

    [PropertyFilter("type", "^PURE$")]
    public class PureHandler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(IPipelineContext context, INext<JObject> next)
        {
            context.Set("type", "PURE.GET");
            return await next.Invoke();
        }
    }

    [PropertyFilter("type", "^PURE1$")]
    public class Pure1Handler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(string type, IPipelineContext context, INext<JObject, string> next)
        {
            return await next.Invoke("PURE1.GET");
        }
    }

    [PropertyFilter("type", "^PURE2$")]
    public class Pure2Handler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(string type, Guid id, IPipelineContext context, INext<JObject, string, Guid> next)
        {
            return await next.Invoke("PURE2.GET", id);
        }
    }

    [PropertyFilter("type", "^PURE3$")]
    public class Pure3Handler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(string type, Guid id, string method, IPipelineContext context, INext<JObject, string, Guid, string> next)
        {
            return await next.Invoke("PURE3.GET", id, method);
        }
    }

    [PropertyFilter("type", "^LEGACY$")]
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
