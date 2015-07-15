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
        public Dictionary<String, float> angerMap;
    }
}
