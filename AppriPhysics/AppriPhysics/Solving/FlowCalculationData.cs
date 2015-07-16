using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class FlowCalculationData
    {
        public FlowComponent flowPusher;
        public double desiredFlowVolume;
        public int attempt;
        public Dictionary<String, double> angerMap = new Dictionary<String, double>();                  //TOOD: Make this lazy loading (probably a getter)
        public Dictionary<String, double[]> combinerMap = new Dictionary<String, double[]>();           //TOOD: This shouldn't really be here...
        //public Dictionary<String, double[]> combinerApplyMap = new Dictionary<String, double[]>();           //TOOD: This shouldn't really be here...
    }
}
