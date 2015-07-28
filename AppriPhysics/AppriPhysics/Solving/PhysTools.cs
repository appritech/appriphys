using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppriPhysics.Solving
{
    public class PhysTools
    {
        public static double[] normalizePercentArray(double[] array, double sumGoal)
        {
            double sum = 0.0;
            for(int i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            if (sum != 0.0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] *= (sumGoal / sum);
                }
            }
            return array;
        }

        public static Dictionary<FluidType, double> calculateNormalizedFluidMap(Dictionary<FluidType, double> map)
        {
            Dictionary<FluidType, double> ret = DictionaryCloner<FluidType, double>.cloneDictionary(map);

            return ret;
        }

        public class DictionaryCloner<TKey, TValue>
        {
            public static Dictionary<TKey, TValue> cloneDictionary(Dictionary<TKey, TValue> toClone)
            {
                if (toClone == null)
                    return null;
                return toClone.ToDictionary(entry => entry.Key, entry => entry.Value); ;
            }
        }

        public static SettingResponseData mixFluidPercentsAndTemperatures(SettingResponseData[] responses, double[] splitValues)
        {
            SettingResponseData ret = new SettingResponseData();
            Dictionary<FluidType, double> fluidTypeMap = new Dictionary<FluidType, double>();
            double fluidTypeSum = 0.0;
            double temperature = 0.0;
            double tempDivisor = 0.0;

            for (int i = 0; i < responses.Length; i++)
            {
                foreach (KeyValuePair<FluidType, double> iter in responses[i].fluidTypeMap)
                {
                    if (splitValues[i] == 0.0)              //TODO: Make sure we don't crash and burn if all of the percentages are 0....
                        continue;

                    if (!fluidTypeMap.ContainsKey(iter.Key))
                        fluidTypeMap.Add(iter.Key, 0.0);
                    double amountToAdd = iter.Value * splitValues[i];
                    fluidTypeMap[iter.Key] += amountToAdd;
                    fluidTypeSum += amountToAdd;
                }
                temperature += responses[i].temperature * splitValues[i];
                tempDivisor += splitValues[i];
            }

            //Normalize oursleves if needed
            if (fluidTypeSum != 1.0 && fluidTypeSum != 0.0)          //1 means we are already normal, 0 would cause NAN.
            {
                //Need to go through and re-normalize
                foreach (FluidType key in fluidTypeMap.Keys.ToList())                //The ToList creates a copy of the keys, which makes it not throw an exception.
                {
                    fluidTypeMap[key] /= fluidTypeSum;
                }
            }

            if (tempDivisor == 0.0)
                ret.temperature = responses[0].temperature;         //Just pick one? It doesn't matter since there is no flow??? TODO: Figure out what this acutally means.
            else
                ret.temperature = temperature / tempDivisor;

            ret.fluidTypeMap = fluidTypeMap;
            return ret;
        }

        public static float timeStep = 0.1f;                     //For now, have a static rate of 10 updates per second. All FlowComponents read from here, so this can change, even dynamically.
    }
}
