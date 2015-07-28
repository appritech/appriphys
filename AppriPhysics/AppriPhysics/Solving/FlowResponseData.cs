using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class FlowResponseData
    {
        public FlowResponseData()
        {

        }
        public double flowPercent;
        public double flowVolume;
        public double backPressure;
        public Dictionary<FluidType, double> fluidTypeMap;
        
        public FlowResponseData clone()
        {
            FlowResponseData ret = new FlowResponseData();

            ret.flowPercent = flowPercent;
            ret.flowVolume = flowVolume;
            ret.backPressure = backPressure;

            ret.fluidTypeMap = PhysTools.DictionaryCloner<FluidType, double>.cloneDictionary(fluidTypeMap);

            return ret;
        }
    }
}
