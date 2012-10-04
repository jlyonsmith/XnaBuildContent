using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolBelt;
using System.Xml;
using System.Reflection;
using System.IO;

namespace XnaBuildContent
{
	[CommandLineDescription("Compiler for building XNA content")]
	[CommandLineTitle("XNA Build Content")]
	public class BuildContentTool : ITool, IProcessCommandLine
	{
        #region Fields
		private bool runningFromCommandLine = false;
        
        #endregion

        #region Construction
		public BuildContentTool(IOutputter outputter)
		{
			this.Output = new OutputHelper(outputter);
		}

        #endregion

		[DefaultCommandLineArgument("default", Description = "Input .content data file", ValueHint = "<content-file>", 
            Initializer = typeof(BuildContentTool), MethodName="ParseCommandLineFilePath")]
		public ParsedPath ContentPath { get; set; }

		[CommandLineArgument("properties", ShortName = "p", Description = "Additional properties to set", ValueHint = "<prop1=val1;prop2=val2>")]
		public string Properties { get; set; }

		[CommandLineArgument("help", Description = "Displays this help", ShortName = "?")]
		public bool ShowHelp { get; set; }

		[CommandLineArgument("nologo", Description = "Suppress logo banner")]
		public bool NoLogo { get; set; }

		[CommandLineArgument("rebuild", Description = "Force a rebuild even if all files are up-to-date")]
		public bool Rebuild { get; set; }

		[CommandLineArgument("test", Description = "Test mode.  Indicates what would content will be compiled, but does not actually compile it")]
		public bool TestMode { get; set; }

		public OutputHelper Output { get; set; }

		private CommandLineParser parser;

		public CommandLineParser Parser
		{
			get
			{
				if (parser == null)
					parser = new CommandLineParser(this.GetType());

				return parser;
			}
		}

		public DateTime NewestAssemblyWriteTime { get; set; }
		public DateTime ContentPathWriteTime { get; set; }

		public int ExitCode
		{
			get
			{
				return Output.HasOutputErrors ? 1 : 0;
			}
		}

		public void Execute()
		{
			try
			{
				SafeExecute();
			}
			catch (Exception e)
			{
				if (e is ContentFileException)
				{
					ContentFileException contentEx = (ContentFileException)e;

					Output.Error(contentEx.FileName, contentEx.LineNumber, 0, e.Message);

					while ((e = e.InnerException) != null)
					{
						string message = e.Message;

						if (e is XmlException)
						{
							int n = message.IndexOf("file://");

							if (n != -1)
								message = message.Substring(0, n);
						}

						Output.Error(contentEx.FileName, contentEx.LineNumber, 0, message);

#if DEBUG
						Console.WriteLine(e.StackTrace);
#endif
					}
				}
				else
				{
					Output.Error(e.Message);
				}
			}
		}

		private void SafeExecute()
		{
			if (!NoLogo)
				Console.WriteLine(Parser.LogoBanner);

			if (!runningFromCommandLine)
			{
				Parser.GetTargetArguments(this);
				Output.Message(MessageImportance.Normal, Parser.CommandName + Parser.Arguments);
			}

			if (ShowHelp)
			{
				Console.WriteLine(Parser.Usage);
				return;
			}

			if (String.IsNullOrEmpty(ContentPath))
			{
				Output.Error("A .content file must be specified");
				return;
			}

			this.ContentPath = this.ContentPath.MakeFullPath();

			if (!File.Exists(this.ContentPath))
			{
				Output.Error("Content file '{0}' does not exist", this.ContentPath);
				return;
			}

			// Initialize properties from the environment and command line
			PropertyGroup globalProps = new PropertyGroup();

			globalProps.AddFromEnvironment();
			globalProps.AddWellKnownProperties(
                new ParsedPath(Assembly.GetExecutingAssembly().Location, PathType.File).VolumeAndDirectory,
                ContentPath.VolumeAndDirectory);
			globalProps.AddFromPropertyString(this.Properties);

			BuildContext buildContext = new BuildContext(this.Output, this.ContentPath);

			ContentFileV2 contentFile = null;

			try
			{
				contentFile = ContentFileReaderV2.ReadFile(this.ContentPath);
			}
			catch (Exception e)
			{
				throw new ContentFileException(this.ContentPath, (int)e.Data["LineNumber"], "Problem reading content file", e);
			}
            
			Output.Message(MessageImportance.Low, "Read content file '{0}'", this.ContentPath);

			ItemGroup globalItems = new ItemGroup();

			globalItems.ExpandAndAddFromList(contentFile.Items, globalProps);

			List<CompilerClass> compilerClasses = LoadCompilerClasses(globalItems, globalProps);

			this.NewestAssemblyWriteTime = FindNewestAssemblyWriteTime(compilerClasses);
			this.ContentPathWriteTime = File.GetLastWriteTime(this.ContentPath);

			List<BuildTarget> BuildTargets = PrepareBuildTargets(contentFile.Targets, globalItems, globalProps);

			foreach (var buildTarget in BuildTargets)
			{
				bool compilerFound = false;

				foreach (var compilerClass in compilerClasses)
				{
					if (buildTarget.InputExtensions.SequenceEqual(compilerClass.InputExtensions) &&
						buildTarget.OutputExtensions.SequenceEqual(compilerClass.OutputExtensions))
					{
						compilerFound = true;

						string msg = String.Format("Building target '{0}' with '{1}' compiler", buildTarget.Name, compilerClass.Name);

						foreach (var input in buildTarget.InputFiles)
						{
							msg += Environment.NewLine + "\t" + input;
						}

						msg += Environment.NewLine + "\t->";

						foreach (var output in buildTarget.OutputFiles)
						{
							msg += Environment.NewLine + "\t" + output;
						}

						Output.Message(MessageImportance.Normal, msg);

						if (TestMode)
							continue;

						compilerClass.ContextProperty.SetValue(compilerClass.Instance, buildContext, null);
						compilerClass.TargetProperty.SetValue(compilerClass.Instance, buildTarget, null);

						try
						{
							compilerClass.CompileMethod.Invoke(compilerClass.Instance, null);
						}
						catch (TargetInvocationException e)
						{
							ContentFileException contentEx = e.InnerException as ContentFileException;
                            
							if (contentEx != null)
							{
								contentEx.EnsureFileNameAndLineNumber(buildContext.ContentFile, buildTarget.LineNumber);
								throw contentEx;
							}
							else
							{
								throw new ContentFileException(
									this.ContentPath, buildTarget.LineNumber, "Unable to compile target '{0}'".CultureFormat(buildTarget.Name), e.InnerException);
							}
						}

						// Ensure that the output files were generated
						foreach (var outputFile in buildTarget.OutputFiles)
						{
							if (!File.Exists(outputFile))
							{
								throw new ContentFileException(this.ContentPath, buildTarget.LineNumber, 
									"Output file '{0}' was not generated".CultureFormat(outputFile));
                           	}
						}
					}
				}

				if (!compilerFound)
					Output.Warning("No compiler found for target '{0}' for extensions '{1}' -> '{2}'", 
	               		buildTarget.Name, 
			            String.Join(Path.PathSeparator.ToString(), buildTarget.InputExtensions),
		                String.Join(Path.PathSeparator.ToString(), buildTarget.OutputExtensions));
			}

			Output.Message(MessageImportance.Normal, "Done");
		}

		public static ParsedPath ParseCommandLineFilePath(string value)
		{
			return new ParsedPath(value, PathType.File);
		}

        #region Private Methods
		private List<BuildTarget> PrepareBuildTargets(List<ContentFileV2.Target> rawTargets, ItemGroup globalItems, PropertyGroup globalProps)
		{
			List<BuildTarget> buildTargets = new List<BuildTarget>();

			foreach (var rawTarget in rawTargets)
			{
				try
				{
					PropertyGroup targetProps = new PropertyGroup(globalProps);

					targetProps.Set("TargetName", rawTarget.Name);
					
					if (rawTarget.Properties != null)
						targetProps.ExpandAndAddFromList(rawTarget.Properties, targetProps);

					ItemGroup targetItems = new ItemGroup(globalItems);

					ParsedPathList inputFiles = new ParsedPathList();
					string[] list = rawTarget.Inputs.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

					foreach (var rawInputFile in list)
					{
						ParsedPath pathSpec = null; 
						string s = targetProps.ReplaceVariables(rawInputFile);

						try
						{
							pathSpec = new ParsedPath(s, PathType.File).MakeFullPath();
						}
						catch (Exception e)
						{
							throw new ContentFileException("Bad path '{0}'".CultureFormat(s), e);
						}

						if (pathSpec.HasWildcards)
						{
							if (!Directory.Exists(pathSpec.VolumeAndDirectory))
							{
								throw new ContentFileException("Directory '{0}' does not exist".CultureFormat(pathSpec.VolumeAndDirectory));
							}

							IList<ParsedPath> files = DirectoryUtility.GetFiles(pathSpec, SearchScope.DirectoryOnly);

							if (files.Count == 0)
							{
								throw new ContentFileException("Wildcard input refers to no files after expansion");
							}

							inputFiles = new ParsedPathList(inputFiles.Concat(files));
						}
						else
						{
							if (!File.Exists(pathSpec))
							{
								throw new ContentFileException("Input file '{0}' does not exist".CultureFormat(pathSpec));
							}

							inputFiles.Add(pathSpec);
						}
					}

					ParsedPathList outputFiles = new ParsedPathList();

					list = rawTarget.Outputs.Split(';');

					foreach (var rawOutputFile in list)
					{
						string s = targetProps.ReplaceVariables(rawOutputFile);

						try 
						{
							ParsedPath outputFile = new ParsedPath(s, PathType.File).MakeFullPath();

							outputFiles.Add(outputFile);
						}
						catch (Exception e)
						{
							throw new ContentFileException("Bad path '{0}'".CultureFormat(s), e);
						}
					}

					targetItems.Set("TargetInputs", inputFiles);
					targetItems.Set("TargetOutputs", outputFiles);

					bool needsRebuild = IsCompileRequired(inputFiles, outputFiles);

					if (!needsRebuild)
						continue;

					buildTargets.Add(new BuildTarget(rawTarget.LineNumber, targetProps, targetItems));
				}
				catch (Exception e)
				{
					throw new ContentFileException(this.ContentPath, rawTarget.LineNumber, "Error preparing targets", e);
				}
			}

			return buildTargets;
		}

		private DateTime FindNewestAssemblyWriteTime(List<CompilerClass> compilerClasses)
		{
			DateTime newestAssemblyWriteTime = DateTime.MinValue;

			foreach (var compilerClass in compilerClasses)
			{
				DateTime dateTime = File.GetLastWriteTime(compilerClass.Assembly.Location);

				if (dateTime > newestAssemblyWriteTime)
					newestAssemblyWriteTime = dateTime;
			}

			return newestAssemblyWriteTime;
		}

		private bool IsCompileRequired(IList<ParsedPath> inputFiles, IList<ParsedPath> outputFiles)
		{
			if (Rebuild)
				return true;

			DateTime newestInputFile = ContentPathWriteTime > NewestAssemblyWriteTime ? 
				ContentPathWriteTime : NewestAssemblyWriteTime;

			foreach (var inputFile in inputFiles)
			{
				DateTime lastWriteTime = File.GetLastWriteTime(inputFile);

				if (lastWriteTime > newestInputFile)
					newestInputFile = lastWriteTime;
			}

			DateTime oldestOutputFile = DateTime.MaxValue;

			foreach (var outputFile in outputFiles)
			{
				DateTime lastWriteTime = File.GetLastWriteTime(outputFile);

				if (lastWriteTime < oldestOutputFile)
					oldestOutputFile = lastWriteTime;
			}

			return newestInputFile > oldestOutputFile;
		}

		private List<CompilerClass> LoadCompilerClasses(ItemGroup itemGroup, PropertyGroup propGroup)
		{
			List<CompilerClass> compilerClasses = new List<CompilerClass>();
			IList<ParsedPath> paths = itemGroup.GetRequiredValue("CompilerAssembly");

			foreach (var path in paths)
			{
				Assembly assembly = null;

				try
				{
					// We use Assembly.Load so that the test assembly and subsequently loaded
					// assemblies end up in the correct load context.  If the assembly cannot be
					// found it will raise a AssemblyResolve event where we will search for the 
					// assembly.
					assembly = Assembly.LoadFrom(path);
				}
				catch (Exception e)
				{
					throw new ApplicationException(String.Format("Unable to load content compiler assembly file '{0}'. {1}", path, e.ToString()), e);
				}

				Type[] types;

				// We won't get dependency errors until we actually try to reflect on all the types in the assembly
				try
				{
					types = assembly.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					string message = String.Format("Unable to reflect on assembly '{0}'", path);

					// There is one entry in the exceptions array for each null in the types array,
					// and they correspond positionally.
					foreach (Exception ex in e.LoaderExceptions)
						message += Environment.NewLine + "   " + ex.Message;

					// Not being able to reflect on classes in the test assembly is a critical error
					throw new ApplicationException(message, e);
				}

				// Go through all the types in the test assembly and find all the 
				// compiler classes, those that inherit from IContentCompiler.
				foreach (var type in types)
				{
					Type interfaceType = type.GetInterface(typeof(IContentCompiler).ToString());

					if (interfaceType != null)
					{
						CompilerClass compilerClass = new CompilerClass(assembly, type);

						compilerClasses.Add(compilerClass);
					}
				}
			}

			return compilerClasses;
		}

        #endregion

        #region IProcessCommandLine Members

		public void ProcessCommandLine(string[] args)
		{
			this.runningFromCommandLine = true;

#if MACOS
			Parser.CommandName = "mono BuildContent.exe";
#endif
			Parser.ParseAndSetTarget(args, this);
		}

        #endregion
	}
}
