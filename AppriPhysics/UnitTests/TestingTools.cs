using System;
using System.Collections.Generic;
using AppriPhysics.Solving;
using AppriPhysics.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    public class TestingTools
    {
        public static void verifyFlow(GraphSolver gs, String name, double flow)
        {
            double componentFlow = gs.getComponent(name).getFlow();
            Assert.AreEqual(flow, componentFlow, 0.00001);
        }

        public static void verifyMixtureAndTemperature(GraphSolver gs, string name, Dictionary<FluidType, double> truthMap, double temp)
        {
            Dictionary<FluidType, double> componentMap = gs.getComponent(name).getCurrentFluidTypeMap();
            double componentTemp = gs.getComponent(name).getInletTemperature();
            Assert.AreEqual(temp, componentTemp, 0.00001);
            foreach(KeyValuePair<FluidType, double> iter in truthMap)
            {
                Assert.AreEqual(true, componentMap.ContainsKey(iter.Key));
                Assert.AreEqual(iter.Value, componentMap[iter.Key], 0.00001);
            }
        }
    }
}
