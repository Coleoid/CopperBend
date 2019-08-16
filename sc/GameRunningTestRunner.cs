using NUnit.Engine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CopperBend.Application
{
    internal class GameRunningTestRunner : ITestEventListener
    {
        public List<string> EventReports { get; set; } = new List<string>();

        public void RunTests()
        {
            ITestEngine engine = TestEngineActivator.CreateInstance();
            TestPackage package = new TestPackage(@"sc.Tests.dll");
            ITestRunner runner = engine.GetRunner(package);
            XmlNode result = runner.Run(this, TestFilter.Empty);
            WriteResult(result);
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
    }
}
