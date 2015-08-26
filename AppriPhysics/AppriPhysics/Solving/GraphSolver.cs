using System;
using System.Collections.Generic;
using AppriPhysics.Components;

namespace AppriPhysics.Solving
{
    public class GraphSolver
    {

        private Dictionary<String, FlowComponent> components = new Dictionary<String, FlowComponent>();
        private Dictionary<String, FlowDriver> flowDrivers = new Dictionary<String, FlowDriver>();                    //This could probably be a set, but we might want to find them...
        private Dictionary<String, double> angerMap = new Dictionary<String, double>();

        public void addComponent(FlowComponent comp)
        {
            components.Add(comp.name, comp);
            if(comp is FlowDriver)
            {
                flowDrivers.Add(comp.name, (FlowDriver)comp);
            }
        }

        public FlowComponent getComponent(string name)
        {
            return components[name];
        }

        public void connectComponents()
        {
            foreach(FlowComponent iter in components.Values)
            {
                iter.connectSelf(components);
            }
            foreach(FlowDriver iter in flowDrivers.Values)
            {
                FlowCalculationData baseData = new FlowCalculationData(iter, angerMap, 0);
                baseData.flowDriver = iter;
                iter.exploreDeliveryGraph(baseData, null);
                iter.exploreSourceGraph(baseData, null);
                flowDriverModifiers[iter.name] = new FlowDriverModifier();
            }
        }

        public void solveMimic()
        {
            bool solved = false;
            int attempt = 0;
            clearFullState();
            while(!solved)
            {
                resetPartialState();

                bool possibleSolve = true;
                attempt++;
                foreach(FlowDriver p in flowDrivers.Values)
                {
                    if (!attemptSolve(p, attempt))
                        possibleSolve = false;
                }

                //Right now, we have to apply the solution each time to see if we have anger... need to rethink the algorithm...
                applySolution(false);

                bool hasAnger = checkAnger();
                if (hasAnger)
                    possibleSolve = false;          //This isn't the final solution, because we still have ANGER!!!

                if (possibleSolve)          //If we haven't determined our solution to be invalid here, then we have solved it!
                    solved = true;
            }

            resetPartialState();
            //Now that we have a final solution, apply the values and let the temperatures and changing of tank levels, etc happen.
            applySolution(true);
        }

        private void applySolution(bool lastTime)
        {
            bool allApplied = false;
            while (!allApplied)
            {
                allApplied = true;
                foreach (FlowDriver p in flowDrivers.Values)
                {
                    if (!p.applySolution(new FlowCalculationData(p, angerMap, 0), flowDriverModifiers[p.name], lastTime))
                        allApplied = false;
                }
            }
        }

        private bool attemptSolve(FlowDriver p, int attempt)
        {
            double flowModifier = flowDriverModifiers[p.name].flowPercent;

            FlowCalculationData baseData = new FlowCalculationData(p, angerMap, attempt);
            FlowResponseData sourceAbility = p.getFlowDriverSourcePossibleValues(baseData, flowDriverModifiers[p.name]);
            baseData.fluidTypeMap = sourceAbility.fluidTypeMap;             //Pass the mixture stuff from source to delivery
            FlowResponseData deliveryAbility = p.getFlowDriverDeliveryPossibleValues(baseData, flowDriverModifiers[p.name]);

            if (flowDriverModifiers[p.name].updateStateRequiresNewSolution(sourceAbility, deliveryAbility))
                return false;

            return true;
        }
        
        Dictionary<String, FlowDriverModifier> flowDriverModifiers = new Dictionary<string, FlowDriverModifier>();

        private void resetPartialState()
        {
            foreach (FlowComponent iter in components.Values)
            {
                iter.resetState();
            }
        }

        private void clearFullState()
        {
            foreach (FlowDriverModifier iter in flowDriverModifiers.Values)
                iter.clearState();
            angerMap.Clear();
        }

        private bool checkAnger()
        {
            FlowComponent angriestComponent = null;
            double angriestAngerLevel = 0.0;
            foreach (FlowComponent c in components.Values)
            {
                double anger = c.getAngerLevel(angerMap);
                if (anger != 0.0)
                {
                    if (angriestComponent == null || angriestAngerLevel < anger)
                    {
                        angriestComponent = c;
                        angriestAngerLevel = anger;
                        //break;              //Temporary to test forgiveness in simpler cases and making sure order doesn't matter.
                    }
                }
            }
            if (angriestComponent != null)
            {
                if (angriestAngerLevel > 0.0)
                {
                    if (angerMap.ContainsKey(angriestComponent.name))
                        angerMap[angriestComponent.name] *= angriestAngerLevel;                //Add the new anger on top of the old anger
                    else
                        angerMap[angriestComponent.name] = angriestAngerLevel;
                }
                else if (angriestAngerLevel < 0.0)
                {
                    //This is negative anger, or forgiveness. That means to just clear this person from the angerMap
                    angerMap.Remove(angriestComponent.name);
                }
                return true;
            }
            return false;           //No anger found
        }

        public void printSolution()
        {
            foreach(FlowComponent iter in components.Values)
            {
                System.Console.WriteLine(iter.solutionString());
            }
        }

    }
}
