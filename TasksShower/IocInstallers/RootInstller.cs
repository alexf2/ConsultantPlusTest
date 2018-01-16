using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;

using Ifx.NLogFactory;
using Ifx.FoundationHelpers.General;
using ConsPlus.TaskShowerRuntime.Views;


namespace ConsPlus.TasksShower.IocInstallers
{
    public sealed class RootInstller : IWindsorInstaller
    {
        public void Install (IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<IAbstractLoggerFactory>().ImplementedBy<NLogFactory>(),
                Classes.FromAssemblyContaining<ConsPlus.TaskShowerRuntime.Models.FileSystemModel>().Where((t) => t != typeof(MainForm)).WithService.DefaultInterfaces().LifestyleTransient()
            );
        }
    }
}
