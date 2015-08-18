using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Solving;
using AppriPhysics.Components;
using AppriPhysics.Components.Pumps;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class StraightCompressorTests
    {
        private GraphSolver gs;
        private Dictionary<FluidType, double> plainAir = new Dictionary<FluidType, double>();

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();
            plainAir.Add(FluidType.AIR, 1.0);

            Tank t1 = new Tank("T1", double.MaxValue, plainAir, double.MaxValue, new string[] { "V1" }, false);             //Infinite tank!
            gs.addComponent(t1);
            FlowLine v1 = new FlowLine("V1", "P1");
            gs.addComponent(v1);
            Pump p1 = new Pump("P1", 100.0, 4.0, "V2");
            gs.addComponent(p1);
            FlowLine v2 = new FlowLine("V2", "T2");
            gs.addComponent(v2);
            Tank t2 = new Tank("T2", 1000.0, plainAir, 1000.0, new string[] { "P2" }, true);             //Sealed tanks should start full of air, so currentVolume = capacity.
            gs.addComponent(t2);


            PressureDifferentialPump p2 = new PressureDifferentialPump("P2", 100.0, 0.0, "V3", 1.0, 2.0);
            gs.addComponent(p2);
            FlowLine v3 = new FlowLine("V3", "T3");
            v3.setFlowAllowedPercent(0.0);
            gs.addComponent(v3);
            Tank t3 = new Tank("T3", 1000.0, plainAir, 1000.0, new string[] { }, true);             //Sealed tanks should start full of air, so currentVolume = capacity.
            gs.addComponent(t3);

            gs.connectComponents();
        }

        [TestMethod]
        public void Straight_Comp_Time_FillBottleWithAir()
        {
            PhysTools.timeStep = 0.1f;

            Tank t1 = (Tank)gs.getComponent("T1");
            Tank t2 = (Tank)gs.getComponent("T2");
            t2.normalPressureDelta = 4.0;

            for (int i = 0; i < 20000; i++)
                gs.solveMimic();
            
            double solutionFlow = 0.0;          //We ran the first tank dry, so no more flow anymore
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);

            //We should get to 4 bar, since that is how high our 'pump' or compressor can go...

            Assert.AreEqual(5000.0, t2.getCurrentVolume(), 0.00001);                //4 barg is 5 bara, which is 5 times more air than it started with...
            Assert.AreEqual(4.0, t2.getTankPressure(), 0.00001);
        }

        [TestMethod]
        public void Straight_Comp_Time_FillBothBottlesWithAir()
        {
            PhysTools.timeStep = 0.1f;

            Tank t1 = (Tank)gs.getComponent("T1");
            Tank t2 = (Tank)gs.getComponent("T2");
            Tank t3 = (Tank)gs.getComponent("T3");
            t2.normalPressureDelta = 4.0;
            t3.normalPressureDelta = 4.0;


            FlowLine v3 = (FlowLine)gs.getComponent("V3");
            v3.setFlowAllowedPercent(1.0);

            for (int i = 0; i < 20000; i++)
                gs.solveMimic();

            gs.solveMimic();

            double solutionFlow = 0.0;          //We ran the first tank dry, so no more flow anymore
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);

            //We should get to 4 bar, since that is how high our 'pump' or compressor can go...

            Assert.AreEqual(5000.0, t2.getCurrentVolume(), 0.0001);                //4 barg is 5 bara, which is 5 times more air than it started with...
            Assert.AreEqual(4.0, t2.getTankPressure(), 0.00001);
            Assert.AreEqual(3.0, t3.getTankPressure(), 0.00001);                    //T3 should fill up to 3 bar, which is 1 bar lower than t2, because of the 1.0 bar min pressure difference to cause flow between them.
        }

        [TestMethod]
        public void Straight_Comp_Time_FillBottleWithWater()
        {
            //This kind of simulates a hydrophore system, where we have water and air at the top.
            PhysTools.timeStep = 0.1f;

            Tank t1 = (Tank)gs.getComponent("T1");
            t1.overrideFluidType(FluidType.WATER);
            Tank t2 = (Tank)gs.getComponent("T2");
            t2.normalPressureDelta = 4.0;

            for (int i = 0; i < 20000; i++)
                gs.solveMimic();                        //10 steps means 1 full second. Thus, we should have transferred 100 of the 500 from T1 to T2'

            gs.solveMimic();

            double solutionFlow = 0.0;          //We ran the first tank dry, so no more flow anymore
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);

            //We should get to 4 bar, since that is how high our 'pump' or compressor can go...

            Assert.AreEqual(0.8, t2.percentFilled, 0.00001);               //4 barg (i.e. 5 bara) pressure is reached when the air is taking up 1/5 of the volume, leaving 4/5 for the water...
            Assert.AreEqual(4.0, t2.getTankPressure(), 0.00001);
        }

        [TestMethod]
        public void Straight_Comp_AllOpen()
        {
            gs.solveMimic();
            double solutionFlow = 100.0;          //Single flow through whole system
            TestingTools.verifyFlow(gs, "T1", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyFlow(gs, "T2", solutionFlow);
        }
        
    }
}
