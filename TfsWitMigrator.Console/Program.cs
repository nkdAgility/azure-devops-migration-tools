using CommandLine.Text;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSTS.DataBulkEditor.Engine;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.IO;
using VSTS.DataBulkEditor.Engine.Configuration;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.ConsoleApp
{
    class Program
    {
        [Verb("init", HelpText = "Creates initial config file")]
        class InitOptions
        {
            //normal options here
        }
        [Verb("execute", HelpText = "Record changes to the repository.")]
        class RunOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }
        }

        static int Main(string[] args)
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(string.Format(@"{0}-{1}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "MigrationRun"), "myListener"));
            //////////////////////////////////////////////////
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //////////////////////////////////////////////////
            int result = (int)Parser.Default.ParseArguments<InitOptions, RunOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (RunOptions opts) => RunExecuteAndReturnExitCode(opts),
                errs => 1);
            //////////////////////////////////////////////////
            Console.WriteLine();
            Console.WriteLine("Freedom");
            Console.ReadKey();
            return result;
        }

        private static object RunExecuteAndReturnExitCode(RunOptions opts)
        {
            EngineConfiguration ec;
            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "vstsbulkeditor.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                Trace.WriteLine("The config file does not exist, nor doe the default 'vstsbulkeditor.json'. Use 'init' to create a configuration file first");
                return 1;
            }
            else
            {
                StreamReader sr = new StreamReader(opts.ConfigFile);
                string vstsbulkeditorjson = sr.ReadToEnd();
                sr.Close();
                ec = JsonConvert.DeserializeObject<EngineConfiguration>(vstsbulkeditorjson, 
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
            }

            MigrationEngine me = new MigrationEngine(ec);
            me.Run();
            return 0;
        }

        private static object RunInitAndReturnExitCode(InitOptions opts)
        {
            if (!File.Exists("vstsbulkeditor.json"))
            {
                string json = JsonConvert.SerializeObject(EngineConfiguration.GetDefault(),
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
                StreamWriter sw = new StreamWriter("vstsbulkeditor.json");
                sw.WriteLine(json);
                sw.Close();
                Trace.WriteLine("New vstsbulkeditor.json file has been created");
            }
            return 0;
        }
    }
}