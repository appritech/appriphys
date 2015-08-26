using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components.FlowDrivers
{
    public class TankOverflowFlowDriver : FlowDriver
    {
        public TankOverflowFlowDriver(String name, double mcrRating, double mcrPressure, String sinkName, double minTankPercent, double maxTankPercent) : base(name, mcrRating, mcrPressure, sinkName)
        {
            this.minTankPercent = minTankPercent;
            this.maxTankPercent = maxTankPercent;
        }

        private double minTankPercent;
        private double maxTankPercent;
        private Tank sourceTank;

        public override void connectSelf(Dictionary<string, FlowComponent> components)
        {
            base.connectSelf(components);

            if (source is Tank)
                sourceTank = (Tank)source;
            else
                throw new InvalidCastException("TankOverflowPump must have a Tank as its input source");
        }

        public override FlowResponseData getFlowDriverSinkPossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            FlowResponseData normalResponse = base.getFlowDriverSinkPossibleValues(baseData, modifier);
            //I don't think that we actually need to do anything special on the sink side of things.
            return normalResponse;
        }

        public override FlowResponseData getFlowDriverSourcePossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            if(sourceTank.percentFilled > minTankPercent)
            {
                pumpingPercent = 1.0;               //Assume full force, unless we are lower than the maximum
                if(sourceTank.percentFilled < maxTankPercent)
                {
                    pumpingPercent = (sourceTank.percentFilled - minTankPercent) / (maxTankPercent - minTankPercent);
                }
                FlowResponseData normalResponse = base.getFlowDriverSourcePossibleValues(baseData, modifier);
                return normalResponse;
            }
            else
            {
                pumpingPercent = 0.0;
                FlowResponseData ret = new FlowResponseData();
                ret.fluidTypeMap = sourceTank.getCurrentFluidTypeMap();
                return ret;
            }
            
        }
    }
}
