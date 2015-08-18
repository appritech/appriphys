using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components.Pumps
{
    public class PressureDifferentialPump : Pump
    {
        public PressureDifferentialPump(String name, double mcrRating, double mcrPressure, String sinkName, double minDeltaP, double maxDeltaP) : base(name, mcrRating, mcrPressure, sinkName)
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

        public override FlowResponseData getPumpSinkPossibleValues(FlowCalculationData baseData, FlowPusherModifier modifier)
        {
            pumpingPercent = lastSourcePossibleValue.flowPercent;
            mcrPressure = lastSourcePossibleValue.backPressure;
            FlowResponseData normalResponse = base.getPumpSinkPossibleValues(baseData, modifier);

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

        public override FlowResponseData getPumpSourcePossibleValues(FlowCalculationData baseData, FlowPusherModifier modifier)
        {
            pumpingPercent = 1.0;
            FlowResponseData normalResponse = base.getPumpSourcePossibleValues(baseData, modifier);
            lastSourcePossibleValue = normalResponse;
            return normalResponse;
        }
    }
}
