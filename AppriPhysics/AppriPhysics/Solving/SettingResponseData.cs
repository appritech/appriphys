using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class SettingResponseData
    {
        public SettingResponseData()
        {

        }
        public double flowVolume;
        public Dictionary<FluidType, double> fluidTypeMap;
        public double temperature;                          //TODO: Should this be an offset or absolute? I wanted offset, but what if one piping diagram spans multiple zones?
    }
}
