using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public abstract class FlowComponent
    {
        public FlowComponent(String name)
        {
            this.name = name;
        }

        public String name;
        protected Dictionary<String, FlowResponseData> lastResponses;
        
        public virtual void connectSelf(Dictionary<String, FlowComponent> components)
        {
            //This should probably be abstract... but default can do nothing...
        }
        public abstract void setSource(FlowComponent source);
        public abstract double getFlow();

        public abstract FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent);
        public abstract FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent);

        public abstract void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent);
        public abstract void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent);

        public String solutionString()
        {
            return name + " flow: " + getFlow();
        }
    }
}
