using System;
using System.Threading.Tasks;

namespace DotJEM.Pipelines
{

    public interface ICompiledPipeline<T>
    {
        Task<T> Invoke();
    }

    public class CompiledPipeline<T> : ICompiledPipeline<T>
    {
        private readonly IPipelineContextCarrier<T> carrier;
        private readonly IUnboundPipeline<T> pipeline;

        public CompiledPipeline(IUnboundPipeline<T> pipeline, IPipelineContextCarrier<T> carrier)
        {
            this.pipeline = pipeline;
            this.carrier = carrier;
        }
        
        public Task<T> Invoke() => pipeline.Invoke(carrier);

        public override string ToString()
        {
            return $"{carrier}{Environment.NewLine}{pipeline}";
        }
    }
}