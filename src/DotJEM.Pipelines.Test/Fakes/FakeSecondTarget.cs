using System;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.NextHandlers;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Test.Fakes
{
    [PropertyFilter("name", ".*")]
    public class FakeSecondTarget : IPipelineHandlerProvider
    {
        [PropertyFilter("test", ".*")]
        public Task<JObject> Run(int id, string name, IPipelineContext context, INext<JObject, int, string> next)
        {
            Console.WriteLine($"FakeSecondTarget.Run({id}, {name})");
            return next.Invoke(50, "OPPS");
        }

    }
}