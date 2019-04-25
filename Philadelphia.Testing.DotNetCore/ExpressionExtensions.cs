using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Philadelphia.Testing.DotNetCore {
    public static class ExpressionExtensions {
        public static string Describe(this Expression expr) => $"{expr?.GetType().Name}({expr}:{expr?.Type})";
        public static object ValueOf(this Expression expr) {
            switch (expr) {
                case ConstantExpression x: return x.Value;
                case MemberExpression x:
                    switch (x.Member) {
                        case FieldInfo m: return m.GetValue(ValueOf(x.Expression));
                        case PropertyInfo m: return m.GetValue(ValueOf(x.Expression));
                        default:
                            throw new Exception($"Member expresion not supported for member type {x.Member.GetType().Name}\n{Describe(expr)}");
                    }
                case null: return null;
                default: throw new Exception("Expresion not supported: " + Describe(expr));
            }
        }
    }
}
