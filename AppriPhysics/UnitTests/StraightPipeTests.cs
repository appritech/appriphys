﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Solving;
using AppriPhysics.Components;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class StraightPipeTests
    {

        private GraphSolver gs;
        private Dictionary<FluidType, double> plainWater = new Dictionary<FluidType, double>();

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();
            plainWater.Add(FluidType.WATER, 1.0);

            Tank t1 = new Tank("T1", 1000.0, plainWater, 500.0, new string[] { "V1" }, false);
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            FlowDriver p1 = new FlowDriver("P1", 100.0, 3.2, "V2");
            gs.addComponent(p1);
            FlowLine v2 = new FlowLine("V2", "T2");
            gs.addComponent(v2);
            Tank t2 = new Tank("T2", 1000.0, plainWater, 500.0, new string[] { }, false);          //We have no sinks, since we are the bottom of this food-chain.
            gs.addComponent(t2);

            gs.connectComponents();
        }

        [TestMethod]
        public void Straight_Time_TransferAll()
        {
            PhysTools.timeStep = 0.1f;

            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(500.0);
            Tank t2 = (Tank)gs.getComponent("T2");
            t2.setCurrentVolume(0.0);
            for (int i = 0; i < 60; i++)
                gs.solveMimic();                        //10 steps means 1 full second. Thus, we should have transferred 100 of the 500 from T1 to T2

            double solutionFlow = 0.0;          //We ran the first tank dry, so no more flow anymore
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);

            Assert.AreEqual(0.0, t1.getCurrentVolume(), 0.00001);
            Assert.AreEqual(500.0, t2.getCurrentVolume(), 0.00001);
        }

        [TestMethod]
        public void Straight_Time_TransferPart()
        {
            PhysTools.timeStep = 0.1f;

            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(500.0);
            Tank t2 = (Tank)gs.getComponent("T2");
            t2.setCurrentVolume(0.0);
            for(int i = 0; i < 10; i++)
                gs.solveMimic();                        //10 steps means 1 full second. Thus, we should have transferred 100 of the 500 from T1 to T2

            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);

            Assert.AreEqual(400.0, t1.getCurrentVolume(), 0.00001);
            Assert.AreEqual(100.0, t2.getCurrentVolume(), 0.00001);
        }

        [TestMethod]
        public void Straight_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void Straight_V2HalfClosed()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 50.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void Straight_V1_25_PercentOpen_V2HalfClosed()
        {
            FlowLine v1 = (FlowLine)gs.getComponent("V1");
            v1.setFlowAllowedPercent(0.25);
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.5);
            gs.solveMimic();
            double solutionFlow = 25.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void Straight_V2Closed()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

        [TestMethod]
        public void Straight_T1Empty()
        {
            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(0.0);
            gs.solveMimic();
            double solutionFlow = 0.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }

    }
}
