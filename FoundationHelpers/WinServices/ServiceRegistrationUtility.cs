using System;
using System.Configuration.Install;
using System.Reflection;
using System.Security.Principal;
using System.Diagnostics;
using System.Linq;

namespace Ifx.FoundationHelpers.WinServices
{
	/// <summary>
	/// Вспомогательный класс для запуска инстоллеров приложения.
	/// </summary>
    public static class ServiceRegistrationUtility
    {
        /// <summary>
        /// Регистрирует севис в сервис-менеджере и выполняет другие инсталляторы, найденные в сборке.
        /// </summary>
        public static void RegisterAll ()
        {			
			if (ensureElevatedPrivilegies())
			{
				try {
					ManagedInstallerClass.InstallHelper(new []{ExecutableName});
				}
				finally {
					Console.WriteLine("...>");
					Console.ReadKey();
				}
			}
        }

        /// <summary>
        /// Разрегистрирует севис в сервис-менеджере и выполняет другие инсталляторы, найденные в сборке.
        /// </summary>
        public static void UnregisterAll ()
        {
			if (ensureElevatedPrivilegies())
			{
				try {
					ManagedInstallerClass.InstallHelper(new []{"/u", ExecutableName});
				}
				finally {
					Console.WriteLine("...>");
					Console.ReadKey();
				}
			}
        }

		/// <summary>
		/// Имя и путь главного исполняемого файла приложения.
		/// </summary>
		private static string ExecutableName
		{
			get {
				return Assembly.GetEntryAssembly().Location;
			}
		}

		static int _requrseCount;
		static bool ensureElevatedPrivilegies ()
		{
			//AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);

			WindowsIdentity identity = WindowsIdentity.GetCurrent();

			if (identity != null)
			{
				if (!new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator) && _requrseCount++ == 0)
				{
					string[] args = Environment.GetCommandLineArgs();
					string argsV = string.Empty;
					if (args != null && args.Length > 1)
						argsV = string.Join(" ", args.Skip(1));

					ProcessStartInfo startInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().Location, argsV)
                    {
                        Verb = "runas",
                        UseShellExecute = true,
                        CreateNoWindow = true						
                    };	

					Process process = Process.Start(startInfo);
					process.EnableRaisingEvents = true;
                    process.WaitForExit();

					return false;
				}
			}
			return true;
		}
    }

}
