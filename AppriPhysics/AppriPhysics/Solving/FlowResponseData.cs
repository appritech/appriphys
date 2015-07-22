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


        public FlowComponent lastCombinerOrTank;            //TODO: Think of a real name for this, and make it real functionality
        public double lastCombinerOrTankPercent;            //TODO: Think of a real name for this, and make it real functionality
        public void setLastCombinerOrTank(FlowComponent combinerOrTank, double combinerOrTankPercent)
        {
            lastCombinerOrTank = combinerOrTank;
            lastCombinerOrTankPercent = combinerOrTankPercent;            //Current value is the one to stash.
        }
    }
}
