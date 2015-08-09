﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Solving;

namespace AppriPhysics.Components
{
    public class Tank : FlowComponent
    {

        public Tank(String name, double capacity, Dictionary<FluidType, double> normalizedVolumeMap, double currentVolume, String[] sinkNames) : base(name)
        {
            this.sinkNames = sinkNames;
            this.capacity = capacity;
            this.currentFluidTypeMap = normalizedVolumeMap;
            this.currentVolume = currentVolume;
            this.currentTemperature = 20.0;                 //20 degrees is a good round temperature for starters.
        }

        private String[] sinkNames;
        //private FlowComponent sink;             //Needs to be a collection of sinks, if we think we want to use it.
        private double capacity;
        private double currentVolume;
        //private Dictionary<FluidType, double> normalizedVolumeMap;
        public double currentTemperature;

        public override void connectSelf(Dictionary<String, FlowComponent> components)
        {
            if (sinkNames != null && sinkNames.Length > 0)
            {
                for (int i = 0; i < sinkNames.Length; i++)
                {
                    FlowComponent sink = components[sinkNames[i]];
                    sink.setSource(this);
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
            ret.backPressure = 0.4;                     //TODO: Make real backpressure based on tank height, etc.
            ret.fluidTypeMap = currentFluidTypeMap;
            outletPressure = ret.backPressure;
            return ret;
        }
        public override FlowResponseData getSinkPossibleValues(FlowCalculationData baseData, FlowComponent caller, double flowPercent, double pressurePercent)
        {
            FlowResponseData ret = new FlowResponseData();
            if (currentVolume < double.MaxValue)             //TODO: Make it so that tanks can't overflow, especially sealed tanks... Right now, it will pretty much always accept any flow that we want to put into it...
            {
                ret.flowPercent = flowPercent;           //Allow everything that they are asking for, since we don't do restrictions inside the tank.
            }
            else
            {
                ret.flowPercent = 0.0f;
            }
            ret.flowVolume = flowPercent * baseData.desiredFlowVolume;
            ret.backPressure = 0.4;                     //TODO: Make real backpressure based on tank height, etc.
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

        public override void setSinkValues(FlowCalculationData baseData, FlowComponent caller, double flowVolume, bool lastTime)
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
                
                currentVolume += volumeToAdd;
            }

            inletTemperature = currentTemperature;
            outletTemperature = currentTemperature;
        }

        public override void exploreSourceGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }

        public override void exploreSinkGraph(FlowCalculationData baseData, FlowComponent caller)
        {
            //Don't have to do anything, since this is the end of the line
        }
    }
}
