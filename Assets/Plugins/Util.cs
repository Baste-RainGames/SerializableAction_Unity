using System;
using System.Collections.Generic;
using System.Text;

namespace SerializableActions.Internal
{
    public static class Util
    {
        /// <summary>
        /// Returns a pretty string representation of an Array. Or anything else that's IEnumerable. Like a list or whatever.
        ///
        /// Does basic [element,element] formatting, and also does recursive calls to inner lists. You can also give it a functon to
        /// do even prettier printing, usefull to get IE. a GameObject's name instead of "name (UnityEngine.GameObject)". If the function
        /// isn't supplied, toString is used.
        ///
        /// Also turns null into "null" instead of ""
        ///
        /// Will cause a stack overflow if you put list A in list B and list B in list A, but you wouldn't do that, would you?
        /// </summary>
        /// <param name="array">Some array</param>
        /// <param name="newLines">Set to true if the elements should be separated with a newline</param>
        /// <param name="printFunc">An optional function that you can use in place of ToString</param>
        /// <typeparam name="T">The type of the array</typeparam>
        /// <returns>a pretty printing of the array</returns>
        public static string PrettyPrint<T>(this IEnumerable<T> array, bool newLines = false, Func<T, string> printFunc = null)
        {
            if (array == null)
                return "null";

            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            bool added = false;
            foreach (T t in array)
            {
                added = true;
                if (t == null)
                    builder.Append("null");
                else if (t is IEnumerable<T>)
                    builder.Append(((IEnumerable<T>) t).PrettyPrint());
                else
                {
                    if (printFunc == null)
                        builder.Append(t);
                    else
                        builder.Append(printFunc(t));
                }

                builder.Append(newLines ? "\n " : ", ");
            }

            if (added) //removes the trailing ", "
                builder.Remove(builder.Length - 2, 2);
            builder.Append("]");

            return builder.ToString();
        }

        public static bool ArraysEqual<T>(T[] a1, T[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                var a1Member = a1[i];
                var a2Member = a2[i];

                if (a1Member == null)
                {
                    if (a2Member != null)
                        return false;
                }
                else
                {
                    if (!a1Member.Equals(a2Member))
                        return false;
                }

            }

            return true;
        }
    }
}