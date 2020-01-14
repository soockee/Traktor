using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Traktor;
using OpenTracing;
namespace Traktor_Test
{
    [TestClass]
    public class ReporterTests
    {
        [TestMethod]
        public void Report_WithValidSpan()
        {
            string expectedOperationName = "Operationname-4Head";
            string address = "localhost";
            int agentport = 13337;
            int reporterport = 13338;
            // Setup Recieving Endpoint before Tracer.Reporter Connects to this Client and before Span Finish() is Called
            Task agentTast = Task.Run(() => {
                
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpClient agent = new UdpClient(agentport);
                Byte[] recieveBytes = agent.Receive(ref RemoteIpEndPoint);
                string spanContent = Encoding.ASCII.GetString(recieveBytes);
                // Uses the IPEndPoint object to determine which of these two hosts responded.
                // Output can be used to comprehend the communication
                Console.WriteLine("This is the message you received " +
                                             spanContent.ToString());
                Console.WriteLine("This message was sent from " +
                                            RemoteIpEndPoint.Address.ToString() +
                                            " on their port number " +
                                            RemoteIpEndPoint.Port.ToString());
                Assert.AreEqual(expectedOperationName, spanContent.ToString().Split(";")[0]);
                Assert.IsNotNull(DateTime.Parse(spanContent.ToString().Split(";")[5]));
            });
            Tracer tracer = new Tracer();
            //Cautions: Test run parallel ->  Portusage needs to vary between tests
            tracer.Configure(address, agentport, reporterport);
            IScope scope = tracer.BuildSpan(expectedOperationName).StartActive();
            ISpan span = scope.Span;
            span.Finish();
            agentTast.Wait();
            Assert.AreNotEqual(DateTime.MinValue, DateTime.Parse(span.ToString().Split(";")[5]));
        }  
    }
}
