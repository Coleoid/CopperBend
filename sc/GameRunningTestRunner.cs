using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using NUnit.Framework.Api;
using NUnit.Framework.Interfaces;
using System.Reflection;
using CopperBend.Contract;

namespace CopperBend.Application
{
    internal class GameRunningTestRunner : ITestEventListener, IScheduleAgent
    {
        public List<string> EventReports { get; set; } = new List<string>();

        public ScheduleEntry GetNextEntry(int offset)
        {
            return null;
            //return new ScheduleEntry
            //{
            //    Action = (cp) => {
            //        ControlPanel = cp; // ?
            //        this.RunTests();
            //    },
            //    Agent = this,
            //    Offset = offset
            //};
        }

        public void RunTests()
        {
            //Debugger.Launch();
            ITestEngine engine = TestEngineActivator.CreateInstance();
            var filterBuilder = engine.Services.GetService<ITestFilterService>().GetTestFilterBuilder();
            filterBuilder.SelectWhere("Context == 'ConsoleRunning'");
            var filter = filterBuilder.GetFilter();

            TestPackage package = new TestPackage(@"sc.Tests.dll");
            ITestRunner runner = engine.GetRunner(package);

            XmlNode result = null;
            result = runner.Run(this, filter);

            WriteResult(result);
            SadConsole.Game.Instance.Exit();
        }

        private static void WriteResult(XmlNode result)
        {
            var writer = new XmlTextWriter(Console.OpenStandardOutput(), Encoding.ASCII);
            result.WriteTo(writer);
            writer.Flush();
            writer.Close();
            Console.WriteLine();
        }

        public void OnTestEvent(string report)
        {
            EventReports.Add(report);
        }

        public ScheduleEntry GetNextEntry()
        {
            // later we want to fill this out
            throw new NotImplementedException();
        }
    }
}
