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

            Tank t1 = new Tank("T1", 1000.0, 500.0, new string[] { "C1" });
            gs.addComponent(t1);
            Junction c1 = new Junction("C1", new string[] { "V1", "V2" }, new string[] { "T1" });
            gs.addComponent(c1);
            FlowLine v1 = new FlowLine("V1", "S1");
            gs.addComponent(v1);
            FlowLine v2 = new FlowLine("V2", "S1");
            gs.addComponent(v2);
            Junction s1 = new Junction("S1", new string[] { "P1" }, new string[] { "V1", "V2" }, new double[] { 0.5, 0.5 }, new double[] { 0.6, 1.0 });
            gs.addComponent(s1);
            Pump p1 = new Pump("P1", 300.0, 3.2, "S2");
            gs.addComponent(p1);
            Junction s2 = new Junction("S2", new string[] { "V3", "V4" }, new string[] { "P1" }, new double[] { 0.5, 0.5 }, new double[] { 0.6, 1.0 });
            gs.addComponent(s2);
            FlowLine v3 = new FlowLine("V3", "C2");
            gs.addComponent(v3);
            FlowLine v4 = new FlowLine("V4", "C2");
            gs.addComponent(v4);
            Junction c2 = new Junction("C2", new string[] { "T2" }, new string[] { "V3", "V4" });
            gs.addComponent(c2);
            Tank t2 = new Tank("T2", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);

            //v1.setFlowAllowedPercent(0.5);
            //v4.setFlowAllowedPercent(0.2);
            t1.setCurrentVolume(0.0);
            //v3.setMaxFlow(140.0);

            gs.connectComponents();
            gs.solveMimic();
            gs.printSolution();
        }
    }
}
