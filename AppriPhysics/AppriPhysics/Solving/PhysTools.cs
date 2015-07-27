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

        public static float timeStep = 0.1f;                     //For now, have a static rate of 10 updates per second. All FlowComponents read from here, so this can change, even dynamically.
    }
}
