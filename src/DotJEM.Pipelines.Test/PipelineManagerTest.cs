using System.Threading.Tasks;
using DotJEM.Pipelines.Factories;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Pipelines.Test
{
    [TestFixture]
    public class PipelineManagerTest
    {
        [Test]
        public void CreateInvocator_ReturnsDelegate()
        {
            IPipelineHandlerProvider[] providersArr = new IPipelineHandlerProvider[]
            {
                new FakeFirstTarget(),
                new FakeSecondTarget(),
                new FakeThirdTarget()
            };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            IPipelines pipelines = new PipelineManager(new FakeLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));
            IPipelineContext context = new FakeContext()
                .Set("id", 42)
                .Set("name", "Foo")
                .Set("test", "Foo");


            ICompiledPipeline<JObject> pipeline = pipelines
                .For<FakeContext, JObject>((FakeContext)context, ctx => Task.FromResult(new JObject()));
            pipeline.Invoke();
        }
    }
}