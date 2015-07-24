using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class Junction : FlowComponent
    {
        public Junction(String name, String[] sinkNames, String[] sourceNames, String combinerName, double[] normalWeights, double[] maxWeights) : base(name)
        {
            this.sinkNames = sinkNames;
            this.sourceNames = sourceNames;
            this.normalWeights = normalWeights;
            this.maxWeights = maxWeights;

            this.linkedCombinerName = combinerName;

            sinks = new FlowComponent[sinkNames.Length];
            sources = new FlowComponent[sourceNames.Length];
        }
        //This is for making a combiner, who doesn't use the weights... at least not for now, but a 3 way valve combiner might need to...
        public Junction(String name, String[] sinkNames, String[] sourceNames) : this(name, sinkNames, sourceNames, "", null, null)
        {
        }

        String[] sinkNames;
        String[] sourceNames;
        double[] normalWeights;
        double[] maxWeights;
        FlowComponent[] sinks;
        FlowComponent[] sources;
        String linkedCombinerName;
        Junction linkedCombiner = null;
        Dictionary<String, FlowResponseData> combinerDownstreamValues = new Dictionary<string, FlowResponseData>();
        public Dictionary<String, double[]> combinerMap = new Dictionary<String, double[]>();
        bool hasMultipleSinks;
        private Dictionary<String, double[]> flowPercentageSolutions = new Dictionary<String, double[]>();
        private Dictionary<String, int> indexByName = new Dictionary<String, int>();
        private Dictionary<String, bool[]> indexesUsedByPump = new Dictionary<String, bool[]>();

        double[] volumeMap;

        public override void connectSelf(Dictionary<string, FlowComponent> components)
        {
            for (int i = 0; i < sinkNames.Length; i++)
            {
                FlowComponent sink = components[sinkNames[i]];
                sink.setSource(this);
                sinks[i] = sink;
                if (sinks.Length > 1)
                {
                    indexByName[sink.name] = i;
                    volumeMap = new double[sinks.Length];
                }
            }
            for (int i = 0; i < sourceNames.Length; i++)
            {
                FlowComponent source = components[sourceNames[i]];
                sources[i] = source;
                if (sources.Length > 1)
                {
                    indexByName[source.name] = i;
                    volumeMap = new double[sources.Length];
                }
            }

            if(!String.IsNullOrEmpty(linkedCombinerName))
            {
                linkedCombiner = (Junction)components[linkedCombinerName];
            }

            //TODO: Add code to validate the configuration, so that we are either 'one to many' or 'many to one'. Never many to many (and never 0 anywhere)
            hasMultipleSinks = sinks.Length > 1;
        }

        public override void setSource(FlowComponent source)
        {
            //We wire ourselves up completely (inputs and outputs), so don't need to do anything here.
        }

        private FlowResponseData calculateSplittingFunctionality(FlowCalculationData baseData, double flowPercent, FlowComponent[] nodes, bool isSink)
        {
            FlowResponseData[] responses = new FlowResponseData[nodes.Length];
            bool foundNull = false;
            for (int attempt = 0; attempt < 2; attempt++)
            {
                //Most of the time, we will need to ask twice, because some will be null the first time.
                foundNull = false;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (responses[i] == null)
                    {
                        if (isSink)
                            responses[i] = nodes[i].getSinkPossibleValues(baseData, this, Math.Min(flowPercent, maxWeights[i]));
                        else
                            responses[i] = nodes[i].getSourcePossibleValues(baseData, this, Math.Min(flowPercent, maxWeights[i]));
                    }

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

            if (!flowPercentageSolutions.ContainsKey(baseData.flowPusher.name))
                flowPercentageSolutions[baseData.flowPusher.name] = new double[nodes.Length];
            
            //If we get here, then we have all the info we should need to solve ourselves.

            double desiredPercent = flowPercent;

            double[] maxFlowPercent = new double[nodes.Length];
            double[] preferredFlowPercent = new double[nodes.Length];
            double percentToReturn = 0.0;
            double sumOfMaxFlow = 0;
            double currentSum = 0.0;

            if (linkedCombiner != null)
            {
                desiredPercent = Math.Min(responses[0].lastCombinerOrTankPercent, flowPercent);            //All are equal... grab first. This is the combiner's maxFlow

                double[] flowPercentToCombiner;
                //NOTE: Both solutions with both divisors gave the same answer, and I found the bug elsewhere...
                //TODO: Determing what divisor we actually want to use.
                if (linkedCombiner.combinerMap.ContainsKey(baseData.flowPusher.name))
                    flowPercentToCombiner = linkedCombiner.combinerMap[baseData.flowPusher.name];
                else
                {
                    flowPercentToCombiner = new double[nodes.Length];
                    for (int i = 0; i < nodes.Length; i++)
                        flowPercentToCombiner[i] = responses[i].flowPercent;              //This was my original solution, but the map is better.
                }

                double tempSum = 0.0;
                for (int i = 0; i < nodes.Length; i++)
                {
                    maxFlowPercent[i] = Math.Min(maxWeights[i], flowPercentToCombiner[i]);
                    preferredFlowPercent[i] = Math.Min(normalWeights[i], maxFlowPercent[i]) * desiredPercent;
                    tempSum += preferredFlowPercent[i];
                }

                preferredFlowPercent = fixPercents(preferredFlowPercent, maxFlowPercent, desiredPercent);
                //Normalize preferredFlowPercent (since we know the final flow as desiredPercent, we can safely do so in this case)
                for (int i = 0; i < nodes.Length; i++)
                {
                    currentSum += preferredFlowPercent[i];
                    percentToReturn += preferredFlowPercent[i];
                    sumOfMaxFlow += maxFlowPercent[i];
                }
            }
            else
            {
                desiredPercent = flowPercent;
                
                for (int i = 0; i < nodes.Length; i++)
                {
                    maxFlowPercent[i] = Math.Min(maxWeights[i], responses[i].flowPercent);
                    preferredFlowPercent[i] = Math.Min(normalWeights[i], maxFlowPercent[i]) * desiredPercent;
                    currentSum += preferredFlowPercent[i];
                    percentToReturn += preferredFlowPercent[i];
                    sumOfMaxFlow += maxFlowPercent[i];
                }
            }
            
            if (currentSum < desiredPercent && sumOfMaxFlow > 0.0)                //sum > 0.0 helps us avoid a divide by 0 below. Also, if we can't push anything, then 0 is the right answer
            {
                percentToReturn = 0;
                for (int i = 0; i < nodes.Length; i++)
                {
                    double trueFlowPercent;
                    if (sumOfMaxFlow == 0.0)
                        trueFlowPercent = 0.0;
                    else
                        trueFlowPercent = desiredPercent * (maxFlowPercent[i] / sumOfMaxFlow);

                    trueFlowPercent = Math.Min(trueFlowPercent, maxWeights[i]);            //Probably don't need this one, since the one right below is better.
                    trueFlowPercent = Math.Min(trueFlowPercent, maxFlowPercent[i]);
                    flowPercentageSolutions[baseData.flowPusher.name][i] = trueFlowPercent;
                    percentToReturn += trueFlowPercent;
                }
            }
            else
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    double trueFlowPercent = preferredFlowPercent[i];           //It all works out with preferred values.
                    flowPercentageSolutions[baseData.flowPusher.name][i] = trueFlowPercent;
                }
            }

            //Need to normalize the flowPercenageSolutions, so that they sum to 1.0, so that when we set values, we are spliting 100 % of the smaller portion that is coming down to us(and don't double-limit things).
            if (percentToReturn != 0.0)
            {
                for (int i = 0; i < nodes.Length; i++)
                    flowPercentageSolutions[baseData.flowPusher.name][i] /= percentToReturn;
            }
            
            FlowResponseData ret = new FlowResponseData();
            ret.flowPercent = percentToReturn;
            ret.flowVolume = percentToReturn * baseData.desiredFlowVolume;
            return ret;
        }

        private double[] fixPercents(double[] preferredFlowPercent, double[] maxFlowPercent, double desiredPercent)
        {
            for(int attemp = 0; attemp < preferredFlowPercent.Length; attemp++)
            {
                double sum = 0.0;
                double goal = desiredPercent;
                for(int i = 0; i < preferredFlowPercent.Length; i++)
                {
                    if(preferredFlowPercent[i] == maxFlowPercent[i])
                    {
                        //This one is at max, so doesn't count
                        goal -= preferredFlowPercent[i];
                    }
                    else
                    {
                        sum += preferredFlowPercent[i];
                    }
                }
                if (sum >= desiredPercent - 0.0000000001)           //Make sure rounding is ok
                    return preferredFlowPercent;
                if (sum == 0.0)
                    return preferredFlowPercent;                    //All items are maxed, so we have to be finished.
                for(int i = 0; i < preferredFlowPercent.Length; i++)
                {
                    if(preferredFlowPercent[i] < maxFlowPercent[i])
                    {
                        //Crank this one up a bit
                        double newValue = preferredFlowPercent[i] * goal / sum;       //If all of them can take it, one pass will get us there.
                        preferredFlowPercent[i] = Math.Min(newValue, maxFlowPercent[i]);
                    }
                    else
                    {
                        preferredFlowPercent[i] = maxFlowPercent[i];
                    }
                }
            }

            return preferredFlowPercent;
        }

        private void setFlowValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, FlowComponent[] nodes, bool isSink)
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                if (isSink)
                    nodes[i].setSinkValues(baseData, this, flowPercentageSolutions[baseData.flowPusher.name][i] * flowVolume);
                else
                    nodes[i].setSourceValues(baseData, this, flowPercentageSolutions[baseData.flowPusher.name][i] * flowVolume);
            }
            if (isSink)
                finalFlow = flowVolume;
            else
                finalFlow = flowVolume;
        }

        public override void resetState()
        {
            base.resetState();
            for(int i = 0; i < volumeMap.Length; i++)
            {
                volumeMap[i] = -1.0;
            }
            combinerDownstreamValues.Clear();
            foreach(KeyValuePair<String, double[]> iter in combinerMap)
            {
                for(int i = 0; i < iter.Value.Length; i++)
                {
                    if (!indexesUsedByPump.ContainsKey(iter.Key) || indexesUsedByPump[iter.Key][i])
                        iter.Value[i] = -1.0;                            //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                    else
                        iter.Value[i] = 0.0;                             //Initialize to 0 if this pump will never get this number
                }
            }
        }

        private void setCombiningFlowValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, FlowComponent[] nodes, bool isSink)
        {
            int index = indexByName[caller.name];
            volumeMap[index] = flowVolume;   

            double volumeSum = 0.0;
            for (int i = 0; i < volumeMap.Length; i++)
            {
                if (volumeMap[i] < 0.0)
                    return;                //We haven't seen all of the inputs to combine them... 
                else
                    volumeSum += volumeMap[i];
            }

            finalFlow = volumeSum;

            //Only pass down data if we have all of the data (return above prevents incomplete data)
            for (int i = 0; i < nodes.Length; i++)
            {
                if (isSink)
                    nodes[i].setSinkValues(baseData, this, volumeSum);
                else
                    nodes[i].setSourceValues(baseData, this, volumeSum);
            }
        }

        private FlowResponseData calculateCombiningFunctionality(FlowCalculationData baseData, FlowComponent caller, double flowPercent, FlowComponent[] nodes, bool isSink)
        {
            if(!combinerMap.ContainsKey(baseData.flowPusher.name))
            {
                double[] toAdd = new double[indexByName.Count];
                for (int i = 0; i < indexByName.Count; i++)
                {
                    if (!indexesUsedByPump.ContainsKey(baseData.flowPusher.name) || indexesUsedByPump[baseData.flowPusher.name][i])
                        toAdd[i] = -1.0;                            //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                    else
                        toAdd[i] = 0.0;                             //Initialize to 0 if this pump will never get this number
                }
                combinerMap[baseData.flowPusher.name] = toAdd;
            }

            double[] percentMap = combinerMap[baseData.flowPusher.name];
            int index = indexByName[caller.name];
            percentMap[index] = flowPercent;

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
            
            if (!combinerDownstreamValues.ContainsKey(baseData.flowPusher.name))
            {
                if (isSink)
                    combinerDownstreamValues[baseData.flowPusher.name] = sinks[0].getSinkPossibleValues(baseData, this, percentSum);
                else
                    combinerDownstreamValues[baseData.flowPusher.name] = sources[0].getSourcePossibleValues(baseData, this, percentSum);
            }

            FlowResponseData ret = combinerDownstreamValues[baseData.flowPusher.name].clone();
            ret.setLastCombinerOrTank(this, ret.flowPercent);

            if (percentSum != 0.0)
            {
                ret.flowPercent *= (percentMap[index] / percentSum);             //The divide here is to normalize it.
                ret.flowVolume *= (percentMap[index] / percentSum);
            }

            if (!flowPercentageSolutions.ContainsKey(baseData.flowPusher.name))
                flowPercentageSolutions[baseData.flowPusher.name] = new double[1];
            flowPercentageSolutions[baseData.flowPusher.name][0] = ret.flowPercent;
            
            return ret;
        }

        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            if (hasMultipleSinks)
                return calculateSplittingFunctionality(baseData, flowPercent, sinks, true);
            else
                return calculateCombiningFunctionality(baseData, caller, flowPercent, sources, true);
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent)
        {
            if (!hasMultipleSinks)
                return calculateSplittingFunctionality(baseData, flowPercent, sources, false);
            else
                return calculateCombiningFunctionality(baseData, caller, flowPercent, sinks, false);
        }

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            if (hasMultipleSinks)
                setFlowValues(baseData, caller, flowVolume, sinks, true);
            else
                setCombiningFlowValues(baseData, caller, flowVolume, sinks, true);
        }

        public override void setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume)
        {
            if (!hasMultipleSinks)
                setFlowValues(baseData, caller, flowVolume, sources, false);
            else
                setCombiningFlowValues(baseData, caller, flowVolume, sources, false);
        }

        public override void exploreSinkGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            for (int i = 0; i < sinks.Length; i++)
            {
                sinks[i].exploreSinkGraph(baseData, this);
            }
            if (!hasMultipleSinks)           //Do the following if we are combining, so we know which combining imputs actually apply to given pump
            {
                if (!indexesUsedByPump.ContainsKey(baseData.flowPusher.name))
                    indexesUsedByPump[baseData.flowPusher.name] = new bool[sources.Length];
                indexesUsedByPump[baseData.flowPusher.name][indexByName[caller.name]] = true;
            }
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            for (int i = 0; i < sources.Length; i++)
            {
                sources[i].exploreSourceGraph(baseData, this);
            }
            if (hasMultipleSinks)           //Do the following if we are combining, so we know which combining imputs actually apply to given pump
            {
                if (!indexesUsedByPump.ContainsKey(baseData.flowPusher.name))
                    indexesUsedByPump[baseData.flowPusher.name] = new bool[sinks.Length];
                indexesUsedByPump[baseData.flowPusher.name][indexByName[caller.name]] = true;
            }
        }
    }
}
