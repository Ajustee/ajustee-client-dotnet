using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;
using System.Reflection;
using System.Linq.Expressions;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Ajustee
{
    internal static class Helper
    {
        #region Private fields region

        private const string m_ConfigurationKeysUrlTemplate = "configurationKeys?path={0}";
        private const string m_WebSocketSchema = "wss";

        private static readonly ConcurrentDictionary<Type, Func<object, IList<KeyValuePair<string, string>>>> m_ReflecteMethodCache = new ConcurrentDictionary<Type, Func<object, IList<KeyValuePair<string, string>>>>();

        #endregion

        #region Public fields region

        public const string AppIdName = "x-api-key";
        public const string KeyPathName = "x-key-path";
        public const string KeyPropsName = "x-key-props";
        public const string TrackerIdName = "ajustee-tracker-id";
        public static readonly IJsonSerializer JsonSerializer = JsonSerializerFactory.Create();

        #endregion

        #region Public methods region

        public static string ValidateAndGetPropertyName(object name)
        {
            if (name is string _name) return _name;
            throw new InvalidCastException("Property value should be string");
        }

        public static string FormatPropertyValue(object value)
        {
            if (value is string _str) return _str;
            if (value is null) return string.Empty;
            return string.Format(CultureInfo.InvariantCulture, "{0}", value);
        }

        public static Uri GetConfigurationKeysUrl(Uri baseUri, string keyPath)
        {
            if (baseUri.AbsoluteUri.EndsWith("/"))
                baseUri = new Uri(baseUri.AbsoluteUri.TrimEnd('/'));
            return new Uri(baseUri, string.Format(m_ConfigurationKeysUrlTemplate, keyPath));
        }

        public static Uri GetSubscribeUrl(Uri baseUri)
        {
//#if DEBUG
//            //return new Uri("wss://viz8masph1.execute-api.us-west-2.amazonaws.com/demo");
//            return new Uri("wss://qlnzq5smse.execute-api.us-west-2.amazonaws.com/demo");
//#else
            var _uriBuilder = new UriBuilder(baseUri);
            _uriBuilder.Scheme = m_WebSocketSchema;// Sets websocket secure schema
            return _uriBuilder.Uri;
//#endif
        }

        public static void ValidateProperties(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties != null)
            {
                foreach (var _property in properties)
                {
                    if (string.IsNullOrEmpty(_property.Key))
                        throw Error.InvalidPropertyName(_property.Key);

                    if (string.Equals(AppIdName, _property.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedPropertyName(_property.Key);

                    if (string.Equals(KeyPathName, _property.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedPropertyName(_property.Key);

                    if (string.Equals(KeyPropsName, _property.Key, StringComparison.OrdinalIgnoreCase))
                        throw Error.ReservedPropertyName(_property.Key);
                }
            }
        }

        public static IEnumerable<KeyValuePair<string, string>> GetMergedProperties(params IEnumerable<KeyValuePair<string, string>>[] properties)
        {
            return properties.Where(ps => ps != null).SelectMany(ps => ps).GroupBy(ps => ps.Key, ps => ps.Value).Select(g => new KeyValuePair<string, string>(g.Key, g.First()));
        }

        public static IList<KeyValuePair<string, string>> ReflectProperties(object obj)
        {
            IList<KeyValuePair<string, string>> _list = null;

            if (obj is IEnumerable<KeyValuePair<string, string>> _enumDirect)
            {
                _list = _enumDirect.ToArray();
            }
            else if (obj is IDictionary _dic)
            {
                _list = _dic.Cast<DictionaryEntry>().Select(e => new KeyValuePair<string, string>(ValidateAndGetPropertyName(e.Key), FormatPropertyValue(e.Value))).ToArray();
            }
            else if (obj is IEnumerable _enum)
            {
                var _type = obj.GetType();

                if (_type.GetInterfacesRT().Any(t => IsStringKeyedValuePairEnumarable(t)))
                {
                    _list = new List<KeyValuePair<string, string>>();

                    PropertyInfo _keyProp = null, _valueProp = null;
                    foreach (var _entry in _enum)
                    {
                        _keyProp ??= _entry?.GetType().GetPropertyRT("Key");
                        _valueProp ??= _entry?.GetType().GetPropertyRT("Value");
                        _list.Add(new KeyValuePair<string, string>(ValidateAndGetPropertyName(_keyProp.GetValue(_entry, null)), FormatPropertyValue(_valueProp.GetValue(_entry, null))));
                    }
                }

                static bool IsStringKeyedValuePairEnumarable(Type t)
                {
                    if (t.IsGenericTypeRT() && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        t = t.GetGenericTypeArgumentsRT()[0];
                        if (t.IsGenericTypeRT() && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                        {
                            var _typeArgs = t.GetGenericTypeArgumentsRT();
                            return _typeArgs?.Length == 2 && _typeArgs[0] == typeof(string);
                        }
                    }
                    return false;
                }
            }
            else if (obj?.GetType() is Type _type && _type.IsClassRT())
            {
                _list = m_ReflecteMethodCache.GetOrAdd(_type, t =>
                {
                    var _exps = new List<Expression>();
                    var _validateAndGetPropertyNameMethod = typeof(Helper).GetMethodRT(nameof(ValidateAndGetPropertyName), isStatic: true, isPublic: false, typeof(object));
                    var _formatPropertyValueMethod = typeof(Helper).GetMethodRT(nameof(FormatPropertyValue), isStatic: true, isPublic: false, typeof(object));
                    var _listAddMethod = typeof(List<KeyValuePair<string, string>>).GetMethodRT(nameof(List<object>.Add), types: typeof(KeyValuePair<string, string>));
                    var _listContructor = typeof(List<KeyValuePair<string, string>>).GetConstructorRT();
                    var _keyValuePairContructor = typeof(KeyValuePair<string, string>).GetConstructorRT(typeof(string), typeof(string));
                    var _returnType = typeof(IList<KeyValuePair<string, string>>);

                    // Gets type properties to proceed.
                    var _properties = t.GetPropertiesRT();

                    // CODE:
                    //  Func(object obj)
                    var _objParameter = Expression.Parameter(typeof(object), "obj");

                    // CODE:
                    //  _obj = (ObjectType)obj;
                    var _objVarExp = Expression.Variable(t, "_obj");
                    _exps.Add(Expression.Assign(_objVarExp, Expression.Convert(_objParameter, t)));

                    // CODE:
                    //  _list = new List<KeyValuePair<string, string>>();
                    var _listVarExp = Expression.Variable(typeof(List<KeyValuePair<string, string>>), "_list");
                    _exps.Add(Expression.Assign(_listVarExp, Expression.New(_listContructor)));

                    // Enumrated for each properties to add to list.
                    foreach (var _property in _properties)
                    {
                        // CODE:
                        //  _list.Add(new KeyValuePair<string, string>(ValidateAndGetPropertyName(PropertyName), HelperFormatPropertyValue(_obj.PropertyName)));
                        _exps.Add(Expression.Call(_listVarExp, _listAddMethod,
                            Expression.New(_keyValuePairContructor,
                                Expression.Call(_validateAndGetPropertyNameMethod, Expression.Constant(_property.Name, typeof(object))),
                                Expression.Call(_formatPropertyValueMethod, Expression.Convert(Expression.Property(_objVarExp, _property), typeof(object))))));
                    }

                    // CODE:
                    //  return _list;
                    _exps.Add(Expression.Convert(_listVarExp, _returnType));

                    return Expression.Lambda<Func<object, IList<KeyValuePair<string, string>>>>(Expression.Block(_returnType, new[] { _objVarExp, _listVarExp }, _exps), _objParameter).Compile();
                }
                )(obj);
            }

            return _list ?? new KeyValuePair<string, string>[0];
        }

        #endregion
    }
}
