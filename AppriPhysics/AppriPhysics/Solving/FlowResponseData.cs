using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class FlowResponseData
    {
        public double flowPercent;
        public double flowVolume;
        
        public FlowResponseData clone()
        {
            FlowResponseData ret = new FlowResponseData();

            ret.flowPercent = flowPercent;
            ret.flowVolume = flowVolume;

            return ret;
        }
    }
}
