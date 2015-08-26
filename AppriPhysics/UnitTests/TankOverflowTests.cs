using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Solving;
using AppriPhysics.Components;
using AppriPhysics.Components.FlowDrivers;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class TankOverflowTests
    {
        private GraphSolver gs;
        private Dictionary<FluidType, double> plainWater = new Dictionary<FluidType, double>();

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();
            plainWater.Add(FluidType.WATER, 1.0);

            Tank t1 = new Tank("T1", 100000.0, plainWater, 100000.0, new string[] { "V1" }, false);             //Infinite tank!
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            FlowDriver p1 = new FlowDriver("P1", 100.0, 4.0, "V2");
            gs.addComponent(p1);
            FlowLine v2 = new FlowLine("V2", "T2");
            gs.addComponent(v2);
            Tank t2 = new Tank("T2", 10000.0, plainWater, 0.0, new string[] { "P2" }, false);             //Sealed tanks should start full of air, so currentVolume = capacity.
            gs.addComponent(t2);


            TankOverflowFlowDriver p2 = new TankOverflowFlowDriver("P2", 100.0, 0.0, "V3", 0.95, .951);
            gs.addComponent(p2);
            FlowLine v3 = new FlowLine("V3", "T3");
            gs.addComponent(v3);
            Tank t3 = new Tank("T3", 10000.0, plainWater, 0.0, new string[] { }, false);             //Sealed tanks should start full of air, so currentVolume = capacity.
            gs.addComponent(t3);

            gs.connectComponents();
        }

        [TestMethod]
        public void TankOverflow_Time_Overflowing()
        {
            Tank t2 = (Tank)gs.getComponent("T2");
            Tank t3 = (Tank)gs.getComponent("T3");

            for (int i = 0; i < 1500; i++)
                gs.solveMimic();                //In 1500 cycles, we should have pumped 15000.0 total. That means T2 will be 95% full (9500), and T3 will have the rest (5500)

            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);

            Assert.AreEqual(9500.0, t2.getCurrentVolume(), 25.0);               //Since the overflow line moves a LOT of liquid each time step, sometimes we get ahead and sometimes behind, but averages out.
            Assert.AreEqual(5500.0, t3.getCurrentVolume(), 25.0);
        }

        [TestMethod]
        public void TankOverflow_SingleRun_Overflowing()
        {
            Tank t2 = (Tank)gs.getComponent("T2");
            t2.setCurrentVolume(10000.0);           //Initialize t2 as 100% full
            t2.percentFilled = 1.0;                 //NOTE: This will update itself after the first cycle, but we should probably tie them together in a setter, etc...

            gs.solveMimic();
            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow * 0.0);              //There is 100 in and 100 out, so that nets 0 for T2
            TestingTools.verifyFlow(gs, "P2", solutionFlow);
            TestingTools.verifyFlow(gs, "V3", solutionFlow);
            TestingTools.verifyFlow(gs, "T3", solutionFlow);             //No overflow yet
        }

        [TestMethod]
        public void TankOverflow_AllOpen_NoOverflow()
        {
            gs.solveMimic();
            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
            TestingTools.verifyFlow(gs, "P2", 0.0);
            TestingTools.verifyFlow(gs, "V3", 0.0);
            TestingTools.verifyFlow(gs, "T3", 0.0);             //No overflow yet
        }

    }
}
