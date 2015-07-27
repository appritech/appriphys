using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace UnitTests
{
    [TestClass]
    public class DualPumpTests
    {
        private GraphSolver gs;

        [TestInitialize()]
        public void InitializeGraph()
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

            gs.connectComponents();
        }

        [TestMethod]
        public void D_T1_Empty()
        {
            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void D_V5_85MaxFlow_V0_100MaxFlow()
        {
            FlowLine v0 = (FlowLine)gs.getComponent("V0");
            v0.setMaxFlow(85.0);
            FlowLine v5 = (FlowLine)gs.getComponent("V5");
            v5.setMaxFlow(100.0);
            gs.solveMimic();
            double solutionFlow = 85.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void D_V5_50MaxFlow_V0_25MaxFlow()
        {
            FlowLine v0 = (FlowLine)gs.getComponent("V0");
            v0.setMaxFlow(25.0);
            FlowLine v5 = (FlowLine)gs.getComponent("V5");
            v5.setMaxFlow(50.0);
            gs.solveMimic();
            double solutionFlow = 25.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void D_V5_50MaxFlow()
        {
            FlowLine v5 = (FlowLine)gs.getComponent("V5");
            v5.setMaxFlow(50.0);
            gs.solveMimic();
            double solutionFlow = 50.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void D_V5_50Percent()
        {
            FlowLine v5 = (FlowLine)gs.getComponent("V5");
            v5.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 100.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void D_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V0", solutionFlow);
            TestingTools.verifyFlow(gs, "C1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P1", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "P2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "C2", solutionFlow);
            TestingTools.verifyFlow(gs, "V5", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }
    }
}
