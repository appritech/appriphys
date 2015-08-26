using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class FlowDriver : FlowComponent
    {

        public FlowDriver(String name, double mcrRating, double mcrPressure, String sinkName) : base(name)
        {
            this.mcrRating = mcrRating;
            this.mcrPressure = mcrPressure;
            this.sinkName = sinkName;
        }

        public double mcrRating;
        protected double mcrPressure;
        protected String sinkName;
        protected FlowComponent source;
        protected FlowComponent sink;
        protected double pumpingPercent = 1.0f;

        protected Boolean solutionApplied = false;

        public override void resetState()
        {
            base.resetState();
            solutionApplied = false;
        }

        private void setPumpingPercent(double pumpingPercent)
        {
            this.pumpingPercent = pumpingPercent;
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        protected double calculateOutletPressure(FlowDriverModifier modifier)
        {
            double ret = mcrPressure * pumpingPercent * modifier.minSourceFlowPercent;           //The source is the main thing that can drop the pressure
            if (modifier.minSourceFlowPercent > modifier.minSinkFlowPercent && modifier.minSourceFlowPercent > 0.0)
            {
                //This is functionality more typical of a centrifugal pump.
                //If the sink is more clogged than the source, then we can add back some because of back pressure.
                ret *= (1.0 + 0.20 * (1.0 - modifier.minSinkFlowPercent / modifier.minSourceFlowPercent));                   //Allow up to 20% higher pressure if the output is clogged
            }
            return ret;
        }

        protected double calculateInletPressure(FlowDriverModifier modifier)
        {
            double ret = -0.2 * pumpingPercent;                           //By default, it will be about -0.2 bar when running at 100% normally
            ret *= (1 + (2.0 * (1.0 - modifier.minSourceFlowPercent)));           //If the source is blocked, it can increase by up to 3x
            return ret;
        }

        public virtual FlowResponseData getFlowDriverSinkPossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            baseData.flowDriver = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * modifier.flowPercent;
            baseData.pressure = calculateOutletPressure(modifier);
            outletPressure = baseData.pressure;
            return sink.getSinkPossibleValues(baseData, this, 1.0, 1.0);             //Always ask 100% of whatever desired flow we have
        }

        public virtual FlowResponseData getFlowDriverSourcePossibleValues(FlowCalculationData baseData, FlowDriverModifier modifier)
        {
            //This shouldn't happen anymore, since GraphSolver calls the pump-specific (i.e. not override) version.
            baseData.flowDriver = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * modifier.flowPercent;
            baseData.pressure = calculateInletPressure(modifier);
            inletPressure = baseData.pressure;
            
            //We only do last response on the source side for pumps (all other components only have one side for this stuff...
            return source.getSourcePossibleValues(baseData, this, 1.0, 1.0);         //Always ask 100% of whatever desired flow we have. Will send smaller percent upon solving whole solution.
        }

        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            if (caller == null)
            {
                //This shouldn't happen anymore, since GraphSolver calls the pump-specific (i.e. not override) version.
                baseData.flowDriver = this;
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
                baseData.flowDriver = this;
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

        public bool applySolution(FlowCalculationData baseData, FlowDriverModifier modifier, bool lastTime)
        {
            if (solutionApplied)
                return true;                     //If we have already successfully sent all our data, then we shouldn't do it again...

            SettingResponseData sourceResponse = setSourceValues(baseData, null, pumpingPercent * mcrRating * modifier.flowPercent, lastTime);
            if (sourceResponse != null)
            {
                baseData.applySourceResponse(sourceResponse);
                setSinkValues(baseData, null, pumpingPercent * mcrRating * modifier.flowPercent, lastTime);
                solutionApplied = true;
            }

            return solutionApplied;
        }
        
        public override SettingResponseData setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            baseData.flowDriver = this;
            baseData.desiredFlowVolume = flowVolume;

            finalFlow = flowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC

            SettingResponseData ret = source.setSourceValues(baseData, this, flowVolume, lastTime);
            if (ret != null)
            {
                currentFluidTypeMap = ret.fluidTypeMap;
                inletTemperature = ret.temperature;
                outletTemperature = ret.temperature;
            }
            return ret;
        }

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            baseData.flowDriver = this;
            baseData.desiredFlowVolume = flowVolume;

            finalFlow = flowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC
            sink.setSinkValues(baseData, this, flowVolume, lastTime);
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
