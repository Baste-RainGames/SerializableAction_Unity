using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class SerializableMethod
{
    [SerializeField]
    private string name;
    [SerializeField]
    private SerializableSystemType containingType;
    [SerializeField]
    private BindingFlags bindingFlags;
    [SerializeField]
    private bool isGeneric;
    [SerializeField]
    private SerializeableParameterType[] parameterTypes;
    public SerializeableParameterType[] ParameterTypes { get { return parameterTypes; } }

    public SerializableMethod(MethodInfo method)
    {
        methodInfo = method;

        name = method.Name;
        containingType = method.DeclaringType;
        isGeneric = method.IsGenericMethod;

        parameterTypes = ExtractParameterTypes(method, isGeneric);

        bindingFlags =
            (method.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) |
            (method.IsStatic ? BindingFlags.Static : BindingFlags.Instance);

    }

    private SerializeableParameterType[] ExtractParameterTypes(MethodInfo method, bool isGeneric)
    {
        var rawParameters = method.GetParameters();
        var serializedParameters = new SerializeableParameterType[rawParameters.Length];

        for (int i = 0; i < rawParameters.Length; i++)
        {
            serializedParameters[i] = new SerializeableParameterType(rawParameters[i].ParameterType);
        }

        if (isGeneric)
        {
            var genericVersion = method.GetGenericMethodDefinition();
            var genericParams = genericVersion.GetParameters();
            for (int i = 0; i < genericParams.Length; i++)
            {
                serializedParameters[i].IsGeneric = genericParams[i].ParameterType.IsGenericParameter;
            }
        }
        return serializedParameters;
    }

    /// <summary>
    /// @TODO: Replace this with a delegate. See:
    /// http://blogs.msmvps.com/jonskeet/2008/08/09/making-reflection-fly-and-exploring-delegates/
    /// http://stackoverflow.com/questions/10313979/methodinfo-invoke-performance-issue
    /// </summary>
    private MethodInfo methodInfo;
    public MethodInfo MethodInfo
    {
        get
        {
            if (methodInfo == null)
            {
                Type[] types = new Type[parameterTypes.Length];
                for (int i = 0; i < types.Length; i++)
                {
                    types[i] = parameterTypes[i].SystemType;
                }

                var allMethods = containingType.SystemType.GetMethods(bindingFlags).Where(method => method.Name == name);
                foreach (var method in allMethods)
                {
                    if (ParametersMatch(method.GetParameters(), parameterTypes))
                    {
                        methodInfo = method;
                        break;
                    }
                }

                if (methodInfo != null && isGeneric)
                {
                    Type[] generics = (from param in parameterTypes where param.IsGeneric select param.SystemType).ToArray();
                    methodInfo = methodInfo.MakeGenericMethod(generics);
                }

                if (methodInfo == null)
                {
                    Debug.LogError("Failed to find method " + name + " on type " + containingType.Name);
                }
            }
            return methodInfo;
        }
    }

    private bool ParametersMatch(ParameterInfo[] parameters, SerializeableParameterType[] serializedParameters)
    {
        if (parameters.Length != serializedParameters.Length)
            return false;

        for (int i = 0; i < parameters.Length; i++)
        {
            //Special-case for generics, just check that both are marked as generic
            if (parameters[i].ParameterType.IsGenericParameter)
            {
                if (!serializedParameters[i].IsGeneric)
                {
                    return false;
                }
            }
            else
            {
                if (parameters[i].ParameterType != serializedParameters[i].SystemType)
                    return false;
            }
        }

        return true;
    }

    public override string ToString()
    {
        if (MethodInfo == null)
        {
            return "Null method";
        }

        string printData = MethodInfo.Name;

        var paramTypes = ParameterTypes;
        if (paramTypes.Length > 0)
        {
            printData += "[";
            for (var i = 0; i < paramTypes.Length; i++)
            {
                printData += paramTypes[i].Name;
                if (i != paramTypes.Length - 1)
                {
                    printData += ", ";
                }
            }
            printData += "]";
        }
        if (MethodInfo.ReturnType != typeof(void))
        {
            printData += " => " + MethodInfo.ReturnType.Name;
        }
        return printData;
    }

    protected bool Equals(SerializableMethod other)
    {
        return string.Equals(name, other.name) &&
               Equals(containingType, other.containingType) &&
               bindingFlags == other.bindingFlags &&
               isGeneric == other.isGeneric &&
               Util.ArraysEqual(parameterTypes, other.parameterTypes);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != this.GetType())
            return false;
        return Equals((SerializableMethod) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = (name != null ? name.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (containingType != null ? containingType.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (int) bindingFlags;
            hashCode = (hashCode * 397) ^ isGeneric.GetHashCode();
            hashCode = (hashCode * 397) ^ (parameterTypes != null ? parameterTypes.GetHashCode() : 0);
            return hashCode;
        }
    }
}