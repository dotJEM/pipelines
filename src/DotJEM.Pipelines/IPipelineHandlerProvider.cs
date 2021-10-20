namespace DotJEM.Pipelines
{
    // Note: 
    // This is a marker interface for registering Pipeline handlers within IoC containers.
    // Strictly speaking, this is not a concern for this project, so if we can find a suidable way for dependant projects to quickly wire
    // this up, e.g. by using a factory or custom implementation of the collection, then deleting this would be fair game. 
    public interface IPipelineHandlerProvider
    {
    }
}