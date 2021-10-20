using System;
using System.Threading.Tasks;
using DotJEM.Pipelines.NextHandlers;

namespace DotJEM.Pipelines.Test.Fakes
{
    public class FakeNextHandler<TResult, T1, T2> : INext<TResult, T1, T2>
    {
        public Task<TResult> Invoke()
        {
            Console.WriteLine($"FakeNextHandler.Invoke()");
            return Task.FromResult(default(TResult));
        }

        public Task<TResult> Invoke(T1 arg1, T2 arg2)
        {
            Console.WriteLine($"FakeNextHandler.Invoke({arg1}, {arg2})");
            return Task.FromResult(default(TResult));
        }
    }
}
