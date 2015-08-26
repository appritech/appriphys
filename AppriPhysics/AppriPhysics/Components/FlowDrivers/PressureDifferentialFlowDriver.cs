using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components.FlowDrivers
{
    public class PressureDifferentialFlowDriver : FlowDriver
    {
        public PressureDifferentialFlowDriver(String name, double mcrRating, double mcrPressure, String deliveryName, double minDeltaP, double maxDeltaP) : base(name, mcrRating, mcrPressure, deliveryName)
        {
            this.minDeltaP = minDeltaP;
            this.maxDeltaP = maxDeltaP;
        }

        private double minDeltaP;
        private double maxDeltaP;
        private FlowResponseData lastSourcePossibleValue = null;

        public override void resetState()
        {
            base.resetState();
            lastSourcePossibleValue = null;
        }

        public override FlowResponseData getFlowDriverDeliveryPossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            pumpingPercent = lastSourcePossibleValue.flowPercent;
            mcrPressure = lastSourcePossibleValue.backPressure;
            FlowResponseData normalResponse = base.getFlowDriverDeliveryPossibleValues(baseData, modifier);

            double deltaP = lastSourcePossibleValue.backPressure - normalResponse.backPressure;
            if (deltaP < minDeltaP)
            {
                normalResponse.flowPercent = 0.0;
            }
            else
            {
                double flowPercent = 1.0;                   //Default to 1, in case deltaP > maxDeltaP, in which case, we do 100% of what we can.
                if (deltaP < maxDeltaP)
                {
                    flowPercent = (deltaP - minDeltaP) / (maxDeltaP - minDeltaP);
                }

                normalResponse.flowPercent = Math.Min(normalResponse.flowPercent, lastSourcePossibleValue.flowPercent) * flowPercent;
            }
            normalResponse.flowVolume = baseData.desiredFlowVolume * normalResponse.flowPercent;

            return normalResponse;
        }

        public override FlowResponseData getFlowDriverSourcePossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            pumpingPercent = 1.0;
            FlowResponseData normalResponse = base.getFlowDriverSourcePossibleValues(baseData, modifier);
            lastSourcePossibleValue = normalResponse;
            return normalResponse;
        }
    }
}
