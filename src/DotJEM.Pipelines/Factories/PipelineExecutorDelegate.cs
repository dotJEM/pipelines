using System.Threading.Tasks;
using DotJEM.Pipelines.NextHandlers;

namespace DotJEM.Pipelines.Factories
{
    public delegate Task<T> PipelineExecutorDelegate<T>(IPipelineContext context, INext<T> next);
}