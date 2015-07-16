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
        bool hasMultipleSinks;
        private Dictionary<String, double> finalFlows = new Dictionary<String, double>();
        private Dictionary<String, double[]> flowPercentageSolutions = new Dictionary<String, double[]>();
        private Dictionary<String, int> indexByName = new Dictionary<String, int>();

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
                if (sinks.Length > 1)
                    indexByName.Add(sink.name, i);
            }
            for (int i = 0; i < sourceNames.Length; i++)
            {
                FlowComponent source = components[sourceNames[i]];
                sources[i] = source;
                if (sources.Length > 1)
                    indexByName.Add(source.name, i);
            }
            //TODO: Add code to validate the configuration, so that we are either 1 to many or many to one. Never many to many (and never 0 anywhere)
            hasMultipleSinks = sinks.Length > 1;
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
                    double trueFlowPercent;
                    if (sumOfMaxFlow == 0.0)
                        trueFlowPercent = 0.0;
                    else
                        trueFlowPercent = curPercent * (maxFlowPercent[i] / sumOfMaxFlow);

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
            if (percentToReturn != 0.0)
            {
                for (int i = 0; i < nodes.Length; i++)
                    flowPercentageSolutions[baseData.flowPusher.name + (isSink ? "_sink" : "_source")][i] /= percentToReturn;
            }

            FlowResponseData ret = new FlowResponseData();
            ret.flowPercent = percentToReturn;
            ret.flowVolume = percentToReturn * baseData.desiredFlowVolume;
            return ret;
        }

        private void setFlowValues(FlowCalculationData baseData, FlowComponent caller, double curPercent, FlowComponent[] nodes, bool isSink)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (isSink)
                    nodes[i].setSinkFlow(baseData, this, flowPercentageSolutions[baseData.flowPusher.name + "_sink"][i] * curPercent);
                else
                    nodes[i].setSourceFlow(baseData, this, flowPercentageSolutions[baseData.flowPusher.name + "_source"][i] * curPercent);
            }
            if (isSink)
                finalFlows[baseData.flowPusher.name + "_" + caller.name + "_sink"] = baseData.desiredFlowVolume * curPercent;
            else
                finalFlows[baseData.flowPusher.name + "_" + caller.name + "_source"] = -1 * baseData.desiredFlowVolume * curPercent;
        }

        private void setCombiningFlowValues(FlowCalculationData baseData, FlowComponent caller, double curPercent, FlowComponent[] nodes, bool isSink)
        {
            //Here, we need to wait until we get all of our values set, before passing the single value down.
            if (!baseData.combinerMap.ContainsKey(baseData.flowPusher.name + "_" + name))
            {
                double[] toAdd = new double[indexByName.Count];
                for (int i = 0; i < indexByName.Count; i++)
                    toAdd[i] = -1.0f;                                   //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                baseData.combinerMap.Add(baseData.flowPusher.name + "_" + name, toAdd);
            }

            double[] percentMap = baseData.combinerMap[baseData.flowPusher.name + "_" + name];
            int index = indexByName[caller.name];
            percentMap[index] = curPercent;

            //Always set this data, it is for internal use
            if (isSink)
                finalFlows[baseData.flowPusher.name + "_" + caller.name + "_sink"] = baseData.desiredFlowVolume * curPercent;
            else
                finalFlows[baseData.flowPusher.name + "_" + caller.name + "_source"] = -1 * baseData.desiredFlowVolume * curPercent;

            double percentSum = 0.0;
            for (int i = 0; i < percentMap.Length; i++)
            {
                if (percentMap[i] < 0.0)
                    return;                //We haven't seen all of the inputs to combine them... 
                else
                    percentSum += percentMap[i];
            }

            //Only pass down data if we have all of the data (return above prevents incomplete data)
            for (int i = 0; i < nodes.Length; i++)
            {
                if (isSink)
                    nodes[i].setSinkFlow(baseData, this, percentSum);
                else
                    nodes[i].setSourceFlow(baseData, this, percentSum);
            }
        }

        private FlowResponseData calculateCombiningFunctionality(FlowCalculationData baseData, FlowComponent caller, double curPercent, FlowComponent[] nodes, bool isSink)
        {
            if(!baseData.combinerMap.ContainsKey(baseData.flowPusher.name + "_" + name))
            {
                double[] toAdd = new double[indexByName.Count];
                for (int i = 0; i < indexByName.Count; i++)
                    toAdd[i] = -1.0f;                                   //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                baseData.combinerMap.Add(baseData.flowPusher.name + "_" + name, toAdd);
            }

            double[] percentMap = baseData.combinerMap[baseData.flowPusher.name + "_" + name];
            int index = indexByName[caller.name];
            percentMap[index] = curPercent;

            double percentSum = 0.0;
            for(int i = 0; i < percentMap.Length; i++)
            {
                if (percentMap[i] < 0.0)
                    return null;                //We haven't seen all of the inputs to combine them... thus, return null until we do see them all.
                else
                    percentSum += percentMap[i];
            }
            if (percentSum > 1.0)
                percentSum = 1.0;
            FlowResponseData ret;
            if (isSink)
                ret = sinks[0].getSinkPossibleFlow(baseData, this, percentSum);
            else
                ret = sources[0].getSourcePossibleFlow(baseData, this, percentSum);
            if (percentSum != 0.0)
            {
                ret.flowPercent *= (percentMap[index] / percentSum);             //The divide here is to normalize it.
                ret.flowVolume *= (percentMap[index] / percentSum);
            }

            if (!flowPercentageSolutions.ContainsKey(baseData.flowPusher.name + (isSink ? "_sink" : "_source")))
                flowPercentageSolutions.Add(baseData.flowPusher.name + (isSink ? "_sink" : "_source"), new double[1]);
            flowPercentageSolutions[baseData.flowPusher.name + (isSink ? "_sink" : "_source")][0] = ret.flowPercent;

            return ret;
        }

        public override FlowResponseData getSinkPossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            if (hasMultipleSinks)
                return calculateSplittingFunctionality(baseData, curPercent, sinks, true);
            else
                return calculateCombiningFunctionality(baseData, caller, curPercent, sources, true);
        }

        public override FlowResponseData getSourcePossibleFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            if (!hasMultipleSinks)
                return calculateSplittingFunctionality(baseData, curPercent, sources, false);
            else
                return calculateCombiningFunctionality(baseData, caller, curPercent, sinks, false);
        }

        public override void setSinkFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            if (hasMultipleSinks)
                setFlowValues(baseData, caller, curPercent, sinks, true);
            else
                setCombiningFlowValues(baseData, caller, curPercent, sinks, true);
        }

        public override void setSourceFlow(FlowCalculationData baseData, FlowComponent caller, double curPercent)
        {
            if (!hasMultipleSinks)
                setFlowValues(baseData, caller, curPercent, sources, false);
            else
                setCombiningFlowValues(baseData, caller, curPercent, sources, false);
        }
    }
}
