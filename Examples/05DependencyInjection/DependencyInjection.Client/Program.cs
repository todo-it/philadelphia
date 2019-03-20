using System;
using Bridge;
using Bridge.Html5;
using Philadelphia.Common;
using DependencyInjection.Domain;
using Philadelphia.Web;

namespace DependencyInjection.Client {
    public class SomeForm : IForm<HTMLElement,SomeForm,Unit> {
        public SomeForm(IHelloWorldService someService) {
            //here would be some construction body...
        }

        //rest of IForm implementation would be present here...
    }

    public class SomeFlow : IFlow<HTMLElement> {
        private readonly SomeForm _someForm;

        public SomeFlow(IHelloWorldService helloService, SomeForm someForm) {
            _someForm = someForm;
            //here would be rest of constructor code...
        }

        public void Run(IFormRenderer<HTMLElement> renderer, Action atExit) {
            //here would be inter-form navigational logic...
        }
    }

    public class ProgramWithoutRichDi {
        [Ready]
        public static void OnReady() {
            var di = new DiContainer();
            Services.Register(di); //registers discovered services from model

            di.Register<SomeFlow>(LifeStyle.Transient);
            di.Register<SomeForm>(LifeStyle.Transient);

            Toolkit.InitializeToolkit();
            var renderer = Toolkit.DefaultFormRenderer();
            
            di.Resolve<SomeFlow>().Run(renderer); //DI container builds SomeFlow instance itself
        }
    }
}
