using System;
using System.Linq.Expressions;
using AgileObjects.ReadableExpressions;
using DotJEM.Pipelines.Factories;
using DotJEM.Pipelines.Test.Fakes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Pipelines.Test
{
    [TestFixture]
    public class PipelineExecutorDelegateFactoryTest
    {

        [Test]
        public void CreateInvocator_ReturnsDelegate()
        {
            PipelineExecutorDelegateFactory factory = new PipelineExecutorDelegateFactory();

            FakeFirstTarget target = new FakeFirstTarget();
            PipelineExecutorDelegate<JObject> action = factory.CreateInvocator<JObject, FakeContext>(target, target.GetType().GetMethod(nameof(FakeFirstTarget.Run)));
            IPipelineContext context = new FakeContext()
                .Set("id", 42)
                .Set("name", "Foo");
            action(context, new FakeNextHandler<JObject, int, string>());
        }

        [Test]
        public void CreateInvocator_ReturnsDelegat2e()
        {

            PipelineExecutorDelegateFactory factory = new PipelineExecutorDelegateFactory();


            FakeFirstTarget target = new FakeFirstTarget();
            Expression<NextFactoryDelegate<JObject>> exp = factory.CreateNextFactoryDelegateExpression<JObject>(target.GetType().GetMethod(nameof(FakeFirstTarget.Run)));

            Console.WriteLine(exp.ToReadableString());

            exp.Compile();

        }



    }
}