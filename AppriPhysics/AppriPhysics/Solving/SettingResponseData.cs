using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class SettingResponseData
    {
        public double flowVolume;
        public Dictionary<FluidType, double> fluidTypeMap;
        //TODO: Add temperature here
    }
}
