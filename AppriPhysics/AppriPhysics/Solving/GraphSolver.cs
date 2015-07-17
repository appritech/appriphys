using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppriPhysics.Components;

namespace AppriPhysics.Solving
{
    public class GraphSolver
    {

        private Dictionary<String, FlowComponent> components = new Dictionary<String, FlowComponent>();
        private Dictionary<String, Pump> pumps = new Dictionary<String, Pump>();                    //This could probably be a set, but we might want to find them...

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
                FlowCalculationData baseData = new FlowCalculationData();
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
                bool possibleSolve = true;
                attempt++;
                foreach(Pump p in pumps.Values)
                {
                    if (!attemptSolve(p, attempt))
                        possibleSolve = false;
                }

                //TODO: Check for anger

                if (possibleSolve)          //If we haven't determined our solution to be invalid here, then we have solved it!
                    solved = true;
            }

            foreach(Pump p in pumps.Values)
            {
                applySolution(p);
            }
        }

        private void applySolution(Pump p)
        {
            double modifier = 1.0f;
            if (pumpModifiers.ContainsKey(p.name))
                modifier = pumpModifiers[p.name];

            p.applySolution(modifier);
        }

        Dictionary<String, double> pumpModifiers = new Dictionary<string, double>();

        private void clearFullState()
        {
            pumpModifiers.Clear();
        }

        private bool attemptSolve(Pump p, int attempt)
        {
            double modifier = 1.0f;
            if (pumpModifiers.ContainsKey(p.name))
                modifier = pumpModifiers[p.name];
            
            FlowResponseData sourceAbility = p.getSourcePossibleFlow(null, null, modifier);       //Only pumps can take null
            FlowResponseData sinkAbility = p.getSinkPossibleFlow(null, null, modifier);           //Only pumps can take null
            double minAbility = Math.Min(sourceAbility.flowPercent, sinkAbility.flowPercent);
            if(minAbility < modifier)
            {
                pumpModifiers.Add(p.name, minAbility);
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
                String dumb = iter.solutionString();
                System.Console.WriteLine(iter.solutionString());
            }
        }

    }
}
