using System;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.NextHandlers;
using Newtonsoft.Json.Linq;

namespace DotJEM.Pipelines.Test.Fakes
{
    public class FakeThirdTarget : IPipelineHandlerProvider
    {
        [PropertyFilter("test", ".*")]
        public Task<JObject> Run(int id, string name, FakeContext context, INext<JObject, int, string> next)
        {
            Console.WriteLine($"FakeSecondTarget.Run({id}, {name})");
            return next.Invoke();
        }

    }
}