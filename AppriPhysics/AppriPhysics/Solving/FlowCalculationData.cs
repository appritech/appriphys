using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Components;

namespace AppriPhysics.Solving
{
    public class FlowCalculationData
    {
        public FlowCalculationData(FlowComponent flowDriver, Dictionary<String, double> angerMap, int attempt)
        {
            this.flowDriver = flowDriver;
            this.angerMap = angerMap;
            this.attempt = attempt;
        }

        public FlowComponent flowDriver;
        public int attempt;
        public Dictionary<String, double> angerMap;

        public double desiredFlowVolume;
        public double pressure;
        public Dictionary<FluidType, double> fluidTypeMap;
        public double temperature;

        public void applySourceResponse(SettingResponseData sourceResponse)
        {
            this.fluidTypeMap = sourceResponse.fluidTypeMap;
            this.temperature = sourceResponse.temperature;
        }
    }
}
