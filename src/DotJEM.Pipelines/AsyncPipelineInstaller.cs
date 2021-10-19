using DotJEM.Pipelines.Factories;

namespace DotJEM.Pipelines
{
    public class AsyncPipelineInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IPipelines>().ImplementedBy<PipelineManager>().LifestyleTransient());
            container.Register(Component.For<IPipelineGraphFactory>().ImplementedBy<PipelineGraphFactory>().LifestyleTransient());
            container.Register(Component.For<IPipelineExecutorDelegateFactory>().ImplementedBy<PipelineExecutorDelegateFactory>());
            container.Register(Component.For<IPipelineHandlerCollection>().ImplementedBy<PipelineHandlerCollection>().LifestyleTransient());
        }
    }


    /* NEW CONCEPT: Named pipelines */


    //Task<JObject> Execute<TContext>(TContext context, Func<TContext, JObject> finalize) where TContext : IPipelineContext;
}
