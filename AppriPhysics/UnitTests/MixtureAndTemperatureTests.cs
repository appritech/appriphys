using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AppriPhysics.Components;
using AppriPhysics.Solving;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class MixtureAndTemperatureTests
    {
        private GraphSolver gs;
        private Dictionary<FluidType, double> plainWater = new Dictionary<FluidType, double>();
        private Dictionary<FluidType, double> seaWater = new Dictionary<FluidType, double>();

        [TestInitialize()]
        public void InitializeGraph()
        {
            gs = new GraphSolver();
            plainWater.Add(FluidType.WATER, 1.0);
            seaWater.Add(FluidType.SEA_WATER, 1.0);
            
            Tank t1 = new Tank("T1", 1000.0, plainWater, 500.0, new string[] { "V1" }, false);
            gs.addComponent(t1);
            t1.currentTemperature = 20.0;
            Tank t2 = new Tank("T2", 1000.0, seaWater, 500.0, new string[] { "V2" }, false);
            gs.addComponent(t2);
            t2.currentTemperature = 10.0;
            FlowLine v1 = new FlowLine("V1", "S1");
            gs.addComponent(v1);
            FlowLine v2 = new FlowLine("V2", "S1");
            gs.addComponent(v2);
            Junction s1 = new Junction("S1", new string[] { "P1" }, new string[] { "V1", "V2" }, "", new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 });
            gs.addComponent(s1);
            Pump p1 = new Pump("P1", 300.0, 3.2, "S2");
            gs.addComponent(p1);
            Junction s2 = new Junction("S2", new string[] { "V3", "V4" }, new string[] { "P1" }, "C2", new double[] { 0.5, 0.5 }, new double[] { 1.0, 1.0 });
            gs.addComponent(s2);
            FlowLine v3 = new FlowLine("V3", "C2");
            gs.addComponent(v3);
            FlowLine v4 = new FlowLine("V4", "C2");
            gs.addComponent(v4);
            Junction c2 = new Junction("C2", new string[] { "T3" }, new string[] { "V3", "V4" });
            gs.addComponent(c2);
            Tank t3 = new Tank("T3", 1000.0, plainWater, 500.0, new string[] { }, false);          //We have no sinks, since we are the bottom of this food-chain.
            t3.currentTemperature = 30.0;
            gs.addComponent(t3);
            
            gs.connectComponents();
        }

        [TestMethod]
        public void MT_V2_25Percent()
        {
            FlowLine v2 = (FlowLine)gs.getComponent("V2");
            v2.setFlowAllowedPercent(0.25);
            gs.solveMimic();
            Dictionary<FluidType, double> mixture = new Dictionary<FluidType, double>();
            mixture.Add(FluidType.WATER, 0.8);
            mixture.Add(FluidType.SEA_WATER, 0.2);
            double temp1 = 20.0;
            double temp2 = 10.0;
            double tempMix = 18.0;
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow * 0.8);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", -solutionFlow * 0.2);
            TestingTools.verifyFlow(gs, "V1", solutionFlow * 0.8);
            TestingTools.verifyMixtureAndTemperature(gs, "V1", plainWater, temp1);
            TestingTools.verifyFlow(gs, "V2", solutionFlow * 0.2);
            TestingTools.verifyMixtureAndTemperature(gs, "V2", seaWater, temp2);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "P1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S2", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V3", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V4", mixture, tempMix);               //V4 really should be the mixture!
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

        [TestMethod]
        public void MT_T1Empty()
        {
            Tank t1 = (Tank)gs.getComponent("T1");
            t1.setCurrentVolume(0.0);
            gs.solveMimic();
            Dictionary<FluidType, double> mixture = new Dictionary<FluidType, double>();
            //mixture.Add(FluidType.WATER, 0.0);
            mixture.Add(FluidType.SEA_WATER, 1.0);
            double temp1 = 20.0;
            double temp2 = 10.0;
            double tempMix = 10.0;
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0 * 0.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", -solutionFlow);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0 * 0.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V1", plainWater, temp1);
            TestingTools.verifyFlow(gs, "V2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "V2", seaWater, temp2);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "P1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S2", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V3", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V4", mixture, tempMix);               //V4 really should be the mixture!
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

        [TestMethod]
        public void MT_Time_AllOpen_T3HasStuff()
        {
            PhysTools.timeStep = 0.1f;

            Tank t3 = (Tank)gs.getComponent("T3");
            t3.setCurrentVolume(300.0);                     //Start T3 empty, so that it does no internal mixing, just the mixing in the pipes and what comes into us.
            t3.currentTemperature = 45.0;                   //Start T3 at 45 degrees, and we will be adding 15 degree fluid, so it will end up at 30 (half and half).
            //Note: T3 also started with plainWater, so it will end up 75% plainWater and 25% seaWater

            for (int i = 0; i < 10; i++)
                gs.solveMimic();                        //Let 1 full second run.

            Dictionary<FluidType, double> mixture = new Dictionary<FluidType, double>();
            mixture.Add(FluidType.WATER, 0.5);
            mixture.Add(FluidType.SEA_WATER, 0.5);

            Dictionary<FluidType, double> t3Mixture = new Dictionary<FluidType, double>();
            t3Mixture.Add(FluidType.WATER, 0.75);
            t3Mixture.Add(FluidType.SEA_WATER, 0.25);
            double t3Temp = 30.0;

            double temp1 = 20.0;
            double temp2 = 10.0;
            double tempMix = 15.0;
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V1", plainWater, temp1);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V2", seaWater, temp2);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "P1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S2", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V3", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V4", mixture, tempMix);               //V4 really should be the mixture!
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "T3", t3Mixture, t3Temp);

            Assert.AreEqual(600.0, t3.getCurrentVolume(), 0.0001);
        }

        [TestMethod]
        public void MT_Time_AllOpen_T3EmptyToStart()
        {
            PhysTools.timeStep = 0.1f;

            Tank t3 = (Tank)gs.getComponent("T3");
            t3.setCurrentVolume(0.0);                   //Start T3 empty, so that it does no internal mixing, just the mixing in the pipes and what comes into us.

            for(int i = 0; i < 10; i++)
                gs.solveMimic();                        //Let 1 full second run.

            Dictionary<FluidType, double> mixture = new Dictionary<FluidType, double>();
            mixture.Add(FluidType.WATER, 0.5);
            mixture.Add(FluidType.SEA_WATER, 0.5);
            double temp1 = 20.0;
            double temp2 = 10.0;
            double tempMix = 15.0;
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V1", plainWater, temp1);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V2", seaWater, temp2);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "P1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S2", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V3", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V4", mixture, tempMix);               //V4 really should be the mixture!
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "T3", mixture, tempMix);

            Assert.AreEqual(300.0, t3.getCurrentVolume(), 0.0001);
        }

        [TestMethod]
        public void MT_AllOpen()
        {
            gs.solveMimic();
            Dictionary<FluidType, double> mixture = new Dictionary<FluidType, double>();
            mixture.Add(FluidType.WATER, 0.5);
            mixture.Add(FluidType.SEA_WATER, 0.5);
            double temp1 = 20.0;
            double temp2 = 10.0;
            double tempMix = 15.0;
            double solutionFlow = 300.0;          //Basic flow through system, but the branches should share half
            TestingTools.verifyFlow(gs, "T1", -solutionFlow / 2.0);             //Zero flow in tanks, because we suck out the same that we put back in.
            TestingTools.verifyFlow(gs, "T2", -solutionFlow / 2.0);
            TestingTools.verifyFlow(gs, "V1", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V1", plainWater, temp1);
            TestingTools.verifyFlow(gs, "V2", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V2", seaWater, temp2);
            TestingTools.verifyFlow(gs, "S1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "P1", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "P1", mixture, tempMix);
            TestingTools.verifyFlow(gs, "S2", solutionFlow);
            TestingTools.verifyMixtureAndTemperature(gs, "S2", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V3", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V3", mixture, tempMix);
            TestingTools.verifyFlow(gs, "V4", solutionFlow / 2.0);
            TestingTools.verifyMixtureAndTemperature(gs, "V4", mixture, tempMix);               //V4 really should be the mixture!
            TestingTools.verifyFlow(gs, "T3", solutionFlow);
        }

    }
}
