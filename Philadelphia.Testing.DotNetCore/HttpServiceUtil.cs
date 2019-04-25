using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Philadelphia.Common;

namespace Philadelphia.Testing.DotNetCore {
    public static class HttpServiceUtil {

        public static T OfMethodCallExpression<T>(
            MethodCallExpression body, 
            Func<MethodInfo,Type,List<object>,T> bld) {

            var m = body.Method;
            var i = m.DeclaringType;

            var prms = body.Arguments.Select(ExpressionExtensions.ValueOf).ToList();

            if (!i.IsInterface) {
                throw new Exception("expected method's declaring type to be service interface");
            }

            if (!i.CustomAttributes.Any(x => x.AttributeType == typeof(HttpService))) {
                throw new Exception(
                    $"expected method's declaring type to be decorated with {nameof(HttpService)} attribute");
            }

            return bld(m, i, prms);
        }

    }
}
