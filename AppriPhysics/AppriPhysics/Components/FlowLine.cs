using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class FlowLine : FlowComponent
    {
        public FlowLine(String name, String sinkName) : base(name)
        {
            this.sinkName = sinkName;
            normalPressureDropPercent = 0.95;
        }

        private String sinkName;
        private FlowComponent source;
        private FlowComponent sink;
        private double flowAllowedPercent = 1.0f;
        private double maxFlow = Double.MaxValue;
        private double normalPressureDropPercent;

        public void setFlowAllowedPercent(double flowAllowedPercent)
        {
            this.flowAllowedPercent = Math.Min(Math.Max(flowAllowedPercent, 0.0), 1.0);               //Clamp the value to be between 0 and 1
        }

        public void setMaxFlow(double maxFlow)
        {
            this.maxFlow = maxFlow;
        }

        public void setNormalPressureDropPercent(double normalPressureDropPercent)
        {
            this.normalPressureDropPercent = normalPressureDropPercent;
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        private double getLimitedFlowPercent(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            double limitedFlowPercent = Math.Min(flowPercent, flowAllowedPercent);
            if (baseData.desiredFlowVolume * limitedFlowPercent > maxFlow)
            {
                //Need to cut down even further, so that we don't go over our maximum.
                limitedFlowPercent = maxFlow / baseData.desiredFlowVolume;
            }

            //Apply anger effects, if they are needed.
            if (baseData.angerMap.ContainsKey(name))
            {
                limitedFlowPercent *= baseData.angerMap[name];
            }

            return limitedFlowPercent;
        }

        private double getLimitedPressurePercent(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            double limitedPressurePercent = pressurePercent * normalPressureDropPercent;

            if(flowPercent == 0.0)
            {
                limitedPressurePercent = 0.0;
            }
            else if(flowAllowedPercent < flowPercent)
            {
                //limitedPressurePercent *= flowAllowedPercent / flowPercent;                   //If flowPercent is included, then splitters with different maxes don't work well...
                limitedPressurePercent *= flowAllowedPercent;
            }

            return limitedPressurePercent;
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            double limitedFlowPercent = getLimitedFlowPercent(baseData, caller, flowPercent);
            double limitedPressurePercent = getLimitedPressurePercent(baseData, caller, flowPercent, pressurePercent);
            FlowResponseData ret = source.getSourcePossibleValues(baseData, this, limitedFlowPercent, limitedPressurePercent);
            if (ret != null)
            {
                setPressuresForSourceSide(baseData.pressure, ret.backPressure, limitedPressurePercent, pressurePercent);
            }
            return ret;
        }
        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            double limitedFlowPercent = getLimitedFlowPercent(baseData, caller, flowPercent);
            double limitedPressurePercent = getLimitedPressurePercent(baseData, caller, flowPercent, pressurePercent);
            FlowResponseData ret = sink.getSinkPossibleValues(baseData, this, limitedFlowPercent, limitedPressurePercent);
            if (ret != null)
            {
                setPressuresForSinkSide(baseData.pressure, ret.backPressure, pressurePercent, limitedPressurePercent);
            }

            return ret;
        }

        public override void setSource(FlowComponent source)
        {
            this.source = source;
        }

        public override double getAngerLevel(Dictionary<String, double> angerMap)
        {
            double flow = Math.Abs(getFlow());              //Source lines have negative flow, so we need to make sure we look at ABS.
            if(angerMap.ContainsKey(name))
            {
                //If we were already angry, we should check and make sure that we don't need to forgive
                if (maxFlow - flow > 0.1)           //If our current flow is significantly lower than our max flow, then we need to forgive
                    return -1.0;                    //Send negative value to cause forgiveness
            }
            if (flow > maxFlow)
                return maxFlow / flow;
            return 0.0;
        }

        public override SettingResponseData setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            finalFlow = flowVolume;
            SettingResponseData ret = source.setSourceValues(baseData, this, flowVolume, lastTime);
            if (ret != null)
            {
                lastFluidTypeMap = ret.fluidTypeMap;                    //On source side, the mixture comes from the return values
                inletTemperature = ret.temperature;
                outletTemperature = ret.temperature;
            }
            return ret;
        }

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            finalFlow = flowVolume;
            sink.setSinkValues(baseData, this, flowVolume, lastTime);
            lastFluidTypeMap = baseData.fluidTypeMap;               //On the sink side, the mixture comes from passed in arguments
            inletTemperature = baseData.temperature;
            outletTemperature = baseData.temperature;
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            source.exploreSourceGraph(baseData, this);
        }

        public override void exploreSinkGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            sink.exploreSinkGraph(baseData, this);
        }
    }
}
