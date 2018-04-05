//
// The Visual HEIFLOW License
//
// Copyright (c) 2015-2018 Yong Tian, SUSTech, Shenzhen, China. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Note: only part of the files distributed in the software belong to the Visual HEIFLOW. 
// The software also contains contributed files, which may have their own copyright notices.
//  If not, the GNU General Public License holds for them, too, but so that the author(s) 
// of the file have the Copyright.

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.VisualBasic;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using System.Collections.Generic;
 

namespace Heiflow.Core.Plugin
{
	/// <summary>
	/// Loads plug-in scripts, compiles and executes them
	/// </summary>
	public class PluginCompiler
	{
        IApplication mainApplication;
		const string LogCategory = "PLUG";
		Hashtable codeDomProviders = new Hashtable(); // File Extension -> Compiler table
		CompilerParameters cp = new CompilerParameters();
        List<PluginInfo> m_plugins = new List<PluginInfo>();  
		StringCollection m_worldWindReferencesList = new StringCollection();
		string m_pluginRootDirectory;
        public Log Log
        {
            get;
            set;
        }
		public string PluginRootDirectory
		{
			get
			{
				return m_pluginRootDirectory;
			}
			set
			{
				m_pluginRootDirectory = value;

                try
                {
                    // Create plugin directory
                    if (!Directory.Exists(m_pluginRootDirectory))
                        Directory.CreateDirectory(m_pluginRootDirectory);
                }
                catch
                {
                }
			}
		}

        public Plugin this[string name]
        {
            get
            {
                Plugin p=null;
                foreach (PluginInfo info in Plugins)
                {
                    if (info.Name == name)
                    {
                        p = info.Plugin;
                        break;
                    }
                }
                return p;
            }
        }

		/// <summary>
		/// The list of discovered plugins.
		/// </summary>
		public List<PluginInfo> Plugins
		{
			get
			{
				return m_plugins;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref= "T:HUST.WREIS.PluginEngine.PluginCompiler"/> class.
		/// </summary>
		/// <param name="worldWind"></param>
		/// <param name="pluginDirectory"></param>
        public PluginCompiler(IApplication mainpage, string pluginDirectory)
		{
            this.mainApplication = mainpage;

			// Add the available codeDomProviders
			// TODO: Enumerate codeDomProviders (easier in .net 2.0)
			AddCodeProvider(new Microsoft.CSharp.CSharpCodeProvider() );
			AddCodeProvider(new Microsoft.VisualBasic.VBCodeProvider() );

			// Setup compiler parameters
			cp.GenerateExecutable = false;
			cp.GenerateInMemory = true;
			cp.IncludeDebugInformation = false;

			// Load all assemblies WW has a reference to
			AssemblyName[] assemblyNames = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
			foreach(AssemblyName assemblyName in assemblyNames)
				Assembly.Load(assemblyName);

			// Reference all assemblies WW has loaded
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach(Assembly assembly in assemblies)
			{
				try
				{
					if(assembly.Location.Length > 0)
						m_worldWindReferencesList.Add(assembly.Location);
				}
				catch(NotSupportedException) 
				{
					// In-memory compiled assembly etc.
				}
			}

			PluginRootDirectory = pluginDirectory;
		}

		/// <summary>
		/// Adds a compiler to the list of available codeDomProviders
		/// </summary>
		public void AddCodeProvider( CodeDomProvider cdp )
		{
			// Add leading dot since that's what Path.GetExtension uses
			codeDomProviders.Add("."+cdp.FileExtension, cdp);
		}

        public void SetPluginVisible(string name, bool visible)
        {
            var pl = this[name];
            if (pl != null)
            {
                if (visible)
                    pl.Show();
                else
                    pl.Hide();
            }
        }
        public void SwitchPluginVisible(string name)
        {
            var pl = this[name];
            if (pl != null)
            {
                if (pl.Visible)
                    pl.Hide();
                else
                    pl.Show();
            }
        }
		/// <summary>
		/// Scan for plugins inside an assembly.
		/// </summary>
		public void FindPlugins( Assembly assembly )
		{
			foreach( Type t in assembly.GetTypes() )
			{
				if(!t.IsClass)
					continue;

				if(!t.IsPublic)
					continue;

				if(t.BaseType!=typeof(Plugin))
					continue;

				try
				{
					PluginInfo pi = new PluginInfo();
					pi.Plugin = (Plugin) assembly.CreateInstance( t.ToString() );
					pi.Name = t.Name;
					pi.Description = "internally loaded plugin.";
					m_plugins.Add(pi);
				}
				catch
				{
					// Ignore exceptions during entry point search.
				}
			}
		}

		/// <summary>
		/// Build/update the list of available plugins.
		/// </summary>
		public void FindPlugins()
		{
			if(!Directory.Exists(m_pluginRootDirectory))
				return;

			// Plugins should reside in subdirectories of path
			foreach(string directory in Directory.GetDirectories(m_pluginRootDirectory))
				AddPlugin(directory);

			// Also scan Plugins base directory
			AddPlugin(m_pluginRootDirectory);
		}

		/// <summary>
		/// Adds plugin from one of the plugins
		/// </summary>
		void AddPlugin(string path)
		{
			foreach (string filename in Directory.GetFiles(path))
			{
				bool isAlreadyInList = false;
				foreach(PluginInfo info in m_plugins)
				{
					if(info.FullPath == filename)
					{
						isAlreadyInList = true;
						break;
					}
				}

				if(isAlreadyInList)
					continue;

				string extension = Path.GetExtension(filename).ToLower();
				if(HasCompiler(extension) || IsPreCompiled(extension))
				{
					PluginInfo plugin = new PluginInfo();
					plugin.FullPath = filename;
					m_plugins.Add(plugin);
				}
			}
		}

		/// <summary>
		/// Loads the plugins that are set for load on world startup.
		/// </summary>
		public void LoadStartupPlugins()
		{
			foreach(PluginInfo pi in m_plugins)
			{
				if(pi.IsLoadedAtStartup)
				{
					try
					{
						// Compile
                        Log.Write(Log.Levels.Debug, LogCategory, "loading "+pi.Name+" ...");
                      //  mainApplication.SplashScreen.SetText("Initializing plugin " + pi.Name);
						Load(pi);
					}
					catch(Exception caught)
					{
						// Plugin failed to load
						string message = "Plugin " + pi.Name + " failed: " + caught.Message;
						Log.Write(Log.Levels.Error, LogCategory, message);
						Log.Write(caught);

						// Disable automatic load of this plugin on startup
						pi.IsLoadedAtStartup = false;

					//	mainApplication.SplashScreen.SetError(message);
					}
				}
			}
		}

		/// <summary>
		/// Determine if a file extension is that of a compilable plugin.
		/// </summary>
		/// <param name="fullPath">File extension to check.</param>
		public bool HasCompiler(string fileExtension)
		{
			CodeDomProvider cdp = (CodeDomProvider)codeDomProviders[fileExtension];
			return cdp != null;
		}

		/// <summary>
		/// Determine if a file extension is that of a pre-compiled plugin.
		/// </summary>
		static public bool IsPreCompiled(string fileExtension)
		{
			return fileExtension==".dll";
		}

		/// <summary>
		/// Load a plugin
		/// </summary>
		public void Load(PluginInfo pi)
		{
			if(pi.Plugin == null)
			{
				// Try to find a suitable compiler
				string extension = Path.GetExtension(pi.FullPath).ToLower();
				Assembly asm = null;
				if(extension==".dll")
				{
					// Load pre-compiled assembly
					asm = Assembly.LoadFile(pi.FullPath);
				}
				else
				{
					CodeDomProvider cdp = (CodeDomProvider)codeDomProviders[extension];
					if(cdp==null)
						return;
					asm = Compile(pi, cdp);
				}

				pi.Plugin = GetPluginInterface(asm);
			}

            string pluginPath = mainApplication.ApplicationPath + "\\Plugins";
			if( pi.FullPath != null && pi.FullPath.Length > 0)
				pluginPath = Path.GetDirectoryName(pi.FullPath);

			pi.Plugin.PluginLoad(mainApplication, pluginPath);
		}

		/// <summary>
		/// Unload a plugin if it's loaded.
		/// </summary>
		public void Unload(PluginInfo pi)
		{
			if(!pi.IsCurrentlyLoaded)
				return;
			pi.Plugin.PluginUnload();
		}

		/// <summary>
		/// Uninstall/delete a plugin.
		/// </summary>
		/// <param name="pi"></param>
		public void Uninstall(PluginInfo pi)
		{
			// Unload the plugin
			Unload(pi);

			File.Delete( pi.FullPath );

			m_plugins.Remove( pi );
		}

		/// <summary>
		/// Shut down plugins
		/// </summary>
		public void Dispose()
		{
			foreach(PluginInfo pi in m_plugins)
			{
				try
				{
					Unload(pi);
				}
				catch(Exception caught)
				{
					Log.Write(Log.Levels.Error, "PLUG", "Plugin unload failed: " + caught.Message);
				}
			}
		}

		/// <summary>
		/// Compiles a file to an assembly using specified compiler.
		/// </summary>
		Assembly Compile( PluginInfo pi, CodeDomProvider cdp )
		{
			// Compile
			//ICodeCompiler compiler = cdp.CreateCompiler();

		    cp.CompilerOptions = "/unsafe";

			// Add references
			cp.ReferencedAssemblies.Clear();
			foreach( string reference in m_worldWindReferencesList)
				cp.ReferencedAssemblies.Add(reference);

			// Add reference to core functions for VB.Net users 
			if(cdp is Microsoft.VisualBasic.VBCodeProvider)
				cp.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");

			// Add references specified in the plugin
			foreach( string reference in pi.References.Split(','))
				AddCompilerReference( pi.FullPath, reference.Trim() );

			CompilerResults cr = cdp.CompileAssemblyFromFile( cp, pi.FullPath );
			if(cr.Errors.HasErrors || cr.Errors.HasWarnings)
			{
				// Handle compiler errors
				StringBuilder error = new StringBuilder();
				foreach (CompilerError err in cr.Errors)
				{
					string type = (err.IsWarning ? "Warning" : "Error");
					if(error.Length>0)
						error.Append(Environment.NewLine);
					error.AppendFormat("{0} {1}: Line {2} Column {3}: {4}", type, err.ErrorNumber, err.Line, err.Column, err.ErrorText );
				}
                if(cr.Errors.HasErrors)
				    throw new Exception( error.ToString() );
			}

			// Success, return our new assembly
			return cr.CompiledAssembly;
		}

		/// <summary>
		/// Adds reference to a local assembly or an assembly in the global cache.
		/// </summary>
		/// <param name="pluginDirectory">Local directory to search.</param>
		/// <param name="assemblyName">Partial name of the assembly.</param>
		void AddCompilerReference( string pluginDirectory, string assemblyName )
		{
			try
			{
				if(assemblyName.Length<=0)
					return;

				Assembly referencedAssembly = Assembly.Load(assemblyName);
				if(referencedAssembly == null)
				{
					// Try plug-in directory
					string pluginReferencePath = Path.Combine( Path.GetDirectoryName(pluginDirectory),
						assemblyName );
					referencedAssembly = Assembly.LoadFile(pluginReferencePath);

					if(referencedAssembly == null)
						throw new ApplicationException("Search for required library '" + assemblyName + "' failed.");
				}

				cp.ReferencedAssemblies.Add( referencedAssembly.Location );
			}
			catch(Exception caught)
			{
				throw new ApplicationException("Failed to load '"+assemblyName+"': "+caught.Message);
			}
		}

		/// <summary>
		/// Looks for class derived from Plugin and returns an instance of this class.
		/// </summary>
		static Plugin GetPluginInterface(Assembly asm)
		{
			foreach( Type t in asm.GetTypes() )
			{
				if(!t.IsClass)
					continue;

				if(!t.IsPublic)
					continue;

				if(t.BaseType!=typeof(Plugin))
					continue;

				try
				{
					Plugin pluginInstance = (Plugin) asm.CreateInstance( t.ToString() );	
					return pluginInstance;
				}
				catch(MissingMethodException)
				{
					throw;
				}
				catch
				{
					// Ignore exceptions during entry point search.
				}
			}

			throw new ArgumentException( "Plugin does not derive from base class Plugin." );
		}
	}
}