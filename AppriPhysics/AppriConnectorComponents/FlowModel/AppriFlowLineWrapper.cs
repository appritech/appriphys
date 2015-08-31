using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineSim;

using AppriPhysics.Components;

namespace AppriConnectorComponents.FlowModel
{
    [ERSVisibility("AppriFlowLineWrapper", ERSVisibilityEnum.DEVELOPER)]
    [ERS_UIVisibility(ERS_UIInterfaceEnum.VIRTUAL_PANEL)]
    public class AppriFlowLineWrapper : CoreComponentBase
    {
        public AppriFlowLineWrapper(string name)
            : base(name)
        {
            flowLine = new FlowLine(name, "");
        }

        private FlowLine flowLine;

        public override bool HandleDataRequest(DataRequestTypes flagOfDataRequested, ref UnionDataType returnVal)
        {
            switch (flagOfDataRequested)
            {
                case DataRequestTypes.FLOW:
                    returnVal.floatVal = (float)flowLine.finalFlow;
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
