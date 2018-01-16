using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

using Ifx.FoundationHelpers.General;
using Ifx.FoundationHelpers.RuntimeModel;

namespace Ifx.FoundationHelpers.WinServices
{
	public abstract class WinServiceHostBootstrap: IServiceHostBootstrap
	{		
		static readonly Regex _exCmd = new Regex(@"/(?'key'\w+)(\x3A\s*(?'params'[\w\,\s]+))?(\s+|$)", RegexOptions.CultureInvariant|RegexOptions.ExplicitCapture|RegexOptions.Singleline);

		readonly protected IAbstractLogger _logger;		
		readonly Dictionary<string, CommandSwitch> _commands = new Dictionary<string, CommandSwitch>(StringComparer.InvariantCultureIgnoreCase);
		bool _testMode; //если true, то старт сервиса в тестовом режиме
		bool _donotStartService; //используется командами, которые предусматривают иные действия, чем запуск сервиса

		public WinServiceHostBootstrap (string serviceName, IAbstractLogger logger)
		{
			if (logger == null)
				throw new ArgumentException("Logger is not specified)");
			_logger = logger;

			ServiceName = serviceName;

			RegisterCommandSwitch("reg", "Registering service", (x) => {_donotStartService = true; registerWinService(true);});
			RegisterCommandSwitch("service", "Registering service", (x) => {_donotStartService = true; registerWinService(true);});
			RegisterCommandSwitch("regserver", "Registering service", (x) => {_donotStartService = true; registerWinService(true);});

			RegisterCommandSwitch("unreg", "Unregistering service", (x) => {_donotStartService = true; registerWinService(false);});
			RegisterCommandSwitch("unregserver", "Unregistering service", (x) => {_donotStartService = true; registerWinService(false);});

			RegisterCommandSwitch("test", "Starting as a console application", (x) => _testMode = true);
		}

		#region IServiceHostBootstrap
		public void Setup ()
		{
			_logger.Info("Setting up bootstrap");
			Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
			AppDomain.CurrentDomain.UnhandledException += currentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += tasks_UnobservedException;
			onSetup();
			_logger.Info("Bootstrap was setted up");
		}
		public int Run (string[] commandLineArgs = null)
		{
			try {
				_logger.Info("Processing command line switches");
				processCommandLine(commandLineArgs);
				_logger.Info("Command line switches were processed");
			}
			catch (Exception exc)
			{
				Environment.ExitCode = -1;
				Console.WriteLine("During processing command line exception happened: {0}", exc.ToString());
				return Environment.ExitCode;
			}

			if (!_donotStartService)
			{
				bool running = false;
				try {
					_logger.Info("Service host is being constructed");
					using (IRuntimeHost host = constructServiceHost())
					{
						_logger.Info("Service host was constructed");
						running = true;
						_logger.Info("Service host is being started");
						host.Run(_testMode, commandLineArgs);
						_logger.Info("Service host was finished");
					}
				}
				catch (Exception ex)
				{
					Environment.ExitCode = -2;
					_logger.Exception(running ? "Starting host":"Constructing host", ex);
					if (_testMode)
						Console.WriteLine("{0}: {1}", running ? "Starting host":"Constructing host", ex);
				}
			}
			return Environment.ExitCode;
		}
		public void Close ()
		{
			_logger.Info("Bootstrap is being closed");
			onClose();
            TaskScheduler.UnobservedTaskException -= tasks_UnobservedException;
			AppDomain.CurrentDomain.UnhandledException -= currentDomain_UnhandledException;            
			_logger.Info("Bootstrap was closed");
		}
		public string ServiceName
		{
			get;
			private set;
		}
		public IEnumerable<CommandSwitch> Commands
		{
			get {
				return _commands.Values;
			}
		}
		#endregion IServiceHostBootstrap

		/// <summary>
		/// Фабричный метод для создания хоста Windows-сервиса
		/// </summary>		
		protected abstract IRuntimeHost constructServiceHost ();

		protected virtual void onSetup ()
		{
		}
		protected virtual void onBeforeRegisterWinService (bool register)
		{
		}
		protected virtual void onAfterRegisterWinService (bool register, bool success)
		{
		}
		protected virtual void onClose ()
		{
		}
		protected virtual void onPrintUsage ()
		{
			Console.WriteLine("{0}: {1}\r\nUsage:\r\n", AssemblyAttributes.Product, AssemblyAttributes.Copyright);
			int i = 0;
			foreach (CommandSwitch kv in _commands.Values)
				Console.WriteLine("\t{0} - {1}{2}", kv.Name, kv.Description, ++i == _commands.Count ? ".":";");
		}

		void registerWinService (bool register)
		{
			onBeforeRegisterWinService(register);

			Console.WriteLine("Service '{0}' is being {1}", ServiceName, register ? "registered":"unregistered");
			try {
				if (register)
					ServiceRegistrationUtility.RegisterAll();
				else
					ServiceRegistrationUtility.UnregisterAll();

				Console.WriteLine("Service '{0}' was {1} succesfully", ServiceName, register ? "registered":"unregistered");

				onAfterRegisterWinService(register, true);
			}
			catch (Exception ex)
			{
				Console.WriteLine("On service {0}:\r\n{1}", register ? "registration":"unregistration", ex.ToString());
				onAfterRegisterWinService(register, false);				
			}
		}

		protected void RegisterCommandSwitch (string swt, string description, Action<string> onSwitch)
		{
			if (_commands.ContainsKey(swt))
				throw new Exception(string.Format("Command line switch '{0}' already exists", swt));
			_commands.Add(swt, new CommandSwitch(){Name = swt, Description = description, Verb = onSwitch});
		}

        void tasks_UnobservedException (object sender, UnobservedTaskExceptionEventArgs args)
        {
            if (!args.Observed)
            {
                args.SetObserved();
                if (args.Exception is AggregateException)
                    ((AggregateException)args.Exception).Handle(ex =>
                    {
                        _logger.Exception("Unabserved task exception", ex);
                        return true;
                    });
                else
                    _logger.Exception("Unabserved task exception", args.Exception);
            }
        }

		void currentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e)
		{			
			if (e.ExceptionObject is Exception)
            {
				_logger.Exception("Unhandled exception", e.ExceptionObject as Exception);
			}
			else
			{
				_logger.Error("Unknown exception");
			}
		}

		void processCommandLine (string[] commandLineArgs)
		{
			string[] args = commandLineArgs ?? Environment.GetCommandLineArgs();
			foreach (string arg in args)
			{
				foreach (Match m in _exCmd.Matches(arg))
					if (m.Success)
					{
						CommandSwitch swt;
						if (_commands.TryGetValue(m.Groups["key"].Value.Trim(), out swt))
						{
							if (swt.Verb != null)
								swt.Verb(m.Groups["params"].Value.Trim());
						}
						else
						{
							_donotStartService = true;
							Console.WriteLine("Unrecognized command line switch '{0}'.", m.Groups["key"].Value.Trim());
							Environment.ExitCode = -1;
						}
					}
			}
		}
	}

}
