using System.Linq;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.Factories;
using NUnit.Framework;

namespace DotJEM.Pipelines.Test
{
    [TestFixture]
    public class PipelineHandlerCollectionTest
    {
        [Test]
        public void Ctor_SimpleDependentModules_OrdersHandlers()
        {
            PipelineHandlerCollection set = new PipelineHandlerCollection(new IPipelineHandlerProvider[]
            {
                new Fourth(),
                new Fifth(),
                new Second(),
                new Third(),
                new First()
            });

            Assert.That(set.Select(x => x.GetType()), Is.EquivalentTo(new[]
            {
                typeof(First),
                typeof(Second),
                typeof(Third),
                typeof(Fourth),
                typeof(Fifth)
            }));
        }

        [Test]
        public void Ctor_MultiDependentModules_OrdersHandlers()
        {
            PipelineHandlerCollection set = new PipelineHandlerCollection(new IPipelineHandlerProvider[]
            {
                new NeedsFirstAndThird(),
                new Fourth(),
                new Fifth(),
                new Second(),
                new Third(),
                new First()
            });

            Assert.That(set.Select(x => x.GetType()), Is.EquivalentTo(new[]
            {
                typeof(First),
                typeof(Second),
                typeof(Third),
                typeof(NeedsFirstAndThird),
                typeof(Fourth),
                typeof(Fifth)
            }));
        }

        [Test]
        public void Ctor_MissingDependency_ThrowsException()
        {
            Assert.That(() => new PipelineHandlerCollection(new IPipelineHandlerProvider[]
            {
                new NeedsFirstAndThird(),
                new Fourth(),
                new Fifth(),
                new Second(),
                new First()
            }), Throws.TypeOf<PipelineDependencyResolutionException>());
        }
        public class First : IPipelineHandlerProvider { }

        [PipelineDepencency(typeof(First))]
        public class Second : IPipelineHandlerProvider { }

        [PipelineDepencency(typeof(First))]
        [PipelineDepencency(typeof(Third))]
        public class NeedsFirstAndThird : IPipelineHandlerProvider { }

        [PipelineDepencency(typeof(Second))]
        public class Third : IPipelineHandlerProvider { }

        [PipelineDepencency(typeof(Third))]
        public class Fourth : IPipelineHandlerProvider { }

        [PipelineDepencency(typeof(Fourth))]
        public class Fifth : IPipelineHandlerProvider { }
    }
}