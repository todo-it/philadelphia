using Philadelphia.Server.Common;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ControlledByTests.Api {
    public class FilterInvocation {
        public FilterInvocationType InvType {get; set;}
        public string FullInterfaceNameOrNull {get; set;}
        public string MethodNameOrNull {get; set;}
        public string Guid {get; set;}
        public string Url {get; set;}
        public ResourceType ResType {get; set;}
        public FilterInvocation BeginBy {get; set;}

        public FilterInvocation Clone() {
            return new FilterInvocation {
                InvType = InvType,
                FullInterfaceNameOrNull = FullInterfaceNameOrNull,
                MethodNameOrNull = MethodNameOrNull,
                Guid = Guid,
                Url = Url,
                ResType = ResType,
                BeginBy = BeginBy
            };
        }

        public static implicit operator CsChoice<ServiceCall,FilterInvocation>(FilterInvocation inp) {
            return CsChoice<ServiceCall,FilterInvocation>.Create(inp);
        }

        public override string ToString() {
            return $"<FilterInvocation Url={Url} ResourceType={ResType} InvType={InvType} FullInterfaceName={FullInterfaceNameOrNull} MethodName={MethodNameOrNull} Guid={Guid}>";
        }
        
        public override bool Equals(object other) {
            if (!(other is FilterInvocation)) {
                return false;
            }

            var o = (FilterInvocation)other;

            return InvType == o.InvType &&
                   FullInterfaceNameOrNull == o.FullInterfaceNameOrNull &&
                   MethodNameOrNull == o.MethodNameOrNull && 
                   (InvType != FilterInvocationType.AfterConnection ||
                    (BeginBy?.Guid ?? Guid) == (o.BeginBy?.Guid ?? o.Guid) );
        }

        public override int GetHashCode() {
            return InvType.GetHashCode() + 
                   (FullInterfaceNameOrNull?.GetHashCode() ?? 0) +
                   (MethodNameOrNull?.GetHashCode() ?? 0);
        }

        
        private static FilterInvocation OfMethodCallExpression(MethodCallExpression body) {
            return HttpServiceUtil.OfMethodCallExpression(body, (m, i, _) => 
                new FilterInvocation {
                    InvType = FilterInvocationType.BeforeConnection,
                    FullInterfaceNameOrNull = i.FullName,
                    MethodNameOrNull = m.Name,
                    ResType = ResourceType.RegularPostService
                }
            );
        }

        public static FilterInvocation OfMethod<A,B,C,D,E,V>(Expression<Func<A,B,C,D,E,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation OfMethod<A,B,C,D,V>(Expression<Func<A,B,C,D,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation OfMethod<A,B,C,V>(Expression<Func<A,B,C,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation OfMethod<A,B,V>(Expression<Func<A,B,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation OfMethod<A,V>(Expression<Func<A,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation OfMethod<V>(Expression<Func<Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static FilterInvocation ExpectOnConnectionAfterFor(FilterInvocation started) {
            var res = started.Clone();
            res.InvType = FilterInvocationType.AfterConnection;
            res.BeginBy = started;
            return res;
        }
    }
}
