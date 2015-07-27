using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class Pump : FlowComponent
    {

        public Pump(String name, double mcrRating, double mcrPressure, String sinkName) : base(name)
        {
            this.mcrRating = mcrRating;
            this.mcrPressure = mcrPressure;
            this.sinkName = sinkName;
        }

        public double mcrRating;
        private double mcrPressure;
        private String sinkName;
        private FlowComponent source;
        private FlowComponent sink;
        private double pumpingPercent = 1.0f;

        private void setPumpingPercent(double pumpingPercent)
        {
            this.pumpingPercent = pumpingPercent;
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        private double calculateOutletPressure(FlowPusherModifier modifier)
        {
            double outletPressure = mcrPressure * pumpingPercent * modifier.minSourceFlowPercent;           //The source is the main thing that can drop the pressure
            if (modifier.minSourceFlowPercent > modifier.minSinkFlowPercent && modifier.minSourceFlowPercent > 0.0)
            {
                //This is functionality more typical of a centrifugal pump.
                //If the sink is more clogged than the source, then we can add back some because of back pressure.
                outletPressure *= (1.0 + 0.20 * (1.0 - modifier.minSinkFlowPercent / modifier.minSourceFlowPercent));                   //Allow up to 20% higher pressure if the output is clogged
            }
            return outletPressure;
        }

        private double calculateInletPressure(FlowPusherModifier modifier)
        {
            double inletPressure = -0.2 * pumpingPercent;                           //By default, it will be about -0.2 bar when running at 100% normally
            inletPressure *= 3.0 * (1.0 - modifier.minSourceFlowPercent);           //If the source is blocked, it can increase by up to 3x
            return outletPressure;
        }

        public FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowPusherModifier modifier)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * modifier.flowPercent;
            baseData.pressure = calculateOutletPressure(modifier);
            outletPressure = baseData.pressure;
            return sink.getSinkPossibleValues(baseData, this, 1.0, 1.0);             //Always ask 100% of whatever desired flow we have
        }

        public FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowPusherModifier modifier)
        {
            //This shouldn't happen anymore, since GraphSolver calls the pump-specific (i.e. not override) version.
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * modifier.flowPercent;
            baseData.pressure = calculateInletPressure(modifier);
            inletPressure = baseData.pressure;
            return source.getSourcePossibleValues(baseData, this, 1.0, 1.0);         //Always ask 100% of whatever desired flow we have. Will send smaller percent upon solving whole solution.
        }

        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            if (caller == null)
            {
                //This shouldn't happen anymore, since GraphSolver calls the pump-specific (i.e. not override) version.
                baseData.flowPusher = this;
                baseData.desiredFlowVolume = pumpingPercent * mcrRating * flowPercent;
                baseData.pressure = mcrPressure * pressurePercent;
                outletPressure = baseData.pressure;
                return sink.getSinkPossibleValues(baseData, this, 1.0, 1.0);             //Always ask 100% of whatever desired flow we have
            }
            else
            {
                return null;                //TODO: Handle this case when pumps are in series
            }
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            if (caller == null)
            {
                //This shouldn't happen anymore, since GraphSolver calls the pump-specific (i.e. not override) version.
                baseData.flowPusher = this;
                baseData.desiredFlowVolume = pumpingPercent * mcrRating * flowPercent;
                baseData.pressure = pumpingPercent * -0.2;
                inletPressure = baseData.pressure;
                return source.getSourcePossibleValues(baseData, this, 1.0, 1.0);         //Always ask 100% of whatever desired flow we have. Will send smaller percent upon solving whole solution.
            }
            else
            {
                return null;                //TODO: Handle this case when pumps are in series
            }
        }

        public override void setSource(FlowComponent source)
        {
            this.source = source;
        }

        public void applySolution(FlowCalculationData baseData, FlowPusherModifier modifier)
        {
            setSourceValues(baseData, null, pumpingPercent * mcrRating * modifier.flowPercent);
            setSinkValues(baseData, null, pumpingPercent * mcrRating * modifier.flowPercent);
        }
        
        public override void setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = flowVolume;

            finalFlow = flowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC
            source.setSourceValues(baseData, this, flowVolume);
        }

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = flowVolume;

            finalFlow = flowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC
            sink.setSinkValues(baseData, this, flowVolume);
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
