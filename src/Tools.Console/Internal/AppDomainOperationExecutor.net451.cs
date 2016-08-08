﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if NET451
using System;
using System.Collections;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Design;

namespace Microsoft.EntityFrameworkCore.Tools.Internal
{
    public class AppDomainOperationExecutor : OperationExecutorBase
    {
        private readonly object _executor;
        private readonly AppDomain _domain;
        private bool _disposed;

        public AppDomainOperationExecutor(
            [NotNull] OperationExecutorSetup setupInfo,
            [CanBeNull] string configFile)
            : base(setupInfo)
        {
            var info = new AppDomainSetup
            {
                ApplicationBase = setupInfo.ApplicationBasePath,
                ConfigurationFile = configFile
            };

            Reporter.Verbose("Using app base path " + setupInfo.ApplicationBasePath);

            _domain = AppDomain.CreateDomain("EntityFrameworkCore.DesignDomain", null, info);

            if (!string.IsNullOrEmpty(setupInfo.DataDirectory))
            {
                _domain.SetData("DataDirectory", setupInfo.DataDirectory);
            }

            var logHandler = new OperationLogHandler(
                Reporter.Error,
                Reporter.Warning,
                Reporter.Output,
                Reporter.Verbose,
                Reporter.Verbose);

            _executor = _domain.CreateInstanceAndUnwrap(DesignAssemblyName,
                ExecutorTypeName,
                false,
                BindingFlags.Default,
                null,
                new object[]
                {
                    logHandler,
                    new Hashtable
                    {
                        { "targetName", setupInfo.AssemblyName },
                        { "startupTargetName", setupInfo.StartupAssemblyName },
                        { "projectDir", setupInfo.ProjectDir },
                        { "contentRootPath", setupInfo.ContentRootPath },
                        { "rootNamespace", setupInfo.RootNamespace },
                        { "environment", setupInfo.EnvironmentName }
                    }
                },
                null, null);
        }

        protected override object CreateResultHandler()
            => new OperationResultHandler();

        protected override void Execute(string operationName, object resultHandler, IDictionary arguments)
        {
            _domain.CreateInstance(
                DesignAssemblyName,
                ExecutorTypeName + "+" + operationName,
                false,
                BindingFlags.Default,
                null,
                new[] { _executor, resultHandler, arguments },
                null,
                null);
        }

        public override void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                AppDomain.Unload(_domain);
            }
        }
    }
}
#endif