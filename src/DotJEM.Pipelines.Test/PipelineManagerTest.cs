using System.Threading.Tasks;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.Test.Fakes;
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
        [Test]
        public void Manager_ReturnsDelegat2e()
        {

            PipelineManager manager = new PipelineManager(new FakeLogger(), new PipelineGraphFactory(new PipelineHandlerCollection(new IPipelineHandlerProvider[]
            {
                new FakeFirstTarget(),
                new FakeSecondTarget(),
                new FakeThirdTarget()
            }), new PipelineExecutorDelegateFactory()));
            IPipelineContext context = new FakeContext()
                .Set("id", 42)
                .Set("name", "Foo")
                .Set("test", "x");

            manager.For(context, async fakeContext => new JObject()).Invoke();
            manager.For(context, async fakeContext => new JObject()).Invoke();
            manager.For(context, async fakeContext => new JObject()).Invoke();
            manager.For(context, async fakeContext => new JObject()).Invoke();

        }
    }
}