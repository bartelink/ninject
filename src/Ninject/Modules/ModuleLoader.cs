#region License
// 
// Author: Nate Kohari <nate@enkari.com>
// Copyright (c) 2007-2010, Enkari, Ltd.
// 
// Dual-licensed under the Apache License, Version 2.0, and the Microsoft Public License (Ms-PL).
// See the file LICENSE.txt for details.
// 
#endregion
#if !NO_ASSEMBLY_SCANNING
#region Using Directives
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !NO_WEB
using System.Text.RegularExpressions;
using System.Web;
#endif
using Ninject.Components;
using Ninject.Infrastructure;
#endregion

namespace Ninject.Modules
{
	/// <summary>
	/// Automatically finds and loads modules from assemblies.
	/// </summary>
	public class ModuleLoader : NinjectComponent, IModuleLoader
	{
		/// <summary>
		/// Gets or sets the kernel into which modules will be loaded.
		/// </summary>
		public IKernel Kernel { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ModuleLoader"/> class.
		/// </summary>
		/// <param name="kernel">The kernel into which modules will be loaded.</param>
		public ModuleLoader(IKernel kernel)
		{
			Ensure.ArgumentNotNull(kernel, "kernel");
			Kernel = kernel;
		}

		/// <summary>
		/// Loads any modules found in the files that match the specified patterns.
		/// </summary>
		/// <param name="patterns">The patterns to search.</param>
		public void LoadModules(IEnumerable<string> patterns)
		{
			var plugins = Kernel.Components.GetAll<IModuleLoaderPlugin>();

			var fileGroups = patterns
				.SelectMany(pattern => GetFilesMatchingPattern(pattern))
				.GroupBy(filename => Path.GetExtension(filename).ToLowerInvariant());

			foreach (var fileGroup in fileGroups)
			{
				string extension = fileGroup.Key;
				IModuleLoaderPlugin plugin = plugins.Where(p => p.SupportedExtensions.Contains(extension)).FirstOrDefault();

				if (plugin != null)
					plugin.LoadModules(fileGroup);
			}
		}

		private static string[] GetFilesMatchingPattern(string pattern)
		{
			string path = NormalizePath(Path.GetDirectoryName(pattern));
			string glob = Path.GetFileName(pattern);

			return Directory.GetFiles(path, glob);
		}

		private static string NormalizePath(string path)
		{
			if (!Path.IsPathRooted(path))
				path = Path.Combine(GetBaseDirectory(), path);

			return Path.GetFullPath(path);
		}

		private static string GetBaseDirectory()
		{
			string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string searchPath = AppDomain.CurrentDomain.RelativeSearchPath;

			return String.IsNullOrEmpty(searchPath) ? baseDirectory : Path.Combine(baseDirectory, searchPath);
		}
	}
}
#endif //!NO_ASSEMBLY_SCANNING