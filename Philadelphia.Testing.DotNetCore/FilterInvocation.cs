﻿using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Philadelphia.Server.Common;

namespace Philadelphia.Testing.DotNetCore {
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

            var sameGuids = InvType != FilterInvocationType.AfterConnection;

            if (InvType == FilterInvocationType.AfterConnection) {
                var f = (BeginBy?.Guid ?? Guid);
                var s = (o.BeginBy?.Guid ?? o.Guid);

                if (f == null || s == null) {
                    throw new Exception("when comparing FilterInvocation for AfterConnection GUID must be present");
                }

                sameGuids = f == s;
            }

            return InvType == o.InvType &&
                   FullInterfaceNameOrNull == o.FullInterfaceNameOrNull &&
                   MethodNameOrNull == o.MethodNameOrNull && 
                   sameGuids;
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

        /*
         * for regular service calls
         */
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
        
        /*
         * for server sent events calls
         */
        public static FilterInvocation OfMethod<CtxT,MsgT>(Expression<Func<CtxT,Func<MsgT,bool>>> inp) {
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
