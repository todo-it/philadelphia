using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Philadelphia.Common {
    public static class ExpressionUtil {
        public static string ExtractFieldName<C, V>(Expression<Func<C, V>> getField) => ExtractField(getField).Name;

        public static MemberInfo ExtractField<C, V>(Expression<Func<C, V>> getField) {
            var member = getField.Body as MemberExpression;

            if (member == null) {
                //it can be that there was an implicit conversion made f.e. from T[] into IEnumerable<T>. Try it to be sure
                var implConv = getField.Body as UnaryExpression;

                if (implConv != null) {
                    var innerMember = implConv.Operand as MemberExpression;

                    if (innerMember != null) {
                        return innerMember.Member;
                    }
                }

                throw new ArgumentException("getField expression is not of expected type MemberExpression");
            }

            return member.Member;
        }
    }
}
