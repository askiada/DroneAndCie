using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

public static class Activation {

    public static float[,] InvokeMethod(string methodName, List<object> args)
    {
        Type t = typeof(Activation);
        MethodInfo info = t.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        return (float[,])info.Invoke(null, args.ToArray());
    }

    static float[,]  DerivativeSigmoid(float[,] values)
    {
        float[,] tmp = new float[1, values.GetLength(1)];
        tmp = Sigmoid(values, false);

        for (int i = 0; i < values.GetLength(1); i++)
        {
            tmp[0, i] = (float)(1 - (tmp[0, i] * tmp[0, i]));
        }

        return tmp;
    }

    static float[,]  Sigmoid(float[,] values, bool reverse = true)
    {
        if (reverse)
        {
            float[,] tmp = new float[values.GetLength(1), 1];
            for (int i = 0; i < values.GetLength(1); i++)
            {
                tmp[i, 0] = (float)System.Math.Tanh(values[0, i]);
            }
            return tmp;
        }
        else
        {
            float[,] tmp = new float[1, values.GetLength(1)];
            for (int i = 0; i < values.GetLength(1); i++)
            {
                tmp[0, i] = (float)System.Math.Tanh(values[0, i]);
            }
            return tmp;
        }
    }

    static float[,]  DerivativeSigmoidExp(float[,] values)
    {
        float[,] tmp = new float[1, values.GetLength(1)];
        tmp = SigmoidExp(values, false);

        for (int i = 0; i < values.GetLength(1); i++)
        {
            tmp[0, i] = (float)(tmp[0, i] * (1.0 - tmp[0, i]));
        }
        return tmp;
    }

    static float[,]  SigmoidExp(float[,] values, bool reverse = true)
    {
        if (reverse)
        {
            float[,] tmp = new float[values.GetLength(1), 1];
            for (int i = 0; i < values.GetLength(1); i++)
            {
                tmp[i, 0] = (float)(1 / (1 + System.Math.Exp((float)(-values[0, i]))));
            }
            return tmp;
        }
        else
        {
            float[,] tmp = new float[1, values.GetLength(1)];
            for (int i = 0; i < values.GetLength(1); i++)
            {
                tmp[0, i] = (float)(1 / (1 + System.Math.Exp((float)(-values[0, i]))));
            }
            return tmp;
        }
    }

    public static float[,] OutputDerivativeActivation(float[,] values, string name)
    {
        return InvokeMethod("Derivative" + name, new List<object> { values });
    }

    public static float[,] OutputActivation(float[,] values, string name, bool reverse = true)
    {
        return InvokeMethod(name, new List <object> { values, reverse });
    }

    public static float[,] InternalDerivativeActivation(float[,] values, string name )
    {

        return InvokeMethod("Derivative" + name, new List<object> { values });
    }

    public static float[,] InternalActivation(float[,] values, string name, bool reverse = true)
    {
        return InvokeMethod(name, new List<object> { values, reverse });
    }


        /*public static float[,] InternalDerivativeActivation(float[,] values)
        {
            float[,] tmp = new float[1, values.GetLength(1)];
            tmp = InternalActivation(values, false);

            for (int i = 0; i < values.GetLength(1); i++)
            {
                tmp[0, i] = (float)(1 - (tmp[0, i] * tmp[0, i]));
            }

            return tmp;
        }

        public static float[,] InternalActivation(float[,] values, bool reverse = true)
        {
            if (reverse)
            {
                float[,] tmp = new float[values.GetLength(1), 1];
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    tmp[i, 0] = (float)System.Math.Tanh(values[0, i]);
                }
                return tmp;
            }
            else
            {
                float[,] tmp = new float[1, values.GetLength(1)];
                for (int i = 0; i < values.GetLength(1); i++)
                {
                    tmp[0, i] = (float)System.Math.Tanh(values[0, i]);
                }
                return tmp;
            }
    }*/
}
