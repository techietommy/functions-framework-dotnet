﻿// Copyright 2020, Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Google.Cloud.Functions.Invoker
{
    /// <summary>
    /// The entry point for the invoker. This is used automatically by the entry point generated by MS Build
    /// targets within the Google.Cloud.Functions.Invoker NuGet package.
    /// </summary>
    public static class EntryPoint
    {
        /// <summary>
        /// The environment variable used to detect the function target name, when not otherwise provided.
        /// </summary>
        public const string FunctionTargetEnvironmentVariable = "FUNCTION_TARGET";

        /// <summary>
        /// The environment variable used to detect the port to listen on.
        /// </summary>
        public const string PortEnvironmentVariable = "PORT";

        /// <summary>
        /// Starts a web server to serve the function in the specified assembly. This method is called
        /// automatically be the generated entry point.
        /// </summary>
        /// <param name="functionAssembly">The assembly containing the function to execute.</param>
        /// <param name="args">Arguments to parse </param>
        /// <returns>A task representing the asynchronous operation.
        /// The result of the task is an exit code for the process, which is 0 for success or non-zero
        /// for any failures.
        /// </returns>
        public static async Task<int> StartAsync(Assembly functionAssembly, string[] args)
        {
            // Clear out the ASPNETCORE_URLS environment variable in order to avoid a warning when we start the server.
            // An alternative would be to *use* the environment variable, but as it's populated (with a non-ideal value) by
            // default, I suspect that would be tricky.
            Environment.SetEnvironmentVariable("ASPNETCORE_URLS", null);

            // TODO: Catch exceptions and return 1, or just let the exception propagate? It probably
            // doesn't matter much. Potentially catch exceptions during configuration, but let any
            // during web server execution propagate.
            var host = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webHostBuilder => webHostBuilder
                    .ConfigureAppConfiguration(builder => builder.AddFunctionsEnvironment().AddFunctionsCommandLine(args))
                    .ConfigureLogging((context, logging) => logging.ClearProviders().AddFunctionsConsoleLogging(context))
                    .ConfigureKestrelForFunctionsFramework()
                    .ConfigureServices((context, services) => services
                    .AddFunctionTarget(context, functionAssembly))
                    .UseFunctionsStartups(functionAssembly)
                    .Configure((context, app) => app.UseFunctionsFramework(context)))
                .Build();
            await host.RunAsync();
            return 0;
        }
    }
}
