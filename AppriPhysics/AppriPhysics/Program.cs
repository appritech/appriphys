using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace AppriPhysics
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // Keep the console window open in debug mode.

            UglyTest();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static GraphSolver gs;

        public static void UglyTest()
        {
            gs = new GraphSolver();

            Tank t1 = new Tank("T1", 1000.0, 500.0, new string[] { "V0" });
            gs.addComponent(t1);
            FlowLine v0 = new FlowLine("V0", "C1");
            gs.addComponent(v0);
            Junction c1 = new Junction("C1", new string[] { "V1", "V2" }, new string[] { "V0" });
            gs.addComponent(c1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            FlowLine v2 = new FlowLine("V2", "P2");
            gs.addComponent(v2);
            Pump p1 = new Pump("P1", 100.0, 3.2, "V3");
            gs.addComponent(p1);
            Pump p2 = new Pump("P2", 100.0, 3.2, "V4");
            gs.addComponent(p2);
            FlowLine v3 = new FlowLine("V3", "C2");
            gs.addComponent(v3);
            FlowLine v4 = new FlowLine("V4", "C2");
            gs.addComponent(v4);
            Junction c2 = new Junction("C2", new string[] { "V5" }, new string[] { "V3", "V4" });
            gs.addComponent(c2);
            FlowLine v5 = new FlowLine("V5", "T2");
            gs.addComponent(v5);
            Tank t2 = new Tank("T2", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);

            //v1.setFlowAllowedPercent(0.5);
            //v4.setFlowAllowedPercent(0.2);
            //t1.setCurrentVolume(0.0);
            v5.setMaxFlow(100.0);
            v0.setMaxFlow(85.0);
            //v5.setFlowAllowedPercent(0.5);

            gs.connectComponents();
            gs.solveMimic();
            gs.printSolution();
        }
    }
}
