using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using lpubsppop01.REPLReaderWriter;

namespace lpubsppop01.REPLReaderWriterTests
{
    [TestClass]
    public class REPLManagerTests
    {
        [TestMethod]
        public void TestWithPython()
        {
            int processID = REPLManager.Start(@"C:\tools\Anaconda3\python.exe", "Python");
            Assert.IsTrue(processID > 0);
            REPLManager.SetTimeout(processID, 1000);
            REPLManager.WriteLine(processID, "1 + 1");
            string outputText = REPLManager.ReadLine(processID);
            Assert.AreEqual("2", outputText);
            REPLManager.Stop(processID);
        }
    }
}
