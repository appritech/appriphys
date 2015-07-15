using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Solving;
using AppriPhysics.Components;

namespace UnitTests
{
    [TestClass]
    public class StraightPipeTests
    {
        [TestMethod]
        public void AllOpen()
        {
            GraphSolver gs = createGraph();
            //TODO: Dynamically set valve positions, etc.
            gs.solveMimic();
            double solutionFlow = 100.0;          //Single flow through whole system
            verifyFlow(gs, "T1", -solutionFlow);
            verifyFlow(gs, "V1", -solutionFlow);
            verifyFlow(gs, "P1", solutionFlow);
            verifyFlow(gs, "V2", solutionFlow);
            verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void V2HalfClosed()
        {
            GraphSolver gs = createGraph();
            //TODO: Dynamically set valve positions, etc.
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 50.0;          //Single flow through whole system
            verifyFlow(gs, "T1", -solutionFlow);
            verifyFlow(gs, "V1", -solutionFlow);
            verifyFlow(gs, "P1", solutionFlow);
            verifyFlow(gs, "V2", solutionFlow);
            verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void V1_25_PercentOpen_V2HalfClosed()
        {
            GraphSolver gs = createGraph();
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.25);
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 25.0;          //Single flow through whole system
            verifyFlow(gs, "T1", -solutionFlow);
            verifyFlow(gs, "V1", -solutionFlow);
            verifyFlow(gs, "P1", solutionFlow);
            verifyFlow(gs, "V2", solutionFlow);
            verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void V2Closed()
        {
            GraphSolver gs = createGraph();
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Single flow through whole system
            verifyFlow(gs, "T1", -solutionFlow);
            verifyFlow(gs, "V1", -solutionFlow);
            verifyFlow(gs, "P1", solutionFlow);
            verifyFlow(gs, "V2", solutionFlow);
            verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void T1Empty()
        {
            GraphSolver gs = createGraph();
            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Single flow through whole system
            verifyFlow(gs, "T1", -solutionFlow);
            verifyFlow(gs, "V1", -solutionFlow);
            verifyFlow(gs, "P1", solutionFlow);
            verifyFlow(gs, "V2", solutionFlow);
            verifyFlow(gs, "T2", solutionFlow);
        }

        public void verifyFlow(GraphSolver gs, String name, double flow)
        {
            double componentFlow = gs.getComponent(name).getFlow();
            Assert.AreEqual(flow, componentFlow, 0.00001);
        }

        private GraphSolver createGraph()
        {
            GraphSolver gs = new GraphSolver();

            Tank t1 = new Tank("T1", 1000.0, 500.0, new string[] { "V1" });
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            Pump p1 = new Pump("P1", 100.0, 3.2, "V2");
            gs.addComponent(p1);
            FlowLine v2 = new FlowLine("V2", "T2");
            gs.addComponent(v2);
            Tank t2 = new Tank("T2", 1000.0, 500.0, new string[] { });          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);

            gs.connectComponents();

            return gs;
        }
    }
}
