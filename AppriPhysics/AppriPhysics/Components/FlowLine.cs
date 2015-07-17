using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class FlowLine : FlowComponent
    {
        public FlowLine(String name, String sinkName) : base(name)
        {
            this.sinkName = sinkName;
        }

        private String sinkName;
        private FlowComponent source;
        private FlowComponent sink;
        private double flowAllowedPercent = 1.0f;
        private double maxFlow = Double.MaxValue;

        private Dictionary<String, double> finalFlows = new Dictionary<String, double>();

        public void setFlowAllowedPercent(double flowAllowedPercent)
        {
            this.flowAllowedPercent = Math.Min(Math.Max(flowAllowedPercent, 0.0), 1.0);               //Clamp the value to be between 0 and 1
        }

        public void setMaxFlow(double maxFlow)
        {
            this.maxFlow = maxFlow;
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        private double getLimitScaler(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            double limitScaler = Math.Min(curPercent, flowAllowedPercent);
            if (baseData.desiredFlowVolume * limitScaler > maxFlow)
            {
                //Need to cut down even further, so that we don't go over our maximum.
                limitScaler = maxFlow / baseData.desiredFlowVolume;
            }

            //Apply anger effects, if they are needed.
            if (baseData.angerMap.ContainsKey(name))
            {
                limitScaler *= baseData.angerMap[name];
            }

            return limitScaler;
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            double limitScaler = getLimitScaler(baseData, caller, curPercent);
            FlowResponseData ret = source.getSourcePossibleFlow(baseData, this, limitScaler);
            return ret;
        }
        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            double limitScaler = getLimitScaler(baseData, caller, curPercent);
            FlowResponseData ret = sink.getSinkPossibleFlow(baseData, this, limitScaler);
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

        public override double getFlow()
        {
            double flow = 0.0;
            foreach(double iter in finalFlows.Values)
            {
                flow += iter;
            }
            return flow;
        }

        public override void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            finalFlows[baseData.flowPusher.name + "_source"] = -1 * baseData.desiredFlowVolume * curPercent;
            source.setSourceFlow(baseData, this, curPercent);
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            finalFlows[baseData.flowPusher.name + "_sink"] = baseData.desiredFlowVolume * curPercent;
            sink.setSinkFlow(baseData, this, curPercent);
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
