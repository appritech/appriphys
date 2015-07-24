﻿using System;
using System.Collections.Generic;
using AppriPhysics.Components;

namespace AppriPhysics.Solving
{
    public class GraphSolver
    {

        private Dictionary<String, FlowComponent> components = new Dictionary<String, FlowComponent>();
        private Dictionary<String, Pump> pumps = new Dictionary<String, Pump>();                    //This could probably be a set, but we might want to find them...
        private Dictionary<String, double> angerMap = new Dictionary<String, double>();

        public void addComponent(FlowComponent comp)
        {
            components.Add(comp.name, comp);
            if(comp is Pump)
            {
                pumps.Add(comp.name, (Pump)comp);
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
            foreach(Pump iter in pumps.Values)
            {
                FlowCalculationData baseData = new FlowCalculationData(iter, angerMap, 0);
                baseData.flowPusher = iter;
                iter.exploreSinkGraph(baseData, null);
                iter.exploreSourceGraph(baseData, null);
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
                foreach(Pump p in pumps.Values)
                {
                    if (!attemptSolve(p, attempt))
                        possibleSolve = false;
                }

                //Right now, we have to apply the solution each time to see if we have anger... need to rethink the algorithm...
                foreach (Pump p in pumps.Values)
                {
                    applySolution(p);
                }

                bool hasAnger = checkAnger();
                if (hasAnger)
                    possibleSolve = false;          //This isn't the final solution, because we still have ANGER!!!

                if (possibleSolve)          //If we haven't determined our solution to be invalid here, then we have solved it!
                    solved = true;
            }
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

        private void applySolution(Pump p)
        {
            double modifier = 1.0f;
            if (pumpModifiers.ContainsKey(p.name))
                modifier = pumpModifiers[p.name];

            p.applySolution(new FlowCalculationData(p, angerMap, 0), modifier);
        }

        Dictionary<String, double> pumpModifiers = new Dictionary<string, double>();

        private void resetPartialState()
        {
            foreach (FlowComponent iter in components.Values)
            {
                iter.resetState();
            }
        }

        private void clearFullState()
        {
            pumpModifiers.Clear();
            angerMap.Clear();
        }

        private bool attemptSolve(Pump p, int attempt)
        {
            double modifier = 1.0f;
            if (pumpModifiers.ContainsKey(p.name))
                modifier = pumpModifiers[p.name];

            FlowCalculationData baseData = new FlowCalculationData(p, angerMap, attempt);
            FlowResponseData sourceAbility = p.getSourcePossibleValues(baseData, null, modifier);       //Only pumps can take null
            FlowResponseData sinkAbility = p.getSinkPossibleValues(baseData, null, modifier);           //Only pumps can take null
            double minAbility = Math.Min(sourceAbility.flowPercent, sinkAbility.flowPercent);
            if(minAbility < modifier)
            {
                pumpModifiers[p.name] = minAbility;
                return false;
            }
            else
            {
                return true;                //This means we actually have solved something...
            }
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
