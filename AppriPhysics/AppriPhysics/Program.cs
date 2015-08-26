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
        private static Dictionary<FluidType, double> plainWater = new Dictionary<FluidType, double>();
        private static Dictionary<FluidType, double> seaWater = new Dictionary<FluidType, double>();

        public static void UglyTest()
        {
            gs = new GraphSolver();
            plainWater.Add(FluidType.WATER, 1.0);
            seaWater.Add(FluidType.SEA_WATER, 1.0);


            //Tanks t1 and t2 are the base sources, and go to v1 and v2 directly (return comes back through v11 and v12)
            gs.addComponent(new Tank("T1", 1000.0, plainWater, 500.0, new string[] { "V1" }, false));
            gs.addComponent(new Tank("T2", 1000.0, plainWater, 500.0, new string[] { "V2" }, false));
            gs.addComponent(new FlowLine("V1", "C1"));         //v1 and v2 both go into S1
            gs.addComponent(new FlowLine("V2", "C1"));

            //Valves v1 and v2 combine into c1, and then split immediatelu to s1
            gs.addComponent(new Junction("C1", new string[] { "S1" }, new string[] { "V1", "V2" }, "", new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Splitter s1 splits the flow into v3 and v4, so that they can got to p1 and p2, which go further on to v5 and v6
            gs.addComponent(new Junction("S1", new String[] { "V3", "V4" }, new string[] { "C1" }, "", new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));
            gs.addComponent(new FlowLine("V3", "P1"));
            gs.addComponent(new FlowLine("V4", "P2"));
            gs.addComponent(new FlowDriver("P1", 100, 3.2, "V5"));
            gs.addComponent(new FlowDriver("P2", 100, 3.2, "V6"));
            gs.addComponent(new FlowLine("V5", "C2"));
            gs.addComponent(new FlowLine("V6", "C2"));

            //Valves v5 and v6 then get combined into c2, which immediately splits out with s2
            gs.addComponent(new Junction("C2", new string[] { "S2" }, new string[] { "V5", "V6" }));

            //Valves v7, v8, and v9 all go in between s2 and c3
            gs.addComponent(new Junction("S2", new String[] { "V7", "V8", "V9" }, new string[] { "C2" }, "C3", new double[] { .1, .4, .5 }, new double[] { .1, .6, 1.0 }));
            gs.addComponent(new FlowLine("V7", "C3"));
            gs.addComponent(new FlowLine("V8", "C3"));
            gs.addComponent(new FlowLine("V9", "C3"));
            gs.addComponent(new Junction("C3", new string[] { "V10" }, new string[] { "V7", "V8", "V9" }));

            //Valve v10 goes between c3 and s3
            gs.addComponent(new FlowLine("V10", "S3"));
            gs.addComponent(new Junction("S3", new String[] { "V11", "V12" }, new string[] { "V10" }, "", new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Valves v11 and v12 go from s3 back to the tanks t1 and t2
            gs.addComponent(new FlowLine("V11", "T1"));
            gs.addComponent(new FlowLine("V12", "T2"));


            //v2.setFlowAllowedPercent(0.25);
            //v4.setFlowAllowedPercent(0.2);
            //t1.setCurrentVolume(0.0);
            //v5.setMaxFlow(100.0);
            //v0.setMaxFlow(85.0);
            //v5.setFlowAllowedPercent(0.5);



            gs.connectComponents();
            gs.solveMimic();
            gs.printSolution();
        }
    }
}
