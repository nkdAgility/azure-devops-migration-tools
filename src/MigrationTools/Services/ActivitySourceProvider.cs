using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Instrumentation.Process;
using OpenTelemetry;

namespace MigrationTools.Services
{
    public class ActivitySourceProvider
    {
        private static readonly ActivitySource _activitySource;
        public static readonly string ActivitySourceName = "MigrationTools";
        public static string OpenTelemetryConnectionString = "InstrumentationKey=823d0de3-69c9-42ee-b902-de7675f681bc;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=4dd8f684-2f91-48ac-974f-dc898b686786";


        public static bool IsActivitySourceEnabled { get; private set; }

        static ActivitySourceProvider()
        {
            IsActivitySourceEnabled = true;
            _activitySource = new ActivitySource(ActivitySourceName);

            //ActivitySource.AddActivityListener(new ActivityListener()
            //{
            //    ShouldListenTo = _ => true,
            //    Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            //    ActivityStarted = activity => Console.WriteLine($"{activity.ParentId}:{activity.Id} - Start"),
            //    ActivityStopped = activity => Console.WriteLine($"{activity.ParentId}:{activity.Id} - Stop")
            //});
        }

        public static ActivitySource GetActivitySource()
        {
            return _activitySource;
        }

        public static void DisableActivitySource()
        {
            IsActivitySourceEnabled = false;
        }

        public static void EnableActivitySource()
        {
            IsActivitySourceEnabled = true;
        }

        internal class ActivityFilteringProcessor : BaseProcessor<Activity>
        {
            public override void OnStart(Activity activity)
            {
                activity.IsAllDataRequested = IsActivitySourceEnabled;
            }
        }
    }

 

    public static class ActivitySourceProviderExtensions
    {
        public static IHostBuilder UseOpenTelemitery(this IHostBuilder builder, string versionString)
        {
            builder.ConfigureServices((context, services) =>
            {

                services.AddOptions();

                // Configure OpenTelemetry
                Assembly entryAssembly = Assembly.GetEntryAssembly();
                string entryAssemblyName = entryAssembly?.GetName().Name;
                services.AddOpenTelemetry()
                    .WithTracing(builder =>
                    {
                        builder
                            .SetSampler(new AlwaysOnSampler())
                            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(entryAssemblyName, serviceVersion: versionString))
                            .AddSource(ActivitySourceProvider.ActivitySourceName) // Register your custom ActivitySource
                            .AddConsoleExporter() // Export traces to console
                            .AddProcessor(new ActivitySourceProvider.ActivityFilteringProcessor())
                            .SetErrorStatusOnException()
                            .AddAzureMonitorTraceExporter(options =>
                            {
                                options.ConnectionString = ActivitySourceProvider.OpenTelemetryConnectionString;

                            });
                    })
                    .WithMetrics(builder =>
                    {
                        builder
                             .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(entryAssemblyName, serviceVersion: versionString))
                             .AddRuntimeInstrumentation()
                             .AddProcessInstrumentation()
                             //.AddConsoleExporter() // Export metrics to console
                             .AddAzureMonitorMetricExporter(options =>
                             {
                                 options.ConnectionString = ActivitySourceProvider.OpenTelemetryConnectionString;
                             });
                    });

                //services.AddLogging(loggingBuilder =>
                //{
                //    loggingBuilder.AddOpenTelemetry(options =>
                //    {
                //        options.AddConsoleExporter();
                //        options.AddAzureMonitorLogExporter(config =>
                //        {
                //            config.ConnectionString = ActivitySourceProvider.OpenTelemetryConnectionString;
                //        });
                //    });
                //});
                services.AddSingleton(sp => ActivitySourceProvider.GetActivitySource());
            });
            return builder;
        }
    }
}
