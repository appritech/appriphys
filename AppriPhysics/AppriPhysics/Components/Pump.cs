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
        private double lastSolutionFlow = 0.0f;

        private void setPumpingPercent(double pumpingPercent)
        {
            this.pumpingPercent = pumpingPercent;
        }

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            sink = components[sinkName];
            sink.setSource(this);
        }

        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * curPercent;
            return sink.getSinkPossibleFlow(baseData, this, 1.0);             //Always ask 100% of whatever desired flow we have
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * curPercent;
            return source.getSourcePossibleFlow(baseData, this, 1.0);         //Always ask 100% of whatever desired flow we have. Will send smaller percent upon solving whole solution.
        }

        public override void setSource(FlowComponent source)
        {
            this.source = source;
        }

        public void applySolution(FlowCalculationData baseData, double curPercent)
        {
            setSourceFlow(baseData, null, curPercent);
            setSinkFlow(baseData, null, curPercent);
        }

        public override double getFlow()
        {
            return lastSolutionFlow;
        }
        
        public override void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * curPercent;

            lastSolutionFlow = baseData.desiredFlowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC
            source.setSourceFlow(baseData, this, 1.0);
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            baseData.flowPusher = this;
            baseData.desiredFlowVolume = pumpingPercent * mcrRating * curPercent;

            lastSolutionFlow = baseData.desiredFlowVolume;              //Stash this value for later. The last method call will have the final solution's flow valueCC
            sink.setSinkFlow(baseData, this, 1.0);
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
