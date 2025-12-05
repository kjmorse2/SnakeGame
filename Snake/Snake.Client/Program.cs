// <copyright file="Program.cs" company="U of U CS3500">
// Copyright (c) U of U CS3500, Kenneth Morse, and Hunter Simmons. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace CS3500.Snake.Client;

/// <summary>
///     Entry point for the Snake client WebAssembly application.
/// </summary>
public class Program
{
    /// <summary>
    ///     The main entry point for the application. Configures logging and starts the WebAssembly host.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task Main(string[ ] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Logging.ClearProviders(); // Use default WebAssembly console logger
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
        builder.Logging.AddFilter("System", LogLevel.Warning);
        builder.Logging.AddFilter("CS3500", LogLevel.Information);

        await builder.Build().RunAsync();
    }
}
