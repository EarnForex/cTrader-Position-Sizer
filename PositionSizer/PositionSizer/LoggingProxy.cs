using System;
using System.Reflection;

namespace cAlgo.Robots;

public class LoggingProxy<T> : DispatchProxy
{
    private T _decorated;
    private bool _classHasLogAttribute;

    public static T Create(T decorated)
    {
        object proxy = Create<T, LoggingProxy<T>>();
        ((LoggingProxy<T>)proxy)._decorated = decorated;
        ((LoggingProxy<T>)proxy)._classHasLogAttribute = decorated.GetType().GetCustomAttribute<LogAttribute>() != null;
        return (T)proxy;
    }

    protected override object Invoke(MethodInfo targetMethod, object[] args)
    {
        // Check if the target method is a property accessor
        bool isPropertyAccessor = targetMethod.IsSpecialName && (targetMethod.Name.StartsWith("get_") || targetMethod.Name.StartsWith("set_"));

        // Log method calls (skip property accessors)
        bool shouldLogMethod = !isPropertyAccessor && (_classHasLogAttribute || targetMethod.GetCustomAttribute<LogAttribute>() != null);

        if (shouldLogMethod)
        {
            //Logger.RaiseLogEvent($"Calling method {targetMethod.Name}");
        }

        var result = targetMethod.Invoke(_decorated, args);

        if (targetMethod.IsSpecialName && (targetMethod.Name.StartsWith("set_") || targetMethod.Name.StartsWith("get_")))
        {
            string propertyName = targetMethod.Name.Substring(4);
            var propertyInfo = _decorated.GetType().GetProperty(propertyName);
            if (propertyInfo != null && (propertyInfo.GetCustomAttribute<LogAttribute>() != null || _classHasLogAttribute))
            {
                if (targetMethod.Name.StartsWith("set_"))
                {
                    Logger.RaiseLogEvent($"Property {propertyName} value changed to {args[0]}");
                }
            }
        }

        return result;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class LogAttribute : Attribute
{
    
}

public delegate void LogEventHandler(string message);

public static class Logger
{
    public static event LogEventHandler LogEvent;

    public static void RaiseLogEvent(string message)
    {
        LogEvent?.Invoke(message);
    }
}
