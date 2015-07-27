using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public abstract class FlowComponent
    {
        public FlowComponent(String name)
        {
            this.name = name;
        }

        public String name;
        protected double finalFlow;
        protected double inletPressure;
        protected double outletPressure;

        public virtual void connectSelf(Dictionary<String, FlowComponent> components)
        {
            //This should probably be abstract... but default can do nothing...
        }
        public virtual double getAngerLevel(Dictionary<String, double> angerMap)
        {
            return 0.0;                     //Most components can't get angry, so default is no anger!
        }
        public abstract void setSource(FlowComponent source);
        public double getFlow()
        {
            return finalFlow;
        }

        public virtual void resetState()
        {
            finalFlow = 0.0;
            inletPressure = 0.0;
            outletPressure = 0.0;
        }

        public abstract FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent,double pressurePercent);
        public abstract FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent);

        public abstract void setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume);
        public abstract void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume);

        public abstract void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller);
        public abstract void exploreSinkGraph(FlowCalculationData baseData, FlowComponent caller);

        protected void setPressuresForSinkSide(double pumpPressure, double backPressure, double inletPercent, double outletPercent)
        {
            double delta = pumpPressure - backPressure;
            inletPressure = inletPercent * delta + backPressure;
            outletPressure = outletPercent * delta + backPressure;
        }

        protected void setPressuresForSourceSide(double pumpPressure, double backPressure, double inletPercent, double outletPercent)
        {
            double delta =  backPressure - pumpPressure;
            inletPressure = backPressure - inletPercent * delta;
            outletPressure = backPressure - outletPercent * delta;
        }

        public String solutionString()
        {
            return name + " flow: " + getFlow() + " \tinPressure: " + inletPressure + " \toutPressure: " + outletPressure;
        }
    }
}
