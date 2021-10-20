using System;
using System.Threading.Tasks;
using DotJEM.Diagnostic;

namespace DotJEM.Pipelines.Test.Fakes
{
    public class FakeLogger : ILogger
    {
        public Task LogAsync(string type, object customData = null)
        {
            Console.WriteLine(type);
            return Task.CompletedTask;
        }
    }
}