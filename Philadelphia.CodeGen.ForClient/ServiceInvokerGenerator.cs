using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Philadelphia.CodeGen.ForClient {    
    // REVIEW: use pascal case 
    // REVIEW: use readonly, as each field is initialized only once
    class UploadDownloadHandlerDto {
        public MethodInfo getter;
        public MethodInfo setter;
    }

    public delegate void Trace(string msg);

    public static class ServiceInvokerGenerator {
        private static string GenerateDependencyInjection<T>(System.Reflection.Assembly assembly) {
            var result = new StringBuilder();

            // REVIEW AppendLine
            result.Append(@"    public class Services {
            public static void Register(IDiContainer container) {");
            result.Append("\r\n");
                
            var services = 
                assembly
                    .GetTypes()
                    .Where(y => 
                        y.IsInterface && 
                        y.GetCustomAttributes(typeof(T), false).Any() )
                    .OrderBy(x => x.FullName);

            foreach (var srv in services) {
                //REVIEW: C# string interpolation $"{}"
                result.Append(string.Format(
                    "                container.RegisterFactoryMethod<{0}>(injector => new WebClient{1}(), Philadelphia.Common.LifeStyle.Singleton);\r\n",
                    srv.FullName,
                    srv.Name.TrimStart('I')
                ));
            }

            result.Append(@"            }
        }");
        
            return result.ToString();
        }
        
        //type, type's depth returns should-continue-visiting and outcome(success/failure)
        private static T VisitType<T>(Type t, Func<Type,int,Tuple<bool,T>> visitorTrueToBreak, int depth = 0) {
            var result = visitorTrueToBreak(t, depth);

            if (!result.Item1) {
                return result.Item2;
            }

            if (t.IsGenericType) {	
                foreach (var subt in t.GetGenericArguments()) {
                    result = visitorTrueToBreak(subt, depth+1);
                    if (!result.Item1) {
                        return result.Item2;
                    }
                }
            }

            return result.Item2;
        }

        private static string GetGenericTypeName(Type t, StringBuilder result = null) {
            if (result == null) {
                result = new StringBuilder();
            }

            if (!t.IsGenericType) {
                result.Append(t.FullName);
                return result.ToString();
            }

            result.Append(t.FullName.Substring(0, t.FullName.IndexOf("`")));
        
            result.Append("<");
            var comma = false;
            foreach (var subt in t.GetGenericArguments()) {
                if (comma) {
                    result.Append(",");
                }
                GetGenericTypeName(subt, result);
                comma = true;
            }
                
            result.Append(">");
        
            return result.ToString();
        }
        
        // REVIEW: FileT not used
        private static string GenerateUploadDownloadProxies<AttrT,FileT>(Assembly assembly) {
            var result = new StringBuilder();
                
            var services = 
                assembly
                    .GetTypes()
                    .Where(y => 
                        y.IsInterface && 
                        y.GetCustomAttributes(typeof(AttrT), false).Any() )
                    .OrderBy(x => x.FullName);

            foreach (var srv in services) {			
                var getterAndSetter = new Dictionary<string,UploadDownloadHandlerDto>();
                var i=0;

                foreach (var method in srv.GetMethods().OrderBy(x => x.Name)) {	
                    UploadDownloadHandlerDto info;
                    string fldName = null;

                    if (method.Name.EndsWith("Getter")) {
                        fldName = method.Name.Substring(0, method.Name.Length - "Getter".Length);
                    
                        if (!getterAndSetter.TryGetValue(fldName, out info)) {
                            info = new UploadDownloadHandlerDto();
                            getterAndSetter.Add(fldName, info);
                        }
                        info.getter = method;
                    } else if (method.Name.EndsWith("Setter")) {
                        fldName = method.Name.Substring(0, method.Name.Length - "Setter".Length);
                    
                        if (!getterAndSetter.TryGetValue(fldName, out info)) {
                            info = new UploadDownloadHandlerDto();
                            getterAndSetter.Add(fldName, info);
                        }
                        info.setter = method;
                    }
                }
            
                foreach (var gas in getterAndSetter) {
                    var clazzName = srv.Name + "_" + gas.Key;

                    result.Append("    public class "+clazzName+" : Philadelphia.Web.BaseDownloadUploadHandler {\n");
                    result.Append("        public "+clazzName+"(");

                    List<ParameterInfo> parms = null;
                    if (gas.Value.getter != null && gas.Value.getter.GetParameters().Any()) {
                        parms = gas.Value.getter.GetParameters().Skip(1).ToList();
                    } else if (gas.Value.setter != null && gas.Value.setter.GetParameters().Any()) {
                        parms = gas.Value.setter.GetParameters().Skip(1).ToList();
                    }

                    if (parms == null) {
                        result.Append("    }\n");
                        continue;
                    }

                    if (parms.Count > 0) {
                        // REVIEW: use local var not global: for(var i = ...) with parm[i]
                        i=0;
                        foreach (var parm in parms) {
                            result.Append("System.Func<");
                            result.Append(GetGenericTypeName(parm.ParameterType));
                            result.Append(">");
                            result.Append(" p" + i++);
                            result.Append(",");
                        }
                        result.Length = result.Length-1;
                    }

                    result.Append(") \n");
                    result.Append("             : base(\n                ");
                    if (parms.Count > 0) {
                        result.Append("() => Bridge.Html5.JSON.Stringify(");
                        if (parms.Count > 1) {
                            result.Append("System.Tuple.Create(");
                        }
                    } else {
                        result.Append("null");
                    }
                
                    // REVIEW: use local var not global: for(var i = ...)
                    i = 0;
                    foreach (var parm in parms) {
                        result.Append(" p" + i++ + "(),");
                    }
                
                    if (parms.Count > 0) {
                        result.Length = result.Length-1;

                        result.Append(")");
                        if (parms.Count > 1) {
                            result.Append(")");
                        }
                    }
                    result.Append(",\n");
                    
                    result.Append("                typeof("+srv.FullName+").FullName,\n");
                    result.Append("                " + (gas.Value.getter == null ? "null" : ("\""+gas.Value.getter.Name+"\""))+ ",\n");
                    result.Append("                " + (gas.Value.setter == null ? "null" : ("\""+gas.Value.setter.Name+"\""))+ ",\n");
                    result.Append("                x => Bridge.Html5.JSON.Stringify(");

                    if (parms.Count >= 1) {
                        result.Append("System.Tuple.Create(");
                    }
                    result.Append("x");

                    if (parms.Count > 0) {
                        // REVIEW: use local var not global: for(var i = ...)
                        i=0;
                        result.Append(",");
                        foreach (var parm in parms) {
                            result.Append(" p" + i++ + "(),");
                        }
                        result.Length = result.Length-1;
                    }
                    if (parms.Count >= 1) {
                        result.Append(")");
                    }
                    result.Append(") ) {}\n");		
                    result.Append("    }\n");
                }
            }

            return result.ToString();
        }

        private static string GenerateRegularProxies<AttrT,FileT>(Assembly assembly, Trace trace) {
            var result = new StringBuilder();
            trace("Looking for services");
            var services = 
                assembly
                    .GetTypes()
                    .Where(y => 
                        y.IsInterface && 
                        y.GetCustomAttributes(typeof(AttrT), false).Any() )
                    .OrderBy(x => x.FullName);

            foreach (var srv in services) {
                trace($"Handling service {srv.FullName}");
                // REVIEW: string interpolation
                result.Append(string.Format("    public class WebClient{0} : {1} {{\r\n",srv.Name.TrimStart('I'), srv.FullName));

                foreach (var method in srv.GetMethods().OrderBy(x => x.Name)) {
                    //trace($"Checking method: {method.Name}, return type {method.ReturnType}");
                    trace($"Checking method: {method}");
                    var isUpload = (method.GetParameters().Any() && method.GetParameters().First().ParameterType == typeof(Philadelphia.Common.UploadInfo));
                    var isFilter = method.ReturnType.GetGenericTypeDefinition() == typeof(Func<object,object>).GetGenericTypeDefinition();

                    result.Append("            public ");

                    if (!isUpload && !isFilter) {
                        result.Append("async ");
                    }

                    result.Append(GetGenericTypeName(method.ReturnType));
                
                    var isArray = VisitType(method.ReturnType, (t, d) => {
                        if (d == 1) {
                            return Tuple.Create(false, t.IsArray);
                        }
                                        
                        return Tuple.Create(d <= 1, false);
                    });

                    var isFile = VisitType(method.ReturnType, (t, d) => {
                        if (d == 1) {
                            return Tuple.Create(false, typeof(FileT).IsAssignableFrom(t));
                        }
                                        
                        return Tuple.Create(d <= 1, false);
                    });
                
                    var returnInnerTypeNameRaw = VisitType(method.ReturnType, (t, d) => {
                        if (d == 1) {
                            return Tuple.Create(false, GetGenericTypeName(t));
                        }
                                        
                        return Tuple.Create<bool,string>(d <= 1, null);
                    });
                
                    var includeResultTypeInCall = !isFile;

                    var returnInnerTypeName = !isArray ? 
                        returnInnerTypeNameRaw 
                        : 
                        returnInnerTypeNameRaw.Substring(0, returnInnerTypeNameRaw.IndexOf('['));

                    result.Append(method.Name);
                
                    result.Append("(");				
                    var i = 0;
                    var comma = false;
                    foreach (var param in method.GetParameters()) {
                        trace($"Handling parameter {param.Name} of type {param.ParameterType}");
                        if (comma) {
                            result.Append(", ");
                        }
                        result.Append(GetGenericTypeName(param.ParameterType, new StringBuilder()));
                        result.Append(" p"+i);
                        i++;
                        comma = true;
                    }

                    result.Append("){\r\n");
                
                    result.Append("                ");
                    if (isUpload) {
                        result.Append("throw new System.Exception(\"uploads cannot be called this way\");\n");
                    } else if (isArray) {
                        result.Append("return await HttpRequester.RunHttpRequestReturningArray");
                    } else if (isFile) { 
                        result.Append("return await HttpRequester.RunHttpRequestReturningAttachment");
                    } else if (!isFilter) {
                        result.Append("return await HttpRequester.RunHttpRequestReturningPlain");
                    } else {
                        result.Append("throw new System.Exception(\"SSE listener cannot be invoked this way\");\n");
                    }

                    if (!isUpload && !isFilter) {
                        if (!isFile || method.GetParameters().Any()) {
                            result.Append("<");
                        }
                                    
                        comma = false;
                        foreach (var param in method.GetParameters()) {
                            if (comma) {
                                result.Append(", ");
                            }
                            result.Append(GetGenericTypeName(param.ParameterType, new StringBuilder()));
                            comma = true;
                        }

                        if (includeResultTypeInCall) {
                            if (comma) {
                                result.Append(", ");
                            }
                            result.Append(returnInnerTypeName);
                        }
                
                        if (!isFile || method.GetParameters().Any()) {
                            result.Append(">");
                        }

                        result.Append("(\r\n                    typeof(");

                        result.Append(srv.FullName);
                        result.Append(").FullName");
                        result.Append(",\r\n                    \"");
                        result.Append(method.Name);
                        result.Append("\"");

                        i = 0;
                        foreach (var param in method.GetParameters()) {
                            // REVIEW: use C# string interpolation aka $"{bbb} ccc"
                            result.Append(string.Format(", p{0}", i++));
                        }

                        result.Append(");\r\n");
                    }
                    result.Append("            }\r\n");
                }

                result.Append("        }\r\n");
            }

            return result.ToString();
        }
        
        private static string GenerateProxies<AttrT,FileT>(Assembly assembly, Trace trace) {
            return 
                GenerateRegularProxies<AttrT,FileT>(assembly, trace) + 
                "\n" +
                GenerateUploadDownloadProxies<AttrT,FileT>(assembly);
        }
        
        private static bool IsFilterMethod(MethodInfo method) {
            return 
                method.ReturnType.GetGenericTypeDefinition() == typeof(Func<object,object>).GetGenericTypeDefinition() &&
                method.GetParameters().Length == 1;
        }

        private static string GenerateServerSideEventsSubscribents<AttrT>(Assembly assembly) {
            var result = new StringBuilder();
            var services = 
                assembly
                    .GetTypes()
                    .Where(y => 
                        y.IsInterface && 
                        y.GetCustomAttributes(typeof(AttrT), false).Any() )
                    .OrderBy(x => x.FullName);
        
            foreach (var srv in services) {
                foreach (var method in srv.GetMethods().Where(IsFilterMethod).OrderBy(x => x.Name)) {
                    var notifType = method.ReturnType.GetGenericArguments()[0];
                    var ctxType = method.GetParameters()[0];

                    result.Append($"    public class {srv.Name}_{method.Name}_SseSubscriber : ServerSentEventsSubscriber<{notifType.FullName},{ctxType.ParameterType.FullName}> {{\n");
                    result.Append($"        public {srv.Name}_{method.Name}_SseSubscriber(System.Func<{ctxType.ParameterType.FullName}> ctxProvider, bool autoConnect=true)\n");
                    result.Append($"            : base(autoConnect, typeof({srv.FullName}), \"{method.Name}\", ctxProvider) {{}}\n");
                    result.Append("    }\n");
                }
            }

            return result.ToString();
        }

        public static void GenerateCode(
            string outputCsFilePath, 
            Assembly serviceDeclarationAssembly, 
            string generatedClassesNamespace,
            Trace trace = null) {
            trace = trace ?? (x => { });
            File.WriteAllText(outputCsFilePath, 
                $@"using Philadelphia.Common;
    using Philadelphia.Web;

    namespace {generatedClassesNamespace} {{
    {GenerateProxies<Philadelphia.Common.HttpService,Philadelphia.Common.FileModel>(serviceDeclarationAssembly, trace)}
    {GenerateServerSideEventsSubscribents<Philadelphia.Common.HttpService>(serviceDeclarationAssembly)}
    {GenerateDependencyInjection<Philadelphia.Common.HttpService>(serviceDeclarationAssembly)}
    }}
    ");
        }
    }
}
