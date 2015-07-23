using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace UnitTests
{
    [TestClass]
    public class CombineBeforeAndAfterPumpTests
    {
        private GraphSolver gs;

        [TestInitialize()]
        public void InitializeGraph()
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

            gs.connectComponents();
        }

        [TestMethod]
        public void C_BandAPump_V1_25Percent_V2_25Percent()
        {
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.25);
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.25);
            gs.solveMimic();
            double solutionFlow = 150;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void C_BandAPump_V1_50Percent_V2_50Percent()
        {
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.5);
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 300;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void C_BandAPump_V4_20Percent()
        {
            FlowLine v4 = (FlowLine)gs.getComponent("V4");
            v4.setFlowAllowedPercent(0.2);
            gs.solveMimic();
            double solutionFlow = 240;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow * 0.75);
            TestingTools.verifyFlow(gs, "V4", solutionFlow * 0.25);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void C_BandAPump_T1_Empty()
        {
            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "V4", solutionFlow);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void C_BandAPump_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }


    }
}
