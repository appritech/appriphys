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

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if(currentVolume > 0.0)
            {
                ret.flowPercent = flowPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
                ret.flowVolume = flowPercent * baseData.desiredFlowVolume;
            }
            else
            {
                ret.flowPercent = 0.0f;
                ret.flowVolume = 0.0f;
            }
            ret.setLastCombinerOrTank(this, ret.flowPercent);
            return ret;
        }
        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if (currentVolume < double.MaxValue)             //TODO: Make it so that tanks can't overflow, especially sealed tanks... Right now, it will pretty much always accept any flow that we want to put into it...
            {
                ret.flowPercent = flowPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
                ret.flowVolume = flowPercent * baseData.desiredFlowVolume;
            }
            else
            {
                ret.flowPercent = 0.0f;
                ret.flowVolume = 0.0f;
            }
            ret.setLastCombinerOrTank(this, ret.flowPercent);
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

        public override void setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            finalFlow += flowVolume;
        }

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            finalFlow += flowVolume;
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
