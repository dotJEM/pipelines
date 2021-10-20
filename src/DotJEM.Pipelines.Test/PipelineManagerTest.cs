using System;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.NextHandlers;
using DotJEM.Pipelines.Test.Fakes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Pipelines.Test
{
    [TestFixture]
    public class PipelineManagerTest
    {
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

        [Test]
        public void For_Context_ReturnsCompiledPipeline()
        {
            IPipelineHandlerProvider[] providersArr = { new FakeFirstTarget(), new FakeSecondTarget(), new FakeThirdTarget() };
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
        public async Task For_AlterCompletion_ReturnsCompiledPipeline()
        {
            IPipelineHandlerProvider[] providersArr = { new FakeFirstTarget(), new FakeSecondTarget(), new FakeThirdTarget() };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            IPipelines pipelines = new PipelineManager(new FakeLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));
            IPipelineContext context = new FakeContext()
                .Set("id", 42)
                .Set("name", "Foo")
                .Set("test", "Foo");

            ICompiledPipeline<JObject> pipelineA = pipelines
                .For<FakeContext, JObject>((FakeContext)context, ctx => Task.FromResult(JObject.FromObject(new { Source="First" })));
            JObject resultA = await pipelineA.Invoke();
            
            ICompiledPipeline<JObject> pipelineB = pipelines
                .For<FakeContext, JObject>((FakeContext)context, ctx => Task.FromResult(JObject.FromObject(new {Source="Second" })));
            JObject resultB = await pipelineB.Invoke();

            Assert.That((string)resultA["Source"], Is.EqualTo("First"));
            Assert.That((string)resultB["Source"], Is.EqualTo("Second"));
        }
        
        [Test]
        public async Task For_DifferentContexts_ReturnsCompiledPipelineWorking()
        {
            IPipelineHandlerProvider[] providersArr = { new DifferentContextProvider() };
            IPipelineHandlerCollection providers = new PipelineHandlerCollection(providersArr);
            IPipelines pipelines = new PipelineManager(new FakeLogger(), new PipelineGraphFactory(providers, new PipelineExecutorDelegateFactory()));
            IPipelineContext contextA = new FakeAContext()
                .Set("id", 42)
                .Set("name", "contextA")
                .Set("context", "A");
            IPipelineContext contextB = new FakeBContext()
                .Set("id", 42)
                .Set("name", "contextB")
                .Set("context", "B");

            ICompiledPipeline<JObject> pipelineA = pipelines
                .For((FakeAContext)contextA, ctx => Task.FromResult(JObject.FromObject(new { ctx.Name })));
            JObject resultA = await pipelineA.Invoke();
            
            ICompiledPipeline<JObject> pipelineB = pipelines
                .For((FakeBContext)contextB, ctx => Task.FromResult(JObject.FromObject(new { ctx.Name })));
            JObject resultB = await pipelineB.Invoke();

            Console.WriteLine(resultA);
            Console.WriteLine(resultB);
        }

        public class DifferentContextProvider : IPipelineHandlerProvider
        {
            [PropertyFilter("context", "A")]
            public Task<JObject> RunA(int id, string name, FakeAContext context, INext<JObject, int, string> next)
            {
                Console.WriteLine($"FakeSecondTarget.RunA({id}, {name})");
                return next.Invoke(50, $"{name}.RunA");
            }

            [PropertyFilter("context", "B")]
            public Task<JObject> RunB(int id, string name, FakeBContext context, INext<JObject, int, string> next)
            {
                Console.WriteLine($"FakeSecondTarget.RunB({id}, {name})");
                return next.Invoke(50, $"{name}.RunB");
            }

        }

        public class FakeAContext : PipelineContext
        {
            public string Name => (string)Get("name");
        }
        public class FakeBContext : PipelineContext 
        {
            public string Name => (string)Get("name");

        }


    }
}