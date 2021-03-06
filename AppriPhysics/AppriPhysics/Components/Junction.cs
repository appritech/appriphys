﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class Junction : FlowComponent
    {
        public Junction(String name, String[] deliveryNames, String[] sourceNames, String combinerName, double[] normalWeights, double[] maxWeights) : base(name)
        {
            this.deliveryNames = deliveryNames;
            this.sourceNames = sourceNames;
            this.normalWeights = normalWeights;
            this.maxWeights = maxWeights;

            this.linkedCombinerName = combinerName;

            deliveryComponents = new FlowComponent[this.deliveryNames.Length];
            sourceComponents = new FlowComponent[sourceNames.Length];
        }
        //This is for making a combiner, who doesn't use the weights... at least not for now, but a 3 way valve combiner might need to...
        public Junction(String name, String[] deliveryNames, String[] sourceNames) : this(name, deliveryNames, sourceNames, "", null, null)
        {
        }

        String[] deliveryNames;
        String[] sourceNames;
        double[] normalWeights;
        double[] maxWeights;
        FlowComponent[] deliveryComponents;
        FlowComponent[] sourceComponents;
        bool hasMultipleDeliveryComponents;

        String linkedCombinerName;
        Junction linkedCombiner = null;
        double lastCombinePercent;
        
        Dictionary<String, FlowResponseData> combinerDownstreamValues = new Dictionary<string, FlowResponseData>();         //This is a helpful cache, but not needed (Could ask multiple times)
        public Dictionary<String, double[]> combinerMap = new Dictionary<String, double[]>();
        private Dictionary<String, double[]> flowPercentageSolutions = new Dictionary<String, double[]>();

        private Dictionary<String, int> indexByName = new Dictionary<String, int>();
        private Dictionary<String, bool[]> indexesUsedByPump = new Dictionary<String, bool[]>();
        SettingResponseData setterDownstreamValue = null;

        double[] setCombiningVolumeMap;

        public override void resetState()
        {
            base.resetState();
            for (int i = 0; i < setCombiningVolumeMap.Length; i++)
            {
                setCombiningVolumeMap[i] = -1.0;
            }
            combinerDownstreamValues.Clear();
            foreach (KeyValuePair<String, double[]> iter in combinerMap)
            {
                for (int i = 0; i < iter.Value.Length; i++)
                {
                    if (!indexesUsedByPump.ContainsKey(iter.Key) || indexesUsedByPump[iter.Key][i])
                        iter.Value[i] = -1.0;                            //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                    else
                        iter.Value[i] = 0.0;                             //Initialize to 0 if this pump will never get this number
                }
            }
            setterDownstreamValue = null;
        }

        public override void connectSelf(Dictionary<string, FlowComponent> components)
        {
            for (int i = 0; i < deliveryNames.Length; i++)
            {
                FlowComponent deliveryComponent = components[deliveryNames[i]];
                deliveryComponent.setSource(this);
                deliveryComponents[i] = deliveryComponent;
                if (deliveryComponents.Length > 1)
                {
                    indexByName[deliveryComponent.name] = i;
                    setCombiningVolumeMap = new double[deliveryComponents.Length];
                }
            }
            for (int i = 0; i < sourceNames.Length; i++)
            {
                FlowComponent source = components[sourceNames[i]];
                sourceComponents[i] = source;
                if (sourceComponents.Length > 1)
                {
                    indexByName[source.name] = i;
                    setCombiningVolumeMap = new double[sourceComponents.Length];
                }
            }

            if(!String.IsNullOrEmpty(linkedCombinerName))
            {
                linkedCombiner = (Junction)components[linkedCombinerName];
            }

            //TODO: Add code to validate the configuration, so that we are either 'one to many' or 'many to one'. Never many to many (and never 0 anywhere)
            hasMultipleDeliveryComponents = deliveryComponents.Length > 1;
        }

        public override void setSource(FlowComponent source)
        {
            //We wire ourselves up completely (inputs and outputs), so don't need to do anything here.
        }

        private double[] modifyPreferenceBasedOnBackPressures(double[] preferredFlowPercent, FlowResponseData[] responses, FlowCalculationData baseData)
        {
            if (baseData.pressure == 0.0)
                return preferredFlowPercent;                //We don't want to have a divide by zero down below.

            //1.0 - backpressure/mcr-pressure seems to be a good starting guess for a scaler. 
            double sumBeforeModification = 0.0;
            for(int i = 0; i < preferredFlowPercent.Length; i++)
            {
                sumBeforeModification += preferredFlowPercent[i];
                double modifier = Math.Max(1.0 - (responses[i].backPressure / baseData.pressure), 0.000001);            //Don't let the number go completely to zero.
                preferredFlowPercent[i] *= modifier;
            }
            return PhysTools.normalizePercentArray(preferredFlowPercent, sumBeforeModification);
        }

        private String arrayToString(double[] array)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString());
                if(i < array.Length - 1)
                    sb.Append(", ");
            }
            return sb.ToString();
        }

        private FlowResponseData calculateSplittingFunctionality(FlowCalculationData baseData, double flowPercent, FlowComponent[] nodes, bool isDeliverySide, double pressurePercent)
        {
            FlowResponseData[] responses = new FlowResponseData[nodes.Length];
            bool foundNull = false;
            double maxBackPressure = 0.0;
            for (int attempt = 0; attempt < 2; attempt++)
            {
                //Most of the time, we will need to ask twice, because some will be null the first time.
                foundNull = false;
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (responses[i] == null)
                    {
                        if (isDeliverySide)
                            responses[i] = nodes[i].getDeliveryPossibleValues(baseData, this, Math.Min(flowPercent, maxWeights[i]), pressurePercent);
                        else
                            responses[i] = nodes[i].getSourcePossibleValues(baseData, this, Math.Min(flowPercent, maxWeights[i]), pressurePercent);
                    }

                    if (responses[i] == null)
                        foundNull = true;
                    else if (responses[i].backPressure > maxBackPressure)
                        maxBackPressure = responses[i].backPressure;
                }

                if (!foundNull)
                    break;
            }
            if (foundNull)
            {
                return null;                //We weren't able to get everything, and this is probably because there is another splitter up stream that will retry us again.
            }

            if (!flowPercentageSolutions.ContainsKey(baseData.flowDriver.name))
                flowPercentageSolutions[baseData.flowDriver.name] = new double[nodes.Length];
            
            //If we get here, then we have all the info we should need to solve ourselves.

            double desiredPercent = flowPercent;

            double[] maxFlowPercent = new double[nodes.Length];
            double[] preferredFlowPercent = new double[nodes.Length];
            double percentToReturn = 0.0;
            double sumOfMaxFlow = 0;
            double currentSum = 0.0;

            if (linkedCombiner != null)
            {
                desiredPercent = Math.Min(linkedCombiner.lastCombinePercent, flowPercent);            //All are equal... grab first. This is the combiner's maxFlow

                double[] flowPercentToCombiner;
                //NOTE: Both solutions with both divisors gave the same answer, and I found the bug elsewhere...
                //TODO: Determing what divisor we actually want to use.
                if (linkedCombiner.combinerMap.ContainsKey(baseData.flowDriver.name))
                    flowPercentToCombiner = linkedCombiner.combinerMap[baseData.flowDriver.name];
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
                
                bool modifySplitBasedOnBackPressures = true;            //I can foresee this wanting to be an optional parameter, sometimes doing it, and sometimes not.
                if (modifySplitBasedOnBackPressures)
                {
                    preferredFlowPercent = modifyPreferenceBasedOnBackPressures(preferredFlowPercent, responses, baseData);
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

            //TODO: Find better way to handle this. The 0.000000001 is there to handle rounding errors, when the sum really is the same number, but it rounds to slightly less...
            if (currentSum < desiredPercent - 0.000000001 && sumOfMaxFlow > 0.0)                //sum > 0.0 helps us avoid a divide by 0 below. Also, if we can't push anything, then 0 is the right answer
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
                    flowPercentageSolutions[baseData.flowDriver.name][i] = trueFlowPercent;
                    percentToReturn += trueFlowPercent;
                }
            }
            else
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    double trueFlowPercent = preferredFlowPercent[i];           //It all works out with preferred values.
                    flowPercentageSolutions[baseData.flowDriver.name][i] = trueFlowPercent;
                }
            }

            //Need to normalize the flowPercenageSolutions, so that they sum to 1.0, so that when we set values, we are spliting 100 % of the smaller portion that is coming down to us(and don't double-limit things).
            if (percentToReturn != 0.0)
            {
                for (int i = 0; i < nodes.Length; i++)
                    flowPercentageSolutions[baseData.flowDriver.name][i] /= percentToReturn;
            }
            
            FlowResponseData ret = new FlowResponseData();
            ret.flowPercent = percentToReturn;
            ret.flowVolume = percentToReturn * baseData.desiredFlowVolume;
            ret.backPressure = maxBackPressure;
            setPressures(baseData.pressure, ret.backPressure, pressurePercent, isDeliverySide);

            return ret;
        }

        private FlowResponseData calculateCombiningFunctionality(FlowCalculationData baseData, FlowComponent caller, double flowPercent, FlowComponent[] nodes, bool isDeliverySide, double pressurePercent)
        {
            if (!combinerMap.ContainsKey(baseData.flowDriver.name))
            {
                double[] toAdd = new double[indexByName.Count];
                for (int i = 0; i < indexByName.Count; i++)
                {
                    if (!indexesUsedByPump.ContainsKey(baseData.flowDriver.name) || indexesUsedByPump[baseData.flowDriver.name][i])
                        toAdd[i] = -1.0;                            //Initialize with all negative numbers, and wait until they are not negative to know when we are done.
                    else
                        toAdd[i] = 0.0;                             //Initialize to 0 if this pump will never get this number
                }
                combinerMap[baseData.flowDriver.name] = toAdd;
            }

            double[] percentMap = combinerMap[baseData.flowDriver.name];
            int index = indexByName[caller.name];
            percentMap[index] = flowPercent;

            double percentSum = 0.0;
            for (int i = 0; i < percentMap.Length; i++)
            {
                if (percentMap[i] < 0.0)
                    return null;                //We haven't seen all of the inputs to combine them... thus, return null until we do see them all.
                else
                    percentSum += percentMap[i];
            }

            if (percentSum > 1.0)
                percentSum = 1.0;

            if (!combinerDownstreamValues.ContainsKey(baseData.flowDriver.name))
            {
                if (isDeliverySide)
                    combinerDownstreamValues[baseData.flowDriver.name] = deliveryComponents[0].getDeliveryPossibleValues(baseData, this, percentSum, pressurePercent);
                else
                    combinerDownstreamValues[baseData.flowDriver.name] = sourceComponents[0].getSourcePossibleValues(baseData, this, percentSum, pressurePercent);
            }

            FlowResponseData ret = combinerDownstreamValues[baseData.flowDriver.name].clone();
            lastCombinePercent = ret.flowPercent;

            if (percentSum != 0.0)
            {
                ret.flowPercent *= (percentMap[index] / percentSum);             //The divide here is to normalize it.
                ret.flowVolume *= (percentMap[index] / percentSum);
            }

            if (!flowPercentageSolutions.ContainsKey(baseData.flowDriver.name))
                flowPercentageSolutions[baseData.flowDriver.name] = new double[1];
            flowPercentageSolutions[baseData.flowDriver.name][0] = ret.flowPercent;

            setPressures(baseData.pressure, ret.backPressure, pressurePercent, isDeliverySide);

            return ret;
        }

        private void setPressures(double pumpPressure, double backPressure, double pressurePercent, bool isDeliverySide)
        {
            if (isDeliverySide)
                setPressuresForDeliverySide(pumpPressure, backPressure, pressurePercent, pressurePercent);
            else
                setPressuresForSourceSide(pumpPressure, backPressure, pressurePercent, pressurePercent);
        }

        private double[] fixPercents(double[] preferredFlowPercent, double[] maxFlowPercent, double desiredPercent)
        {
            for(int attemp = 0; attemp < preferredFlowPercent.Length; attemp++)
            {
                double sum = 0.0;
                double goal = desiredPercent;
                for(int i = 0; i < preferredFlowPercent.Length; i++)
                {
                    if(preferredFlowPercent[i] >= maxFlowPercent[i])
                    {
                        //This one is at max, so doesn't count
                        preferredFlowPercent[i] = maxFlowPercent[i];
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

        private SettingResponseData setFlowValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, FlowComponent[] nodes, bool isDeliverySide, bool lastTime)
        {
            if(isDeliverySide)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].setDeliveryValues(baseData, this, flowPercentageSolutions[baseData.flowDriver.name][i] * flowVolume, lastTime);
                }
                finalFlow = flowVolume;

                currentFluidTypeMap = baseData.fluidTypeMap;               //On the delivery side, the mixture comes from passed in arguments
                inletTemperature = baseData.temperature;
                outletTemperature = baseData.temperature;
                
                return null;
            }
            else
            {
                SettingResponseData[] responses = new SettingResponseData[nodes.Length];
                double[] volumes = new double[nodes.Length];
                bool hasNull = false;
                for (int attempt = 0; attempt < 2; attempt++)
                {
                    hasNull = false;
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        volumes[i] = flowPercentageSolutions[baseData.flowDriver.name][i] * flowVolume;
                        if(responses[i] == null)
                            responses[i] = nodes[i].setSourceValues(baseData, this, volumes[i], lastTime);
                        if (responses[i] == null)
                            hasNull = true;
                    }
                    if (!hasNull)
                        break;
                }
                if (hasNull)
                    return null;

                finalFlow = flowVolume;
                SettingResponseData ret = PhysTools.mixFluidPercentsAndTemperatures(responses, volumes);
                ret.flowVolume = flowVolume;
                //ret.fluidTypeMap = PhysTools.mixFluids(responses, volumes);

                currentFluidTypeMap = ret.fluidTypeMap;                    //On source side, the mixture comes from the return values
                inletTemperature = ret.temperature;
                outletTemperature = ret.temperature;

                return ret;
            }
        }

        private SettingResponseData setCombiningFlowValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, FlowComponent[] nodes, bool isDeliverySide, bool lastTime)
        {
            int index = indexByName[caller.name];
            setCombiningVolumeMap[index] = flowVolume;   

            double volumeSum = 0.0;
            for (int i = 0; i < setCombiningVolumeMap.Length; i++)
            {
                if (setCombiningVolumeMap[i] < 0.0)
                    return null;                //We haven't seen all of the inputs to combine them... 
                else
                    volumeSum += setCombiningVolumeMap[i];
            }

            finalFlow = volumeSum;

            if (isDeliverySide)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    nodes[i].setDeliveryValues(baseData, this, volumeSum, lastTime);
                }

                currentFluidTypeMap = baseData.fluidTypeMap;               //On the delivery side, the mixture comes from passed in arguments
                inletTemperature = baseData.temperature;
                outletTemperature = baseData.temperature;

                return null;
            }
            else
            {
                if (nodes.Length != 1)
                    throw new Exception("Source combiners can only have 1 node");
                if (setterDownstreamValue == null)
                    setterDownstreamValue = nodes[0].setSourceValues(baseData, this, volumeSum, lastTime);
                
                SettingResponseData ret = new SettingResponseData();
                ret.flowVolume = volumeSum;
                ret.fluidTypeMap = setterDownstreamValue.fluidTypeMap;
                ret.temperature = setterDownstreamValue.temperature;

                currentFluidTypeMap = ret.fluidTypeMap;                    //On source side, the mixture comes from the return values
                inletTemperature = ret.temperature;
                outletTemperature = ret.temperature;

                return ret;
            }
        }

        public override FlowResponseData getDeliveryPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            if (hasMultipleDeliveryComponents)
                return calculateSplittingFunctionality(baseData, flowPercent, deliveryComponents, true, pressurePercent);
            else
                return calculateCombiningFunctionality(baseData, caller, flowPercent, sourceComponents, true, pressurePercent);
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            if (!hasMultipleDeliveryComponents)
                return calculateSplittingFunctionality(baseData, flowPercent, sourceComponents, false, pressurePercent);
            else
                return calculateCombiningFunctionality(baseData, caller, flowPercent, deliveryComponents, false, pressurePercent);
        }

        public override void setDeliveryValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            if (hasMultipleDeliveryComponents)
                setFlowValues(baseData, caller, flowVolume, deliveryComponents, true, lastTime);
            else
                setCombiningFlowValues(baseData, caller, flowVolume, deliveryComponents, true, lastTime);
        }

        public override SettingResponseData setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            if (!hasMultipleDeliveryComponents)
                return setFlowValues(baseData, caller, flowVolume, sourceComponents, false, lastTime);
            else
                return setCombiningFlowValues(baseData, caller, flowVolume, sourceComponents, false, lastTime);
        }

        public override void exploreDeliveryGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            for (int i = 0; i < deliveryComponents.Length; i++)
            {
                deliveryComponents[i].exploreDeliveryGraph(baseData, this);
            }
            if (!hasMultipleDeliveryComponents)           //Do the following if we are combining, so we know which combining imputs actually apply to given pump
            {
                if (!indexesUsedByPump.ContainsKey(baseData.flowDriver.name))
                    indexesUsedByPump[baseData.flowDriver.name] = new bool[sourceComponents.Length];
                indexesUsedByPump[baseData.flowDriver.name][indexByName[caller.name]] = true;
            }
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            for (int i = 0; i < sourceComponents.Length; i++)
            {
                sourceComponents[i].exploreSourceGraph(baseData, this);
            }
            if (hasMultipleDeliveryComponents)           //Do the following if we are combining, so we know which combining imputs actually apply to given pump
            {
                if (!indexesUsedByPump.ContainsKey(baseData.flowDriver.name))
                    indexesUsedByPump[baseData.flowDriver.name] = new bool[deliveryComponents.Length];
                indexesUsedByPump[baseData.flowDriver.name][indexByName[caller.name]] = true;
            }
        }
    }
}
