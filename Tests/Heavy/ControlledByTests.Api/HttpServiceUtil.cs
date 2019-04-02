using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Philadelphia.Common;

namespace ControlledByTests.Api {
    public static class HttpServiceUtil {
        public static T OfMethodCallExpression<T>(
            MethodCallExpression body, 
            Func<MethodInfo,Type,List<object>,T> bld) {

            var m = body.Method;
            var i = m.DeclaringType;

            var prms = 
                body.Arguments
                    .Select(x => {
                        //constant (unlikely)
                        if (x is ConstantExpression) {
                            return ((ConstantExpression)x).Value;
                        }

                        //or simple variable
                        if (!(x is MemberExpression)) {
                            throw new Exception("could not treat parameter as MemberExpression");
                        }

                        var mx = (MemberExpression)x;
                        if (!(mx.Member is FieldInfo)) {
                            throw new Exception("parameter's field Member is not FieldInfo");
                        }

                        if (mx.Expression == null) {
                            var fi = (FieldInfo)mx.Member;
                            return fi.GetValue(mx);
                        }

                        if (!(mx.Expression is ConstantExpression)) {
                            throw new Exception("parameter's field Expression is not ConstantExpression");
                        }

                        var cmxe = (ConstantExpression)mx.Expression;
                        var fmx = (FieldInfo)mx.Member;
                        return fmx.GetValue(cmxe.Value);
                    })
                    .ToList();

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
