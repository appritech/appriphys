using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineSim;

using AppriPhysics.Components;

namespace AppriConnectorComponents.FlowModel
{
    [ERSVisibility("AppriPumpWrapper", ERSVisibilityEnum.DEVELOPER)]
    [ERS_UIVisibility(ERS_UIInterfaceEnum.VIRTUAL_PANEL)]
    public class AppriPumpWrapper : CoreComponentBase
    {
        public AppriPumpWrapper(string name)
            :base(name)
        {
            pump = new FlowDriver(name, 100.0, 3.2, "");
        }

        private FlowDriver pump;

        public override bool HandleDataRequest(DataRequestTypes flagOfDataRequested, ref UnionDataType returnVal)
        {
            switch (flagOfDataRequested)
            {
                case DataRequestTypes.PUMP_INLET_PRESSURE:
                    returnVal.floatVal = (float)pump.inletPressure;
                    break;
                case DataRequestTypes.PUMP_OUTLET_PRESSURE:
                    returnVal.floatVal = (float)pump.outletPressure;
                    break;

                case DataRequestTypes.FLOW:
                    returnVal.floatVal = (float)pump.finalFlow;
                    break;

                case DataRequestTypes.PUMP_INLET_TEMPERATURE:
                    returnVal.floatVal = (float)pump.inletTemperature;
                    break;
                case DataRequestTypes.PUMP_OUTLET_TEMPERATURE:
                    returnVal.floatVal = (float)pump.outletTemperature;
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
