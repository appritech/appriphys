using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace UnitTests
{
    [TestClass]
    public class WholeModelTest
    {
        private GraphSolver gs;

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();

            //Tanks t1 and t2 are the base sources, and go to v1 and v2 directly (return comes back through v11 and v12)
            gs.addComponent(new Tank("T1", 1000.0, 500.0, new string[] { "V1" }));
            gs.addComponent(new Tank("T2", 1000.0, 500.0, new string[] { "V2" }));
            gs.addComponent(new FlowLine("V1", "C1"));         //v1 and v2 both go into S1
            gs.addComponent(new FlowLine("V2", "C1"));

            //Valves v1 and v2 combine into c1, and then split immediatelu to s1
            gs.addComponent(new Junction("C1", new string[] { "S1" }, new string[] { "V1", "V2" }, new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Splitter s1 splits the flow into v3 and v4, so that they can got to p1 and p2, which go further on to v5 and v6
            gs.addComponent(new Junction("S1", new String[] { "V3", "V4" }, new string[] { "C1" }));
            gs.addComponent(new FlowLine("V3", "P1"));
            gs.addComponent(new FlowLine("V4", "P2"));
            gs.addComponent(new Pump("P1", 100, 100, "V5"));
            gs.addComponent(new Pump("P2", 100, 100, "V6"));
            gs.addComponent(new FlowLine("V5", "C2"));
            gs.addComponent(new FlowLine("V6", "C2"));

            //Valves v5 and v6 then get combined into c2, which immediately splits out with s2
            gs.addComponent(new Junction("C2", new string[] { "S2" }, new string[] { "V5", "V6" }));

            //Valves v7, v8, and v9 all go in between s2 and c3
            gs.addComponent(new Junction("S2", new String[] { "V7", "V8", "V9" }, new string[] { "C2" }, new double[] { .1, .4, .5 }, new double[] { .1, .6, 1.0 }));
            gs.addComponent(new FlowLine("V7", "C3"));
            gs.addComponent(new FlowLine("V8", "C3"));
            gs.addComponent(new FlowLine("V9", "C3"));
            gs.addComponent(new Junction("C3", new string[] { "V10" }, new string[] { "V7", "V8", "V9" }));

            //Valve v10 goes between c3 and s3
            gs.addComponent(new FlowLine("V10", "S3"));
            gs.addComponent(new Junction("S3", new String[] { "V11", "V12" }, new string[] { "V10" }, new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 }));

            //Valves v11 and v12 go from s3 back to the tanks t1 and t2
            gs.addComponent(new FlowLine("V11", "T1"));
            gs.addComponent(new FlowLine("V12", "T2"));

            gs.connectComponents();
        }

        [TestMethod]
        public void WM_V7_Closed_V11_V12_Limited()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);
            
            ((FlowLine)gs.getComponent("V11")).setFlowAllowedPercent(0.25);
            ((FlowLine)gs.getComponent("V12")).setFlowAllowedPercent(0.25);

            ((FlowLine)gs.getComponent("V7")).setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 100.0;          //Basic flow through system, but the branches should share half
            //TestingTools.verifyFlow(gs, "T1", solutionFlow * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", 0.0);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 40.0 / 90.0);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 50.0 / 90.0);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_V2_V11_Closed()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            //Valve v11 = (Valve) mc.getComponent("V11");
            //v11.setOpenPercentage(0);
            ((FlowLine)gs.getComponent("V11")).setFlowAllowedPercent(0.0);
            ((FlowLine)gs.getComponent("V2")).setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 150.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);   //Transferring from t1 to t2.
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", 15.0);
            TestingTools.verifyFlow(gs, "V8", 60.0);
            TestingTools.verifyFlow(gs, "V9", 75.0);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow);
        }

        [TestMethod]
        public void WM_T1T2_Both_Empty()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            ((FlowLine)gs.getComponent("V2")).setMaxFlow(150);

            ((Tank)gs.getComponent("T1")).setCurrentVolume(0.0);
            ((Tank)gs.getComponent("T2")).setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_T1_Empty_V2_Limited_WithAnger()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            ((FlowLine)gs.getComponent("V2")).setMaxFlow(150);

            ((Tank)gs.getComponent("T1")).setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 150.0;          //Basic flow through system, but the branches should share half
            //TestingTools.verifyFlow(gs, "T1", solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0 * 3.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_T1_Empty()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);
            
            ((Tank)gs.getComponent("T1")).setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            //TestingTools.verifyFlow(gs, "T1", solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0 * 3.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_V11_V12_Limited()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            //Valve v11 = (Valve) mc.getComponent("V11");
            //v11.setOpenPercentage(0);
            ((FlowLine)gs.getComponent("V11")).setFlowAllowedPercent(0.25);
            ((FlowLine)gs.getComponent("V12")).setFlowAllowedPercent(0.25);
            gs.solveMimic();
            double solutionFlow = 100.0;          //Basic flow through system, but the branches should share half
            //TestingTools.verifyFlow(gs, "T1", solutionFlow * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", 10.0);
            TestingTools.verifyFlow(gs, "V8", 40.0);
            TestingTools.verifyFlow(gs, "V9", 50.0);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_V5_50Percent()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            ((FlowLine)gs.getComponent("V5")).setFlowAllowedPercent(0.5);

            gs.solveMimic();
            double solutionFlow = 150.0;          //Basic flow through system, but the branches should share half
            double flow1 = 50.0;
            double flow2 = 100.0;
            //TestingTools.verifyFlow(gs, "T1", solutionFlow * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", flow1);
            TestingTools.verifyFlow(gs, "V4", flow2);
            TestingTools.verifyFlow(gs, "P1", flow1);
            TestingTools.verifyFlow(gs, "P2", flow2);
            TestingTools.verifyFlow(gs, "V5", flow1);
            TestingTools.verifyFlow(gs, "V6", flow2);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_V3_75Percent()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            ((FlowLine)gs.getComponent("V3")).setFlowAllowedPercent(0.75);

            gs.solveMimic();
            double solutionFlow = 175.0;          //Basic flow through system, but the branches should share half
            double flow1 = 75.0;
            double flow2 = 100.0;
            //TestingTools.verifyFlow(gs, "T1", solutionFlow * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", flow1);
            TestingTools.verifyFlow(gs, "V4", flow2);
            TestingTools.verifyFlow(gs, "P1", flow1);
            TestingTools.verifyFlow(gs, "P2", flow2);
            TestingTools.verifyFlow(gs, "V5", flow1);
            TestingTools.verifyFlow(gs, "V6", flow2);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }

        [TestMethod]
        public void WM_AllOpen()
        {
            ((FlowLine)gs.getComponent("V11")).setMaxFlow(150);
            ((FlowLine)gs.getComponent("V12")).setMaxFlow(150);

            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            //TestingTools.verifyFlow(gs, "T1", solutionFlow * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            //TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V5", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V6", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V7", solutionFlow * 0.1);
            TestingTools.verifyFlow(gs, "V8", solutionFlow * 0.4);
            TestingTools.verifyFlow(gs, "V9", solutionFlow * 0.5);
            TestingTools.verifyFlow(gs, "C3", solutionFlow);
            TestingTools.verifyFlow(gs, "V10", solutionFlow);
            TestingTools.verifyFlow(gs, "S3", solutionFlow);
            TestingTools.verifyFlow(gs, "V11", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V12", solutionFlow / 2.0);
        }
    }
}
