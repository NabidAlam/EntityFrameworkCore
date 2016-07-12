﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.EntityFrameworkCore.Tools.Internal;

// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class Program
    {
        public static int Main(string[] args)
        {
            HandleDebugSwitch(ref args);

            try
            {
                var options = CommandLineOptions.Parse(args);
                if (options == null)
                {
                    Reporter.Output("Specify --help for a list of available options and commands.");
                    return 1;
                }

                Reporter.IsVerbose = options.Verbose;

                if (options.IsHelp)
                {
                    return 2;
                }

                if (options.Command == null)
                {
                    Reporter.Error("Error in parsing command line arguments");
                    return 1;
                }

                using (var executor = GetExecutor(options))
                {
                    options.Command.Run(executor);
                }

                return 0;
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                if (!(ex is OperationErrorException) && !(ex is CommandParsingException))
                {
                    Reporter.Error(ex.ToString());
                }

                Reporter.Error(ex.Message);
                return 1;
            }
        }

        private static OperationExecutorBase GetExecutor(CommandLineOptions options)
        {
            return new AssemblyLoadContextOperationExecutor(options.TargetProject, 
                options.StartupProject,
                options.Configuration,
                options.BuildBasePath,
                options.BuildOutputPath, 
                options.Framework, 
                options.EnvironmentName, options.NoBuild);
        }


        [Conditional("DEBUG")]
        private static void HandleDebugSwitch(ref string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "--debug")
                {
                    args = args.Take(i).Concat(args.Skip(i + 1)).ToArray();
                    Console.WriteLine("Waiting for debugger to attach. Press ENTER to continue");
                    Console.WriteLine($"Process ID: {Process.GetCurrentProcess().Id}");
                    Console.ReadLine();
                }
            }
        }

        private static readonly Assembly ThisAssembly
            = typeof(CommandLineOptions).GetTypeInfo().Assembly;

        public static string GetVersion()
            => ThisAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
               ?? ThisAssembly.GetName().Version.ToString();
    }
}
