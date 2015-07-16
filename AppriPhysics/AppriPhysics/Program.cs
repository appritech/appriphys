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

            Tank t1 = new Tank("T1", 1000.0, 500.0, new string[] { "V1" });
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "S1");
            gs.addComponent(v1);
            Tank t2 = new Tank("T2", 1000.0, 500.0, new string[] { "V2" });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);
            FlowLine v2 = new FlowLine("V2", "S1");
            gs.addComponent(v2);
            Junction s1 = new Junction("S1", new string[] { "P1" }, new string[] { "V1", "V2" }, new double[] { 0.5, 0.5 }, new double[] { 0.6, 1.0 });
            gs.addComponent(s1);
            Pump p1 = new Pump("P1", 200.0, 3.2, "V3");
            gs.addComponent(p1);
            FlowLine v3 = new FlowLine("V3", "T3");
            gs.addComponent(v3);
            Tank t3 = new Tank("T3", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t3);

            //v1.setFlowAllowedPercent(0.5);
            //v3.setFlowAllowedPercent(0.5);
            v3.setMaxFlow(140.0);

            gs.connectComponents();
            gs.solveMimic();
            gs.printSolution();
        }
    }
}
