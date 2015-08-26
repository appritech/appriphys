using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class FluidType
    {
        public static readonly FluidType WATER = new FluidType("Water", false);
        public static readonly FluidType SEA_WATER = new FluidType("Sea Water", false);
        public static readonly FluidType AIR = new FluidType("Air", true);
        public static readonly FluidType DIESEL_OIL = new FluidType("Diesel Oil", false);

        public static Dictionary<FluidType, double> createSingleVolumeMap(FluidType fluidType)
        {
            Dictionary<FluidType, double> ret = new Dictionary<FluidType, double>();
            ret.Add(fluidType, 1.0);
            return ret;
        }

        public static IEnumerable<FluidType> Values
        {
            get
            {
                yield return WATER;
                yield return SEA_WATER;
                yield return AIR;
                yield return DIESEL_OIL;
            }
        }

        public String description;
        public bool isGas;

        //TODO: Probably want to add SG, and other properties
        private FluidType(String description, bool isGas)
        {
            this.description = description;
            this.isGas = isGas;
        }

    }
}
