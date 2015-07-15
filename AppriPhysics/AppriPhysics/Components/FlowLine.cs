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

        private Dictionary<String, double> finalFlows = new Dictionary<String, double>();

        public void setFlowAllowedPercent(double flowAllowedPercent)
        {
            this.flowAllowedPercent = Math.Min(Math.Max(flowAllowedPercent, 0.0), 1.0);               //Clamp the value to be between 0 and 1
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            FlowResponseData ret = source.getSourcePossibleFlow(baseData, this, curPercent * flowAllowedPercent);
            return ret;
        }
        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            FlowResponseData ret = sink.getSinkPossibleFlow(baseData, this, curPercent * flowAllowedPercent);
            return ret;
        }
        public override void setSource(FlowComponent source)
        {
            this.source = source;
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
            finalFlows.Add(baseData.flowPusher.name + "_source", -1 * baseData.desiredFlowVolume * curPercent);
            source.setSourceFlow(baseData, this, curPercent);
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            finalFlows.Add(baseData.flowPusher.name + "_sink", baseData.desiredFlowVolume * curPercent);
            sink.setSinkFlow(baseData, this, curPercent);
        }
    }
}
