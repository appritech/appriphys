using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * flowPercent;
            return sink.getSinkPossibleValues(baseData, this, 1.0);             //Always ask 100% of whatever desired flow we have
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * flowPercent;
            return source.getSourcePossibleValues(baseData, this, 1.0);         //Always ask 100% of whatever desired flow we have. Will send smaller percent upon solving whole solution.
        }

        public override void setSource(FlowComponent source)
        {
            this.source = source;
        }

        public void applySolution(FlowCalculationData baseData, double flowPercent)
        {
            setSourceValues(baseData, null, pumpingPercent * mcrRating * flowPercent);
            setSinkValues(baseData, null, pumpingPercent * mcrRating * flowPercent);
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
