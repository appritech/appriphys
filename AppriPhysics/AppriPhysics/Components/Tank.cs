using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class Tank : FlowComponent
    {

        public Tank(String name, double capacity, Dictionary<FluidType, double> normalizedVolumeMap, double currentVolume, String[] deliveryNames, bool isSealed) : base(name)
        {
            this.deliveryNames = deliveryNames;
            this.capacity = capacity;
            this.currentFluidTypeMap = PhysTools.DictionaryCloner<FluidType, double>.cloneDictionary(normalizedVolumeMap);
            this.currentVolume = currentVolume;
            this.currentTemperature = 20.0;                 //20 degrees is a good round temperature for starters.
            this.isSealed = isSealed;
            
            //if(isSealed)
            //{
            //    //Initialize a sealed tank to be full of ambient air at normal atmospheric pressure, but the values for fluidTypeMap, and currentVolume come from external to this constructor... Be smart!
            //    currentVolume = capacity;
            //}
        }

        private String[] deliveryNames;
        private double capacity;
        private double currentVolume;
        public double currentTemperature;
        public double percentFilled = 0.0;          //NOTE: In a sealed tank, this doesn't equal currentVolume / capacity...

        private bool isSealed;
        public double normalPressureDelta = 0.0;             //This is the normal pressure differential that gives 100% flow. Any dP less than this will cause restricted flow. Any dP greater than this will be unrestricted.
        private double tankPressure = 0.0;                    //This is is barg. Always start at ambient pressure.

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            if (deliveryNames != null && deliveryNames.Length > 0)
            {
                for (int i = 0; i < deliveryNames.Length; i++)
                {
                    FlowComponent deliveryComponent = components[deliveryNames[i]];
                    deliveryComponent.setSource(this);
                }
            }
        }

        public override FlowResponseData getSourcePossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if(currentVolume > 0.0)
            {
                ret.flowPercent = flowPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
            }
            else
            {
                ret.flowPercent = 0.0f;
            }
            ret.flowVolume = flowPercent * baseData.desiredFlowVolume;
            ret.backPressure = tankPressure;
            ret.fluidTypeMap = currentFluidTypeMap;
            outletPressure = ret.backPressure;
            return ret;
        }

        public double getTankPressure()
        {
            return tankPressure;
        }

        public override FlowResponseData getDeliveryPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            FlowResponseData ret = new FlowResponseData();

            ret.backPressure = tankPressure;

            if (isSealed)
            {
                double deltaPressure = baseData.pressure - tankPressure;
                if(deltaPressure < 0.0)
                {
                    //There is too much back-pressure for this pump to overcome it. Thus, nothing will flow in.
                    ret.flowPercent = 0.0;
                }
                else if(deltaPressure > normalPressureDelta || normalPressureDelta == 0.0)          //Don't allow else to divide by 0
                {
                    //We don't add any restriction, since the dP is greater than our defined 'normal'.
                    ret.flowPercent = flowPercent;
                }
                else
                {
                    //Here we need to do a linear interpolation between deltaPressure = normalPressure meaning 1, and deltaPressure = 0 meaning 0.
                    //Thus, at normalPressure, we get full flow, and at no pressure difference, we get no flow.
                    ret.flowPercent = deltaPressure / normalPressureDelta * flowPercent;
                }
            }
            else
            {
                if (currentVolume < double.MaxValue)             //TODO: Make it so that tanks can't overflow, especially sealed tanks... Right now, it will pretty much always accept any flow that we want to put into it...
                {
                    ret.flowPercent = flowPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
                }
                else
                {
                    ret.flowPercent = 0.0;
                }
            }

            ret.flowVolume = flowPercent * baseData.desiredFlowVolume;
            ret.fluidTypeMap = currentFluidTypeMap;
            outletPressure = ret.backPressure;
            return ret;
        }
        public override void setSource(FlowComponent source)
        {
            //Tanks are the ultimate source... so we don't really need to do anything right now, until we are tracking the multiple inputs and outputs... Then, we will need to keep track of all of them probably...
        }

        public void setCurrentVolume(double volume)
        {
            this.currentVolume = volume;
        }

        public double getCurrentVolume()
        {
            return this.currentVolume;
        }

        public override SettingResponseData setSourceValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            SettingResponseData ret = new SettingResponseData();
            finalFlow -= flowVolume;
            if(lastTime)
            {
                currentVolume -= flowVolume * PhysTools.timeStep;
            }

            ret.flowVolume = flowVolume;
            ret.fluidTypeMap = currentFluidTypeMap;
            ret.temperature = currentTemperature;

            inletTemperature = currentTemperature;
            outletTemperature = currentTemperature;

            return ret;
        }

        public override void setDeliveryValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
        {
            finalFlow += flowVolume;
            if(lastTime)
            {
                double volumeToAdd = flowVolume * PhysTools.timeStep;
                SettingResponseData tankCurrentFluids = new SettingResponseData(currentTemperature, this.currentFluidTypeMap);
                SettingResponseData incomingFluids = new SettingResponseData(baseData.temperature, baseData.fluidTypeMap);
                SettingResponseData mixture = PhysTools.mixFluidPercentsAndTemperatures(new SettingResponseData[] { tankCurrentFluids, incomingFluids }, new double[] { currentVolume, volumeToAdd });

                this.currentTemperature = mixture.temperature;
                this.currentFluidTypeMap = mixture.fluidTypeMap;

                if(isSealed)
                {
                    currentVolume += volumeToAdd;
                    //Now, we need to calculate the pressure.
                    double gasVolume = 0.0;
                    double liquidVolume = 0.0;
                    PhysTools.normalizeFluidMixture(ref currentFluidTypeMap);               //Make sure that the percentages are normalized, otherwise very bad things would happen
                    foreach (KeyValuePair<FluidType, double> kvp in currentFluidTypeMap)
                    {
                        //NOTE: If we have multiple gasses that need to compress differently (I don't think that is possible, even with AIR and STEAM), then we need to re-think this algorithm from the ground up.
                        if (kvp.Key.isGas)
                            gasVolume += kvp.Value * currentVolume;
                        else
                            liquidVolume += kvp.Value * currentVolume;
                    }

                    percentFilled = liquidVolume / capacity;
                    double volumeForGas = capacity - liquidVolume;
                    tankPressure = (gasVolume / volumeForGas) - 1.0;                //Subtract 1.0 to convert from bara to barg. 
                }
                else
                {
                    double adjustmentToMake = 0.0;
                    //Vent any gasses out the top.
                    foreach(FluidType key in currentFluidTypeMap.Keys.ToList<FluidType>())
                    {
                        if(key.isGas)
                        {
                            adjustmentToMake += currentFluidTypeMap[key];
                            currentFluidTypeMap.Remove(key);
                        }
                    }
                    if (adjustmentToMake > 0.0)
                    {
                        //We ended up removing some gas, so we need to re-normalize, and adjust the values
                        PhysTools.normalizeFluidMixture(ref currentFluidTypeMap);
                        volumeToAdd *= (1.0 - adjustmentToMake);                    //Remove the volume of gas that is escaping
                    }
                    tankPressure = 0.4;                 //TODO: Adjust non-sealed tank outlet pressures based on size/shape, etc.
                    
                    currentVolume += volumeToAdd;

                    percentFilled = currentVolume / capacity;
                }
            }

            inletTemperature = currentTemperature;
            outletTemperature = currentTemperature;
        }

        public void overrideFluidType(FluidType newType)
        {
            currentFluidTypeMap.Clear();
            currentFluidTypeMap.Add(newType, 1.0);
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }

        public override void exploreDeliveryGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }
    }
}
