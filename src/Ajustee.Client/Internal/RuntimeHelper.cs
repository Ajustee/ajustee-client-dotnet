using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ajustee
{
    internal static class RuntimeHelper
    {
        #region Public methods region

        public static bool IsClassRT(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsClass;
#else
            return type.IsClass;
#endif
        }

        public static IEnumerable<Type> GetInterfacesRT(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().ImplementedInterfaces;
#else
            return type.GetInterfaces();
#endif
        }

        public static IEnumerable<PropertyInfo> GetPropertiesRT(this Type type, bool isStatic = false, bool isPublic = true)
        {
#if NETSTANDARD1_3
            foreach (var _property in type.GetRuntimeProperties())
            {
                var _getter = _property.GetMethod;
                if (_getter != null && _getter.IsStatic == isStatic && _getter.IsPublic == isPublic)
                    yield return _property;
            }
#else
            return type.GetProperties(BindingFlags.GetProperty | (isStatic ? BindingFlags.Static : BindingFlags.Instance) | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic));
#endif
        }

        public static MethodInfo GetMethodRT(this Type type, string methodName, bool isStatic = false, bool isPublic = true, params Type[] types)
        {
#if NETSTANDARD1_3
            var _method = type.GetRuntimeMethod(methodName, types);
            if (_method != null && _method.IsStatic == isStatic && _method.IsPublic == isPublic)
                return _method;
            return null;
#else
            return type.GetMethod(methodName, (isStatic ? BindingFlags.Static : BindingFlags.Instance) | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic), null, types, null);
#endif
        }

        public static ConstructorInfo GetConstructorRT(this Type type, params Type[] types)
        {
#if NETSTANDARD1_3
            foreach (var _constructor in type.GetTypeInfo().DeclaredConstructors)
            {
                var _isFound = false;
                if (_constructor.IsPublic && !_constructor.IsAbstract && !_constructor.IsStatic)
                {
                    var _parameters = _constructor.GetParameters();
                    if (_parameters.Length == types.Length)
                    {
                        _isFound = true;
                        for (int i = 0; i < _parameters.Length; i++)
                        {
                            if (_parameters[i].ParameterType != types[i])
                            {
                                _isFound = false;
                                break;
                            }
                        }
                    }
                }

                if (_isFound)
                    return _constructor;
            }
            return null;
#else
            return type.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, types, null);
#endif
        }

        public static PropertyInfo GetPropertyRT(this Type type, string propertyName, bool isStatic = false, bool isPublic = true)
        {
#if NETSTANDARD1_3
            var _property = type.GetRuntimeProperty(propertyName);
            if (_property != null)
            {
                var _getter = _property.GetMethod;
                if (_getter != null && _getter.IsStatic == isStatic && _getter.IsPublic == isPublic)
                    return _property;
            }
            return null;
#else
            return type.GetProperty(propertyName, (isStatic ? BindingFlags.Static : BindingFlags.Instance) | (isPublic ? BindingFlags.Public : BindingFlags.NonPublic));
#endif
        }

        public static Type[] GetGenericTypeArgumentsRT(this Type type)
        {
#if NETSTANDARD1_3
            return type.GenericTypeArguments;
#else
            return type.GetGenericArguments();
#endif
        }

        public static bool IsGenericTypeRT(this Type type)
        {
#if NETSTANDARD1_3
            return type.GetTypeInfo().IsGenericType;
#else
            return type.IsGenericType;
#endif
        }

        #endregion
    }
}
