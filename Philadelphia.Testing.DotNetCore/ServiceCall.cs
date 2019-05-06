using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Philadelphia.Common;

namespace Philadelphia.Testing.DotNetCore {
    public class ServiceCall {
        public string FullInterfaceName {get; set;}
        public string MethodName {get; set;}
        public object[] Params {get; set;}
        
        public override bool Equals(object other) {
            if (!(other is ServiceCall)) {
                return false;
            }

            var o = (ServiceCall)other;

            return 
                FullInterfaceName == o.FullInterfaceName &&
                MethodName == o.MethodName &&
                Params.IsTheSameAs(o.Params);
        }

        public override int GetHashCode() {
            return 
                FullInterfaceName.GetHashCode() +
                MethodName.GetHashCode() +
                Params.Sum(x => x?.GetHashCode() ?? 0);
        }

        public override string ToString() {
            return $"<ServiceCall FullInterfaceName={FullInterfaceName} MethodName={MethodName} Params={Params.PrettyToString()}>";
        }

        private static ServiceCall OfMethodCallExpression(MethodCallExpression body) {
            return HttpServiceUtil.OfMethodCallExpression(body, (m, i, p) => 
                new ServiceCall {
                    MethodName = m.Name,
                    FullInterfaceName = i.FullName,
                    Params = p.ToArray()
                }
            );
        }

        /*
         * for regular service calls
         */
        public static ServiceCall OfMethod<A,B,C,D,E,V>(Expression<Func<A,B,C,D,E,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static ServiceCall OfMethod<A,B,C,D,V>(Expression<Func<A,B,C,D,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static ServiceCall OfMethod<A,B,C,V>(Expression<Func<A,B,C,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static ServiceCall OfMethod<A,B,V>(Expression<Func<A,B,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static ServiceCall OfMethod<A,V>(Expression<Func<A,Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static ServiceCall OfMethod<V>(Expression<Func<Task<V>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }
        
        /*
         * for server sent events calls
         */
        public static ServiceCall OfMethod<CtxT,MsgT>(Expression<Func<CtxT,Func<MsgT,bool>>> inp) {
            if (!(inp.Body is MethodCallExpression)) {
                throw new Exception("expected method call as expression's body");
            }
            
            return OfMethodCallExpression((MethodCallExpression)inp.Body);
        }

        public static implicit operator CsChoice<ServiceCall,FilterInvocation>(ServiceCall inp) {
            return CsChoice<ServiceCall,FilterInvocation>.Create(inp);
        }
    }
}
