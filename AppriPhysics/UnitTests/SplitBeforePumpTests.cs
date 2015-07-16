using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace UnitTests
{
    [TestClass]
    public class SplitBeforePumpTests
    {
        private GraphSolver gs;

        [TestInitialize()]
        public void InitializeGraph()
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

            gs.connectComponents();
        }

        [TestMethod]
        public void S_BeforePump_V2Closed()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.6;          //Basic flow through system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", -solutionFlow);
            TestingTools.verifyFlow(gs, "T2", 0.0);
            TestingTools.verifyFlow(gs, "V2", 0.0);
            TestingTools.verifyFlow(gs, "S1", -solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

        [TestMethod]
        public void S_BeforePump_V1Closed()
        {
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", 0.0);
            TestingTools.verifyFlow(gs, "V1", 0.0);
            TestingTools.verifyFlow(gs, "T2", -solutionFlow);
            TestingTools.verifyFlow(gs, "V2", -solutionFlow);
            TestingTools.verifyFlow(gs, "S1", -solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }


        [TestMethod]
        public void S_BeforePump_V2_20_Percent_Open()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.2);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.8;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -200.0 * 0.6);
            TestingTools.verifyFlow(gs, "V1", -200.0 * 0.6);
            TestingTools.verifyFlow(gs, "T2", -200.0 * 0.2);
            TestingTools.verifyFlow(gs, "V2", -200.0 * 0.2);
            TestingTools.verifyFlow(gs, "S1", -solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

        [TestMethod]
        public void S_BeforePump_V3HalfClosed()
        {
            FlowLine v3 = (FlowLine)gs.getComponent("V3");
            v3.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.5;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", -solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

        [TestMethod]
        public void S_BeforePump_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "S1", -solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

    }
}
