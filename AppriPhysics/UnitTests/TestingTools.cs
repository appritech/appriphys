using System;
using System.Collections.Generic;
using AppriPhysics.Solving;
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
    }
}
