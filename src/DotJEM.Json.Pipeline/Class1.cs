using System;
using Newtonsoft.Json.Linq;

namespace DotJEM.Json.Pipeline
{
    public interface IPipelineContext
    {
        JObject Next(JObject value);
    }

    public interface IPipelineHandler
    {
        JObject Execute(JObject value, IPipelineContext context);
    }

    public class ContentFilterAttribute : Attribute
    {
        public string Selector { get; }
        public string Filter { get; }

        public ContentFilterAttribute(string selector, string filter)
        {
            Selector = selector;
            Filter = filter;
        }
    }

    public class ContextFilterAttribute : Attribute
    {
        public string Selector { get; }
        public string Filter { get; }
        public string Target { get; set; }
        
        public ContextFilterAttribute(string selector, string filter)
        {
            Selector = selector;
            Filter = filter;
        }
    }

    [ContentFilter("contentType", "notification")]
    public class PipelineModule
    {
        [ContextFilter("Method", "GET")]
        public JObject ExecuteGet(JObject value, IPipelineContext context)
        {
            return value;
        }
    }
}
