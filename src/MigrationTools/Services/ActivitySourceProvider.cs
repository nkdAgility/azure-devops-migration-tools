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
using Serilog;
using MigrationTools.Options;

namespace MigrationTools.Services
{
    public class ActivitySourceProvider
    {
        public static readonly string ActivitySourceName = "MigrationTools";
        private static string OpenTelemetryConnectionString = "InstrumentationKey=823d0de3-69c9-42ee-b902-de7675f681bc;IngestionEndpoint=https://westeurope-5.in.applicationinsights.azure.com/;LiveEndpoint=https://westeurope.livediagnostics.monitor.azure.com/;ApplicationId=4dd8f684-2f91-48ac-974f-dc898b686786";
        private static string MigrationToolAIConnectionString = "InstrumentationKey=2d666f84-b3fb-4dcf-9aad-65de038d2772;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=9146fe72-5c18-48d7-a0f2-8fb891ef1277";


            public static ActivitySource ActivitySource { get; private set; }
        public static bool IsActivitySourceEnabled { get; private set; }

        public static bool TelemeteryDebug { get; private set; }

        public static string GetConnectionString()
        {
            return OpenTelemetryConnectionString;
        }


        static ActivitySourceProvider()
        {
            IsActivitySourceEnabled = true;
            ActivitySource = new ActivitySource(ActivitySourceName);

            // Enable debug listener
            ActivitySource.AddActivityListener(new ActivityListener
            {
                ShouldListenTo = _ => true,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStarted = activity =>
                {
                    if (TelemeteryDebug)
                    {
                        Log.Debug($"Telemetry:{GetActivityPath(activity)} - Start");
                    }
                },
                ActivityStopped = activity =>
                {
                    if (TelemeteryDebug)
                    {
                        Log.Debug($"Telemetry:{GetActivityPath(activity)} - Stop");
                    }
                }
            });
        }

        public static string GetActivityPath(Activity activity)
        {
            if (activity == null)
            {
                return string.Empty;
            }

            // Recursively get the parent's path
            string parentPath = GetActivityPath(activity.Parent);

            // If the parent path is not empty, append the current activity's name with a colon separator
            if (!string.IsNullOrEmpty(parentPath))
            {
                return $"{parentPath}:{activity.DisplayName}";
            }

            // If there is no parent, just return the current activity's name
            return activity.DisplayName;
        }



        public static ActivitySource GetActivitySource()
        {
            return ActivitySource;
        }

        public static void DisableActivitySource()
        {
            IsActivitySourceEnabled = false;
        }

        public static void EnableActivitySource()
        {
            IsActivitySourceEnabled = true;
        }

        public static void DisableTelemeteryDebug()
        {
            TelemeteryDebug = false;
        }

        public static void EnableTelemeteryDebug()
        {
            TelemeteryDebug = true;
        }

        public static void FlushTelemetery()
        {
            throw new NotImplementedException();
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
                            //.AddConsoleExporter() // Export traces to console
                            .AddProcessor(new ActivitySourceProvider.ActivityFilteringProcessor())
                            .AddHttpClientInstrumentation()
                            .SetErrorStatusOnException()
                            .AddAzureMonitorTraceExporter(options =>
                            {
                                options.ConnectionString = ActivitySourceProvider.GetConnectionString();

                            });
                    })
                    .WithMetrics(builder =>
                    {
                        builder
                             .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(entryAssemblyName, serviceVersion: versionString))
                             .AddHttpClientInstrumentation()
                             .AddRuntimeInstrumentation()
                             .AddProcessInstrumentation()
                             //.AddConsoleExporter() // Export metrics to console
                             .AddAzureMonitorMetricExporter(options =>
                             {
                      
                                 options.ConnectionString = ActivitySourceProvider.GetConnectionString();
                             });
                    });
                services.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddOpenTelemetry(options =>
                    {
                        //options.AddConsoleExporter();
                        options.AddAzureMonitorLogExporter(config =>
                        {
                            config.ConnectionString = ActivitySourceProvider.GetConnectionString();
                        });
                    });
                });
                services.AddSingleton(sp => ActivitySourceProvider.GetActivitySource());
            });

    

            return builder;
        }

        public static void SetTagsFromOptions(this Activity activity, IOptions options)
        {
            if (options == null)
            {
                return;
            }
            var properties = options.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(options);

                if (value != null)
                {
                    activity.SetTag($"options.{property.Name}", value.ToString());
                    activity.SetTag($"{options.GetType().Name}.{property.Name}", value.ToString());
                }
            }
            return;
        }

public static void SetTagsFromObject(this Activity activity, object values)
        {
            if (values == null)
            {
                return;
            }
            var properties = values.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                var value = property.GetValue(values);

                if (value != null)
                {
                    activity.SetTag($"{values.GetType().Name}.{property.Name}", value.ToString());
                }
            }
            return;
        }
    }
}
