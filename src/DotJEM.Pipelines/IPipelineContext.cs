using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotJEM.Pipelines
{
    public interface IPipelineContext
    {
        object this[string key] { get; set; }

        object Get(string key);
        bool TryGetValue(string key, out object value);

        IPipelineContext Replace(params (string key, object value)[] values);
        IPipelineContext Add(string key, object value);
        IPipelineContext Set(string key, object value);
        IPipelineContext Remove(string key);
    }

    public class PipelineContext : IPipelineContext
    {
        protected readonly Dictionary<string, object> parameters = new();

        public object this[string key]
        {
            get => parameters[key];
            set => parameters[key] = value;
        }

        public PipelineContext(params (string key, object value)[] values)
        {
            foreach ((string key, object value) in values)
                parameters[key] = value;
        }

        public virtual bool TryGetValue(string key, out object value) => parameters.TryGetValue(key, out value);

        public object Get(string key)
        {
            return parameters.TryGetValue(key, out object value) ? value : null;
        }

        public IPipelineContext Replace(params (string key, object value)[] values)
        {
            foreach ((string key, object value) in values)
            {
                if (!parameters.ContainsKey(key))
                    throw new MissingMemberException($"The given key '{key}' was not found in the context.");
                parameters[key] = value;
            }
            return this;
        }

        public IPipelineContext Add(string key, object value)
        {
            parameters.Add(key, value);
            return this;
        }

        public IPipelineContext Set(string key, object value)
        {
            parameters[key] = value;
            return this;
        }

        public IPipelineContext Remove(string key)
        {
            parameters.Remove(key);
            return this;
        }


        public override string ToString()
        {
            return parameters.Aggregate(new StringBuilder($"{GetType().Name}"), (sb, pair) => sb.AppendLine($"  -> {pair.Key} == {pair.Value}")).ToString();
        }
    }
}