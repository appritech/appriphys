using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Components
{
    public class FlowCalculationData
    {
        public FlowCalculationData(FlowComponent flowPusher, Dictionary<String, double> angerMap, int attempt)
        {
            this.flowPusher = flowPusher;
            this.angerMap = angerMap;
            this.attempt = attempt;
        }

        public FlowComponent flowPusher;
        public int attempt;
        public Dictionary<String, double> angerMap;

        public double desiredFlowVolume;
    }
}
