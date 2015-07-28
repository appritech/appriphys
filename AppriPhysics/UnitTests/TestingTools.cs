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

        public static void verifyMixture(GraphSolver gs, string name, Dictionary<FluidType, double> truthMap)
        {
            Dictionary<FluidType, double> componentMap = gs.getComponent(name).getLastFluidTypeMap();
            foreach(KeyValuePair<FluidType, double> iter in truthMap)
            {
                Assert.AreEqual(true, componentMap.ContainsKey(iter.Key));
                Assert.AreEqual(iter.Value, componentMap[iter.Key], 0.00001);
            }
        }
    }
}
