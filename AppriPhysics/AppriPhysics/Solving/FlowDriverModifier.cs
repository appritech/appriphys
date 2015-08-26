using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class FlowDriverModifier
    {
        public FlowDriverModifier()
        {
            clearState();
        }
        public FlowResponseData sourceAbility = null;
        public FlowResponseData deliveryAbility = null;

        public double flowPercent;
        public double minDeliveryFlowPercent;
        public double minSourceFlowPercent;

        public bool updateStateRequiresNewSolution(FlowResponseData sourceAbility, FlowResponseData deliveryAbility)
        {
            bool needsNewSolution = false;
            this.sourceAbility = sourceAbility;
            this.deliveryAbility = deliveryAbility;
            double minFlowAbility = Math.Min(sourceAbility.flowPercent, deliveryAbility.flowPercent);
            if (minFlowAbility < flowPercent)
            {
                flowPercent = minFlowAbility;
                needsNewSolution = true;
            }

            if (sourceAbility.flowPercent < minSourceFlowPercent)
                minSourceFlowPercent = sourceAbility.flowPercent;
            if (deliveryAbility.flowPercent < minDeliveryFlowPercent)
                minDeliveryFlowPercent = deliveryAbility.flowPercent;

            return needsNewSolution;
        }

        public void clearState()
        {
            flowPercent = 1.0;
            minSourceFlowPercent = 1.0;
            minDeliveryFlowPercent = 1.0;
        }
    }
}
