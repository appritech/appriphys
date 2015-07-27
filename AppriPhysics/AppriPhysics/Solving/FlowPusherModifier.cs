using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class FlowPusherModifier
    {
        public FlowPusherModifier()
        {
            clearState();
        }
        public FlowResponseData sourceAbility = null;
        public FlowResponseData sinkAbility = null;

        public double flowPercent;
        public double minSinkFlowPercent;
        public double minSourceFlowPercent;

        public bool updateStateRequiresNewSolution(FlowResponseData sourceAbility, FlowResponseData sinkAbility)
        {
            bool needsNewSolution = false;
            this.sourceAbility = sourceAbility;
            this.sinkAbility = sinkAbility;
            double minFlowAbility = Math.Min(sourceAbility.flowPercent, sinkAbility.flowPercent);
            if (minFlowAbility < flowPercent)
            {
                flowPercent = minFlowAbility;
                needsNewSolution = true;
            }

            if (sourceAbility.flowPercent < minSourceFlowPercent)
                minSourceFlowPercent = sourceAbility.flowPercent;
            if (sinkAbility.flowPercent < minSinkFlowPercent)
                minSinkFlowPercent = sinkAbility.flowPercent;

            return needsNewSolution;
        }

        public void clearState()
        {
            flowPercent = 1.0;
            minSourceFlowPercent = 1.0;
            minSinkFlowPercent = 1.0;
        }
    }
}
