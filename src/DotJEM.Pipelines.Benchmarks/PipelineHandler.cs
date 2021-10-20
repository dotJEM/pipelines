using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.NextHandlers;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Benchmarks
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HttpMethodFilterAttribute : PropertyFilterAttribute
    {
        public HttpMethodFilterAttribute(string method, RegexOptions options = RegexOptions.None)
            : base("method", $"^{method}$", options | RegexOptions.IgnoreCase)
        {
        }
    }


    public abstract class PipelineHandler : IPipelineHandlerProvider
    {
        [HttpMethodFilter("GET")]
        public async Task<JObject> Get(string contentType, Guid id, IPipelineContext context, INext<JObject, string, Guid> next)
        {
            JObject entity = await next.Invoke();
            return AfterGet(entity, contentType, context);
        }

        [HttpMethodFilter("POST")]
        public async Task<JObject> Post(string contentType, JObject entity, IPipelineContext context, INext<JObject, string, JObject> next)
        {
            entity = BeforePost(entity, contentType, context);
            entity = await next.Invoke(contentType, entity);
            return AfterPost(entity, contentType, context);
        }

        [HttpMethodFilter("PUT")]
        public async Task<JObject> Put(string contentType, Guid id, JObject entity, JObject previous, IPipelineContext context, INext<JObject, string, Guid, JObject, JObject> next)
        {
            entity = BeforePut(entity, previous, contentType, context);
            entity = await next.Invoke(contentType, id, entity, previous);
            return AfterPut(entity, previous, contentType, context);
        }

        [HttpMethodFilter("DELETE")]
        public async Task<JObject> Put(string contentType, Guid id, JObject previous, IPipelineContext context, INext<JObject, string, Guid, JObject> next)
        {
            previous = BeforeDelete(previous, contentType, context);
            previous = await next.Invoke(contentType, id, previous);
            return AfterDelete(previous, contentType, context);
        }

        [PropertyFilter("type", "REVERT")]
        public async Task<JObject> Revert(string contentType, Guid id, JObject target, JObject current, IPipelineContext context, INext<JObject, string, Guid, JObject, JObject> next)
        {
            target = BeforeRevert(target, current, contentType, context);
            target = await next.Invoke(contentType, id, target, current);
            return AfterRevert(target, current, contentType, context);
        }

        public virtual JObject AfterGet(JObject entity, string contentType, IPipelineContext context) => AfterGet(entity, contentType);
        public virtual JObject AfterGet(JObject entity, string contentType) => entity;
        public virtual JObject BeforePost(JObject entity, string contentType, IPipelineContext context) => BeforePost(entity, contentType);
        public virtual JObject BeforePost(JObject entity, string contentType) => entity;
        public virtual JObject AfterPost(JObject entity, string contentType, IPipelineContext context) => AfterPost(entity, contentType);
        public virtual JObject AfterPost(JObject entity, string contentType) => entity;
        public virtual JObject BeforePut(JObject entity, JObject previous, string contentType, IPipelineContext context) => BeforePut(entity, previous, contentType);
        public virtual JObject BeforePut(JObject entity, JObject previous, string contentType) => entity;
        public virtual JObject AfterPut(JObject entity, JObject previous, string contentType, IPipelineContext context) => AfterPut(entity, previous, contentType);
        public virtual JObject AfterPut(JObject entity, JObject previous, string contentType) => entity;
        public virtual JObject BeforeDelete(JObject entity, string contentType, IPipelineContext context) => BeforeDelete(entity, contentType);
        public virtual JObject BeforeDelete(JObject entity, string contentType) => entity;
        public virtual JObject AfterDelete(JObject entity, string contentType, IPipelineContext context) => AfterDelete(entity, contentType);
        public virtual JObject AfterDelete(JObject entity, string contentType) => entity;
        public virtual JObject BeforeRevert(JObject entity, JObject current, string contentType, IPipelineContext context) => BeforeRevert(entity, current, contentType);
        public virtual JObject BeforeRevert(JObject entity, JObject current, string contentType) => entity;
        public virtual JObject AfterRevert(JObject entity, JObject current, string contentType, IPipelineContext context) => AfterRevert(entity, current, contentType);
        public virtual JObject AfterRevert(JObject entity, JObject current, string contentType) => entity;
    }
}