﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotJEM.Pipelines.Attributes;

namespace DotJEM.Pipelines.Factories
{
    public interface IPipelineHandlerCollection : IEnumerable<IPipelineHandlerProvider>
    {
    }

    public class PipelineHandlerCollection : IPipelineHandlerCollection
    {
        private readonly List<IPipelineHandlerProvider> providers;

        public PipelineHandlerCollection(IPipelineHandlerProvider[] providers)
        {
            this.providers = OrderHandlers(providers);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IPipelineHandlerProvider> GetEnumerator()
        {
            return providers.GetEnumerator();
        }

        private List<T> OrderHandlers<T>(T[] steps)
        {
            Queue<T> queue = new(steps);
            Dictionary<Type, T> map = steps.ToDictionary(h => h.GetType());
            HashSet<Type> ordered = new();
            while (queue.Count > 0)
            {
                T handler = queue.Dequeue();
                Type handlerType = handler.GetType();
                PipelineDepencency[] dependencies = PipelineDepencency.GetDepencencies(handler);
                if (dependencies.Length < 1 || dependencies.All(d => ordered.Contains(d.Type)))
                {
                    ordered.Add(handlerType);
                }
                else
                {
                    IEnumerable<PipelineDepencency> unknownDependencies = dependencies
                        .Where(dep => !map.ContainsKey(dep.Type))
                        .ToArray();
                    if (unknownDependencies.Any())
                    {
                        string message = $"{handlerType.FullName} has dependencies to be satisfied, missing dependencies:" +
                                         $"\n\r{string.Join("\n\r - ", unknownDependencies.Select(d => d.Type.FullName))}";
                        throw new PipelineDependencyResolutionException(message);
                    }
                    queue.Enqueue(handler);
                }
            }
            return ordered.Select(type => map[type]).ToList();
        }
    }

    public class PipelineDependencyResolutionException : Exception
    {
        public PipelineDependencyResolutionException(string message) : base(message)
        {
        }
    }
}