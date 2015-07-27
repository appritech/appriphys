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
        
        public FlowResponseData clone()
        {
            FlowResponseData ret = new FlowResponseData();

            ret.flowPercent = flowPercent;
            ret.flowVolume = flowVolume;
            ret.backPressure = backPressure;

            return ret;
        }
    }
}
