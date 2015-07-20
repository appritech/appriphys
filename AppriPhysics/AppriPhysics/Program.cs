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

            
            //Tanks t1 and t2 are the base sources, and go to v1 and v2 directly (return comes back through v11 and v12)
            gs.addComponent(new Tank("t1", 1000.0, 500.0, new string[] { "v1" }));
            gs.addComponent(new Tank("t2", 1000.0, 500.0, new string[] { "v2" }));
            gs.addComponent(new FlowLine("v1", "c1"));         //v1 and v2 both go into S1
            gs.addComponent(new FlowLine("v2", "c1"));

            //Valves v1 and v2 combine into c1, and then split immediatelu to s1
            gs.addComponent(new Junction("c1", new string[] { "s1" }, new string[] { "v1", "v2" }, new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Splitter s1 splits the flow into v3 and v4, so that they can got to p1 and p2, which go further on to v5 and v6
            gs.addComponent(new Junction("s1", new String[] { "v3", "v4" }, new string[] { "c1" }));
            gs.addComponent(new FlowLine("v3", "p1"));
            gs.addComponent(new FlowLine("v4", "p2"));
            gs.addComponent(new Pump("p1", 100, 100, "v5"));
            gs.addComponent(new Pump("p2", 100, 100, "v6"));
            gs.addComponent(new FlowLine("v5", "c2"));
            gs.addComponent(new FlowLine("v6", "c2"));

            //Valves v5 and v6 then get combined into c2, which immediately splits out with s2
            gs.addComponent(new Junction("c2", new string[] { "s2" }, new string[] { "v5", "v6" }));

            //Valves v7, v8, and v9 all go in between s2 and c3
            gs.addComponent(new Junction("s2", new String[] { "v7", "v8", "v9" }, new string[] { "c2" }, new double[] { .1, .4, .5 }, new double[] { .1, .6, 1.0 }));
            gs.addComponent(new FlowLine("v7", "c3"));
            gs.addComponent(new FlowLine("v8", "c3"));
            gs.addComponent(new FlowLine("v9", "c3"));
            gs.addComponent(new Junction("c3", new string[] { "v10" }, new string[] { "v7", "v8", "v9" }));

            //Valve v10 goes between c3 and s3
            gs.addComponent(new FlowLine("v10", "s3"));
            gs.addComponent(new Junction("s3", new String[] { "v11", "v12" }, new string[] { "v10" }, new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Valves v11 and v12 go from s3 back to the tanks t1 and t2
            gs.addComponent(new FlowLine("v11", "t1"));
            gs.addComponent(new FlowLine("v12", "t2"));
            
            ((FlowLine)gs.getComponent("v11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("v12")).setMaxFlow(150);

            ((FlowLine)gs.getComponent("v3")).setFlowAllowedPercent(0.75);
            //((FlowLine)gs.getComponent("v5")).setFlowAllowedPercent(0.5);

            //((FlowLine)gs.getComponent("v11")).setFlowAllowedPercent(0.25);
            //((FlowLine)gs.getComponent("v12")).setFlowAllowedPercent(0.25);

            //((FlowLine)gs.getComponent("v7")).setFlowAllowedPercent(0.0);



            /*
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
            v4.setFlowAllowedPercent(0.2);
            //t1.setCurrentVolume(0.0);
            //v5.setMaxFlow(100.0);
            //v0.setMaxFlow(85.0);
            //v5.setFlowAllowedPercent(0.5);
            */


            gs.connectComponents();
            gs.solveMimic();
            gs.printSolution();
        }
    }
}
