using System.Threading.Tasks;
using DotJEM.Pipelines.Nodes;

namespace DotJEM.Pipelines.NextHandlers
{
    public interface INext<TResult>
    {
        Task<TResult> Invoke();
    }

    public class Next<TResult> : INext<TResult>
    {
        protected INode<TResult> NextNode { get; }
        protected IPipelineContext Context { get; }

        public Next(IPipelineContext context, INode<TResult> next)
        {
            this.Context = context;
            this.NextNode = next;
        }

        public Task<TResult> Invoke() => NextNode.Invoke(Context);
    }
}
