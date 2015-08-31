using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineSim;

using AppriPhysics.Components;
using AppriPhysics.Solving;

namespace AppriConnectorComponents.FlowModel
{
    [ERSVisibility("AppriTankWrapper", ERSVisibilityEnum.DEVELOPER)]
    [ERS_UIVisibility(ERS_UIInterfaceEnum.VIRTUAL_PANEL)]
    public class AppriTankWrapper : CoreComponentBase
    {
        public AppriTankWrapper(string name)
            : base(name)
        {
            tank = new Tank(name, 10000.0, FluidType.createSingleVolumeMap(FluidType.WATER), 5000.0, new String[] { }, false);
        }

        private Tank tank;

        public override bool HandleDataRequest(DataRequestTypes flagOfDataRequested, ref UnionDataType returnVal)
        {
            switch (flagOfDataRequested)
            {
                case DataRequestTypes.FILLED_PERCENT_REAL:
                    returnVal.floatVal = (float)tank.percentFilled;
                    break;

                default:
                    return base.HandleDataRequest(flagOfDataRequested, ref returnVal);
            }
            return true;
        }

        public override int MaxNumValidInputs
        {
            get { return 1; }
        }

        public override int MaxNumValidOutputs
        {
            get { return 1; }
        }
    }
}
