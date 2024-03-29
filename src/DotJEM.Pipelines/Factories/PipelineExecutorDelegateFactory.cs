﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DotJEM.Pipelines.Attributes;
using DotJEM.Pipelines.NextHandlers;
using DotJEM.Pipelines.Nodes;

namespace DotJEM.Pipelines.Factories
{
    public interface IPipelineExecutorDelegateFactory
    {
        MethodNode<T> CreateNode<T, TContext>(object target, MethodInfo method, PipelineFilterAttribute[] filters);
        PipelineExecutorDelegate<T> CreateInvocator<T, TContext>(object target, MethodInfo method);
        Expression<PipelineExecutorDelegate<T>> BuildLambda<T, TContext>(object target, MethodInfo method);
        NextFactoryDelegate<T> CreateNextFactoryDelegate<T>(MethodInfo method);
        Expression<NextFactoryDelegate<T>> CreateNextFactoryDelegateExpression<T>(MethodInfo method);
    }

    public class PipelineExecutorDelegateFactory : IPipelineExecutorDelegateFactory
    {
        private static readonly MethodInfo contextParameterGetter = typeof(IPipelineContext).GetMethod("Get");

        public MethodNode<T> CreateNode<T,TContext>(object target, MethodInfo method, PipelineFilterAttribute[] filters)
        {
            PipelineExecutorDelegate<T> @delegate = CreateInvocator<T, TContext>(target, method);
            NextFactoryDelegate<T> nextFactory = CreateNextFactoryDelegate<T>(method);

            string parameters = string.Join(", ", method.GetParameters().Select(param => $"{param.ParameterType.Name} {param.Name}"));
            string signature = $"{ target.GetType().Name}.{method.Name}({parameters})";
            return new MethodNode<T>(filters, @delegate, nextFactory, signature);
        }

        public PipelineExecutorDelegate<T> CreateInvocator<T, TContext>(object target, MethodInfo method)
        {
            Expression<PipelineExecutorDelegate<T>> lambda = BuildLambda<T, TContext>(target, method);
            return lambda.Compile();
        }

        public Expression<PipelineExecutorDelegate<T>> BuildLambda<T, TContext>(object target, MethodInfo method)
        {
            ConstantExpression targetParameter = Expression.Constant(target);
            //ParameterExpression contextParameter = Expression.Parameter(typeof(TContext), "context");
            ParameterExpression contextParameter = Expression.Parameter(typeof(IPipelineContext), "context");
            ParameterExpression nextParameter = Expression.Parameter(typeof(INext<T>), "next");

            // context.GetParameter("first"), ..., context, (INextHandler<...>) next);
            List<Expression> parameters = BuildParameterList<TContext>(method, contextParameter, nextParameter);
            UnaryExpression convertTarget = Expression.Convert(targetParameter, target.GetType());
            MethodCallExpression methodCall = Expression.Call(convertTarget, method, parameters);
            UnaryExpression castMethodCall = Expression.Convert(methodCall, typeof(Task<T>));
            return Expression.Lambda<PipelineExecutorDelegate<T>>(castMethodCall, contextParameter, nextParameter);
        }

        private List<Expression> BuildParameterList<TContext>(MethodInfo method, Expression contextParameter, Expression nextParameter)
        {
            // Validate that method's signature ends with Context and Next.
            ParameterInfo[] list = method.GetParameters();
            ParameterInfo contextParameterInfo = list[list.Length - 2];

            Expression inputContextParameter = contextParameter;
            if (contextParameterInfo.ParameterType != typeof(IPipelineContext))
                inputContextParameter = Expression.Convert(inputContextParameter, contextParameterInfo.ParameterType);

            Type contextType = typeof(TContext);
            if (contextType != typeof(IPipelineContext))
                contextParameter = Expression.Convert(contextParameter, contextType);
            
            return list
                .Take(list.Length - 2)
                .Select(info =>
                {
                    string pname = $"{char.ToUpperInvariant(info.Name[0])}{info.Name.Substring(1)}";
                    PropertyInfo propertyInfo = contextType.GetProperty(pname);
                    if (propertyInfo != null)
                    {
                        // context.Name;
                        MemberExpression property = Expression.Property(contextParameter, propertyInfo);
                        if (info.ParameterType.IsAssignableFrom(propertyInfo.PropertyType))
                            return property;

                        // (parameterType) context.GetParameter("name"); 
                        return Expression.Convert(property, info.ParameterType);
                    }

                    // context.GetParameter("name");
                    MethodCallExpression call = Expression.Call(contextParameter, contextParameterGetter, Expression.Constant(info.Name));

                    // (parameterType) context.GetParameter("name"); 
                    return (Expression)Expression.Convert(call, info.ParameterType);
                })
                .Append(inputContextParameter)
                .Append(Expression.Convert(nextParameter, list.Last().ParameterType))
                .ToList();
        }

        public NextFactoryDelegate<T> CreateNextFactoryDelegate<T>(MethodInfo method)
        {
            Expression<NextFactoryDelegate<T>> lambda = CreateNextFactoryDelegateExpression<T>(method);
            return lambda.Compile();
        }

        public Expression<NextFactoryDelegate<T>> CreateNextFactoryDelegateExpression<T>(MethodInfo method)
        {
            ParameterInfo[] list = method.GetParameters();
            ParameterInfo nextParameterInfo = list[list.Length - 1];
            Type[] generics = nextParameterInfo.ParameterType.GetGenericArguments();

            ParameterExpression carrierParameter = Expression.Parameter(typeof(IPipelineContextCarrier<T>), "carrier");
            ParameterExpression nodeParameter = Expression.Parameter(typeof(INode<T>), "node");

            Expression[] arguments = list
                .Take(list.Length - 2)
                .Select(p => (Expression)Expression.Constant(p.Name))
                .Prepend(nodeParameter)
                .Prepend(carrierParameter)
                .ToArray();
            MethodCallExpression methodCall = Expression.Call(typeof(NextFactory), nameof(NextFactory.Create), generics, arguments);

            return Expression.Lambda<NextFactoryDelegate<T>>(methodCall, carrierParameter, nodeParameter);
        }
    }
}