using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeeMind.Pages;

public static class FluentExtensions
{
    public static T When<T>(this T obj, Func<bool> testFunc, Func<T, T> positiveAction)
    {
        if (testFunc())
        {
            return positiveAction(obj);
        }

        return obj;
    }

    public static T When<T>(this T obj, Func<T, bool> testFunc, Func<T, T> positiveAction)
    {
        if (testFunc(obj))
        {
            return positiveAction(obj);
        }

        return obj;
    }

    public static T When<T>(this T obj, bool flag, Func<T, T> positiveAction)
    {
        if (flag)
        {
            return positiveAction(obj);
        }

        return obj;
    }
}
