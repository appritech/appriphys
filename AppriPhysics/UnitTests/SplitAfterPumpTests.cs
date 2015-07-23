using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class SplitAfterPumpTests
    {
        private GraphSolver gs;

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();

            Tank t1 = new Tank("T1", 1000.0, 500.0, new string[] { "V1" });
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            Pump p1 = new Pump("P1", 200.0, 3.2, "S1");
            gs.addComponent(p1);
            Junction s1 = new Junction("S1", new string[] { "V2", "V3" }, new string[] { "P1" }, new double[] { 0.5, 0.5 }, new double[] { 0.6, 1.0 });
            gs.addComponent(s1);
            FlowLine v2 = new FlowLine("V2", "T2");
            gs.addComponent(v2);
            Tank t2 = new Tank("T2", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);
            FlowLine v3 = new FlowLine("V3", "T3");
            gs.addComponent(v3);
            Tank t3 = new Tank("T3", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t3);

            gs.connectComponents();
        }

        [TestMethod]
        public void S_AfterPump_V3Closed()
        {
            FlowLine v3 = (FlowLine)gs.getComponent("V3");
            v3.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.6;          //Basic flow through system
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", 0.0);
            TestingTools.verifyFlow(gs, "T3", 0.0);
        }

        [TestMethod]
        public void S_AfterPump_V2Closed()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", 0.0);
            TestingTools.verifyFlow(gs, "T2", 0.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }


        [TestMethod]
        public void S_AfterPump_V3_20_Percent_Open()
        {
            FlowLine v3 = (FlowLine)gs.getComponent("V3");
            v3.setFlowAllowedPercent(0.2);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.8;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", 200.0 * 0.6);
            TestingTools.verifyFlow(gs, "T2", 200.0 * 0.6);
            TestingTools.verifyFlow(gs, "V3", 200.0 * 0.2);
            TestingTools.verifyFlow(gs, "T3", 200.0 * 0.2);
        }

        [TestMethod]
        public void S_AfterPump_V1HalfClosed()
        {
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 200.0 * 0.5;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T3", solutionFlow / 2.0);
        }

        [TestMethod]
        public void S_AfterPump_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 200.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T2", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "T3", solutionFlow / 2.0);
        }

    }
}
