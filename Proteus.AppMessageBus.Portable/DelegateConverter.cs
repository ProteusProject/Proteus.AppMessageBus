using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Proteus.Infrastructure.Messaging.Portable
{
    /*
     * borrowed from http://codebetter.com/gregyoung/2009/10/03/delegate-mapper/
     */
    [DebuggerStepThrough]
    public class DelegateConverter
    {
        public static Action<TBase> CastArgument<TBase, TDerived>(Expression<Action<TDerived>> source) where TDerived : TBase
        {
            if (typeof(TDerived) == typeof(TBase))
            {
                return (Action<TBase>)((Delegate)source.Compile());

            }
            ParameterExpression sourceParameter = Expression.Parameter(typeof(TBase), "source");
            var result = Expression.Lambda<Action<TBase>>(
                Expression.Invoke(
                    source,
                    Expression.Convert(sourceParameter, typeof(TDerived))),
                sourceParameter);
            return result.Compile();
        }
    }
}