using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class Tank : FlowComponent
    {

        public Tank(String name, double capacity, double currentVolume, String[] sinkNames) : base(name)
        {
            this.sinkNames = sinkNames;
            this.capacity = capacity;
            this.currentVolume = currentVolume;
        }

        private String[] sinkNames;
        //private FlowComponent sink;             //Needs to be a collection of sinks, if we think we want to use it.
        private double capacity;
        private double currentVolume;

        private Dictionary<String, double> finalFlows = new Dictionary<String, double>();

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            if (sinkNames != null && sinkNames.Length > 0)
            {
                for (int i = 0; i < sinkNames.Length; i++)
                {
                    FlowComponent sink = components[sinkNames[i]];
                    sink.setSource(this);
                }
            }
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if(currentVolume > 0.0)
            {
                ret.flowPercent = curPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
                ret.flowVolume = curPercent * baseData.desiredFlowVolume;
            }
            else
            {
                ret.flowPercent = 0.0f;
                ret.flowVolume = 0.0f;
            }
            return ret;
        }
        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if (currentVolume < double.MaxValue)             //TODO: Make it so that tanks can't overflow, especially sealed tanks... Right now, it will pretty much always accept any flow that we want to put into it...
            {
                ret.flowPercent = curPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
                ret.flowVolume = curPercent * baseData.desiredFlowVolume;
            }
            else
            {
                ret.flowPercent = 0.0f;
                ret.flowVolume = 0.0f;
            }
            return ret;
        }
        public override void setSource(FlowComponent source)
        {
            //Tanks are the ultimate source... so we don't really need to do anything right now, until we are tracking the multiple inputs and outputs... Then, we will need to keep track of all of them probably...
        }

        public void setCurrentVolume(double volume)
        {
            this.currentVolume = volume;
        }

        public override double getFlow()
        {
            double flow = 0.0;
            foreach (double iter in finalFlows.Values)
            {
                flow += iter;
            }
            return flow;
        }

        public override void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            finalFlows[baseData.flowPusher.name + "_" + caller.name + "_source"] = -1 * baseData.desiredFlowVolume * curPercent;
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            finalFlows[baseData.flowPusher.name + "_" + caller.name + "_sink"] = baseData.desiredFlowVolume * curPercent;
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }

        public override void exploreSinkGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }
    }
}
