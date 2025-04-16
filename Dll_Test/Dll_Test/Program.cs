using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Dll_Test
{
	internal static class Program
	{
		private static void ApplicationStart()
		{
			Mutex mutex = new Mutex( true, "DLL_Test" );
			if( !mutex.WaitOne( TimeSpan.Zero, true ) ) {
				MessageBox.Show( "Already Dll_Test.exe Running..." );
				mutex.ReleaseMutex();
				return;
			}
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new Form1() );

			mutex.ReleaseMutex();
		}

		private static Assembly ResolveAssembly( object sender, ResolveEventArgs args )
		{
			Assembly thisAssembly = Assembly.GetExecutingAssembly();
			string name = args.Name.Substring( 0, args.Name.IndexOf( ',' ) ) + ".dll";
			List<string> resources = thisAssembly.GetManifestResourceNames().Where( s => s.EndsWith( name ) ).ToList();
			if( !resources.Any() ) return null;
			string resourceName = resources.First();
			using( Stream stream = thisAssembly.GetManifestResourceStream( resourceName ) ) {
				if( stream == null ) return null;
				var block = new byte[ stream.Length ];
				stream.Read( block, 0, block.Length );
				return Assembly.Load( block );
			}
		}

		private static void CurrentDomain_UnhandledException( object sender, UnhandledExceptionEventArgs e )
		{
			Debug.WriteLine( e.ExceptionObject );
			// log
		}

		/// <summary>
		/// 해당 애플리케이션의 주 진입점입니다.
		/// </summary>
		[STAThread]
		static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			ApplicationStart();
		}
	}
}
