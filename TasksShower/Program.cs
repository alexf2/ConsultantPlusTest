using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.ExceptionServices;
using NLog;

using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.MicroKernel.Releasers;
using Castle.MicroKernel.Registration;
using Ifx.FoundationHelpers.General;

using ConsPlus.TaskShowerModel;
using ConsPlus.TaskShowerRuntime.Views;

namespace TasksShower
{
    static class Program
    {
        const string LoggerName = "Main";
        static readonly Logger _logger = LogManager.GetLogger(LoggerName);
        
        [STAThread]
        [HandleProcessCorruptedStateExceptions]
        static void Main ()
        {
            _logger.Info("Application is being started...");
            try
            {
                Application.ThreadException += application_ThreadException;
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += currentDomain_UnhandledException;
                TaskScheduler.UnobservedTaskException += taskScheduler_UnobservedTaskException;

                using (WindsorContainer container = createIoC())
                {
                    using (ITaskShowerController ctrl = container.Resolve<ITaskShowerController>())
                    {                        
                        Application.Run((Form)ctrl.View);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Can't start application: see the log file");
                _logger.LogException(LogLevel.Error, "Can't start application", ex);
            }
            _logger.Info("Application finished");
        }

        static WindsorContainer createIoC ()
        {
            WindsorContainer cont = new WindsorContainer();
            cont.Install(FromAssembly.This());
            cont.Kernel.ReleasePolicy = new NoTrackingReleasePolicy();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            cont.Register(
                Component.For<IShowerView>().Instance(
                    new MainForm(cont.Resolve<IAbstractLoggerFactory>().GetLogger(LoggerName))
                )
            );

            return cont;
        }

        static void application_ThreadException (object sender, ThreadExceptionEventArgs e)
        {            
            _logger.LogException(LogLevel.Error, "Unhandled Thread Exception", e.Exception);
        }

        static void currentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
        {            
            if (e.ExceptionObject is Exception)
                _logger.LogException(LogLevel.Error, "Unhandled Domain Exception", e.ExceptionObject as Exception);
            else
                _logger.Error("Unhandled Domain Exception: {0}", e.ExceptionObject.ToString());
        }

        static void taskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs arg)
        {
            arg.Exception.Handle((ex) =>
            {
                _logger.LogException(LogLevel.Error, "Unobserved task's exception", ex);
                return true;
            });
            arg.SetObserved();
        }
    }
}
