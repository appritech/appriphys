using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class Junction : FlowComponent
    {
        public Junction(String name, String[] sinkNames, String[] sourceNames, double[] normalWeights, double[] maxWeights) : base(name)
        {
            this.sinkNames = sinkNames;
            this.sourceNames = sourceNames;
            this.normalWeights = normalWeights;
            this.maxWeights = maxWeights;

            sinks = new FlowComponent[sinkNames.Length];
            sources = new FlowComponent[sourceNames.Length];
        }
        //This is for making a combiner, who doesn't use the weights... at least not for now, but a 3 way valve combiner might need to...
        public Junction(String name, String[] sinkNames, String[] sourceNames) : this(name, sinkNames, sourceNames, null, null)
        {
        }

        String[] sinkNames;
        String[] sourceNames;
        double[] normalWeights;
        double[] maxWeights;
        FlowComponent[] sinks;
        FlowComponent[] sources;
        private Dictionary<String, double> finalFlows = new Dictionary<String, double>();
        private Dictionary<String, double[]> flowPercentageSolutions = new Dictionary<String, double[]>();

        public override double getFlow()
        {
            double flow = 0.0;
            foreach (double iter in finalFlows.Values)
            {
                flow += iter;
            }
            return flow;
        }

        public override void connectSelf(Dictionary<string, FlowComponent> components)
        {
            for (int i = 0; i < sinkNames.Length; i++)
            {
                FlowComponent sink = components[sinkNames[i]];
                sink.setSource(this);
                sinks[i] = sink;
            }
            for (int i = 0; i < sourceNames.Length; i++)
            {
                FlowComponent source = components[sourceNames[i]];
                sources[i] = source;
            }
            //TODO: Add code to validate the configuration, so that we are either 1 to many or many to one. Never many to many (and never 0 anywhere)
        }

        public override void setSource(FlowComponent source)
        {
            //We wire ourselves up completely (inputs and outputs), so don't need to do anything here.
        }

        private FlowResponseData calculateSplittingFunctionality(FlowCalculationData baseData, double curPercent, FlowComponent[] nodes, bool isSink)
        {
            FlowResponseData[] responses = new FlowResponseData[nodes.Length];
            double[] maxFlowPercent = new double[nodes.Length];
            double[] preferredFlowPercent = new double[nodes.Length];
            double currentSum = 0.0;
            double percentToReturn = 0.0;
            double sumOfMaxFlow = 0;
            bool foundNull = false;
            for (int attempt = 0; attempt < 2; attempt++)
            {
                //Most of the time, we will need to ask twice, because some will be null the first time.
                foundNull = false;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if(isSink)
                        responses[i] = nodes[i].getSinkPossibleFlow(baseData, this, Math.Min(curPercent, maxWeights[i]));
                    else
                        responses[i] = nodes[i].getSourcePossibleFlow(baseData, this, Math.Min(curPercent, maxWeights[i]));

                    if (responses[i] == null)
                        foundNull = true;
                }

                if (!foundNull)
                    break;
            }
            if (foundNull)
            {
                return null;                //We weren't able to get everything, and this is probably because there is another splitter up stream that will retry us again.
            }

            if (!flowPercentageSolutions.ContainsKey(baseData.flowPusher.name + (isSink ? "_sink" : "_source")))
                flowPercentageSolutions.Add(baseData.flowPusher.name + (isSink ? "_sink" : "_source"), new double[nodes.Length]);

            //If we get here, then we have all the info we should need to solve ourselves.
            for (int i = 0; i < nodes.Length; i++)
            {
                maxFlowPercent[i] = Math.Min(maxWeights[i], responses[i].flowPercent);
                preferredFlowPercent[i] = Math.Min(normalWeights[i], maxFlowPercent[i]) * curPercent;
                currentSum += preferredFlowPercent[i];
                percentToReturn += preferredFlowPercent[i];
                sumOfMaxFlow += maxFlowPercent[i];
            }

            if (currentSum < curPercent && sumOfMaxFlow > 0.0)                //sum > 0.0 helps us avoid a divide by 0 below. Also, if we can't push anything, then 0 is the right answer
            {
                percentToReturn = 0;

                for (int i = 0; i < nodes.Length; i++)
                {
                    double trueFlowPercent = curPercent * (maxFlowPercent[i] / sumOfMaxFlow);

                    trueFlowPercent = Math.Min(trueFlowPercent, maxWeights[i]);            //Probably don't need this one, since the one right below is better.
                    trueFlowPercent = Math.Min(trueFlowPercent, maxFlowPercent[i]);
                    //trueFlows.set(i, trueFlowPercent);
                    flowPercentageSolutions[baseData.flowPusher.name + (isSink ? "_sink" : "_source")][i] = trueFlowPercent;
                    percentToReturn += trueFlowPercent;
                }
            }
            else
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    double trueFlowPercent = preferredFlowPercent[i];           //It all works out with preferred values.
                    flowPercentageSolutions[baseData.flowPusher.name + (isSink ? "_sink" : "_source")][i] = trueFlowPercent;
                }
            }

            //Need to normalize the flowPercenageSolutions, so that they sum to 1.0, so that when we set values, we are spliting 100% of the smaller portion that is coming down to us (and don't double-limit things).
            for (int i = 0; i < nodes.Length; i++)
                flowPercentageSolutions[baseData.flowPusher.name + (isSink ? "_sink" : "_source")][i] /= percentToReturn;

            FlowResponseData ret = new FlowResponseData();
            ret.flowPercent = percentToReturn;
            ret.flowVolume = percentToReturn * baseData.desiredFlowVolume;
            return ret;
        }

        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            return calculateSplittingFunctionality(baseData, curPercent, sinks, true);
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            return calculateSplittingFunctionality(baseData, curPercent, sources, false);
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            //NOTE: setSinkFlow and setSourceFlow need to be kept in synch. If it gets any more complicated, then we should refactor and share methods
            for(int i = 0; i < sinks.Length; i++)
            {
                sinks[i].setSinkFlow(baseData, this, flowPercentageSolutions[baseData.flowPusher.name + "_sink"][i] * curPercent);
            }
            finalFlows.Add(baseData.flowPusher.name + "_" + caller.name + "_sink", baseData.desiredFlowVolume * curPercent);
        }

        public override void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].setSourceFlow(baseData, this, flowPercentageSolutions[baseData.flowPusher.name + "_source"][i] * curPercent);
            }
            finalFlows.Add(baseData.flowPusher.name + "_" + caller.name + "_source", -1 * baseData.desiredFlowVolume * curPercent);
        }
    }
}
