using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineSim;

using AppriPhysics.Components;
using AppriPhysics.Solving;

using EngineSim.Components.Controllers;
using System.ComponentModel;

namespace AppriConnectorComponents.FlowModel
{
    [ERSVisibility("AppriGraphSolverWrapper", ERSVisibilityEnum.DEVELOPER)]
    [ERS_UIVisibility(ERS_UIInterfaceEnum.VIRTUAL_PANEL)]
    public class AppriGraphSolverWrapper : CoreComponentBase
    {
        public AppriGraphSolverWrapper(string name)
            : base(name)
        {
            
        }
        
        [DoNotSaveInSnapshot]
        private DynamicAssociatedComponents<FlowDriver> _pumps;
        [Category(CategoryAssociations)]
        [ERSVisibility("Pumps", ERSVisibilityEnum.DEVELOPER)]
        public DynamicAssociatedComponents<FlowDriver> Pumps
        {
            get { return _pumps; }
            set { _pumps = value; }
        }

        public override bool HandleDataRequest(DataRequestTypes flagOfDataRequested, ref UnionDataType returnVal)
        {
            switch (flagOfDataRequested)
            {

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

