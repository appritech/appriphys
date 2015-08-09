using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class SettingResponseData
    {
        //NOTE: If this class grows much larger, then the temp and fluidTypeMap should be put into an interface, and have a super-light version for Tanks to mix the temps and fluidTypes.
        public SettingResponseData(double temperature, Dictionary<FluidType, double> fluidTypeMap)
        {
            this.temperature = temperature;
            this.fluidTypeMap = fluidTypeMap;
        }
        public SettingResponseData()
        {

        }
        public double flowVolume;
        public Dictionary<FluidType, double> fluidTypeMap;
        public double temperature;                          //TODO: Should this be an offset or absolute? I wanted offset, but what if one piping diagram spans multiple zones?
    }
}
