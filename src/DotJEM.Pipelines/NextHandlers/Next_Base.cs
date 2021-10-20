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
        protected IPipelineContextCarrier<TResult> Carrier { get; }
        protected IPipelineContext Context => Carrier.Context;
        protected INode<TResult> NextNode { get; }

        public Next(IPipelineContextCarrier<TResult> carrier, INode<TResult> next)
        {
            this.Carrier = carrier;
            this.NextNode = next;
        }

        public Task<TResult> Invoke() => NextNode.Invoke(Carrier);
    }
}
