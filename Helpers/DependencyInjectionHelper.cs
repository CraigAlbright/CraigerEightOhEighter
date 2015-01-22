using Autofac;
using Autofac.Core.Lifetime;
using CraigerEightOhEighter.Models;
using CraigerEightOhEighter.ViewModels;

namespace CraigerEightOhEighter.Helpers
{
    public class DependencyInjectionHelper
    {
        public IContainer Container { get; set; }
        public DependencyInjectionHelper()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<MainUiViewModel>().SingleInstance();
            builder.RegisterType<Mixer>().As<IMixer>();
            Container = builder.Build();
        }
    }
}
