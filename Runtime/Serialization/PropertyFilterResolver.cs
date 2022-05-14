using System;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;
namespace DrboumLibrary.Serialization {
    public class PropertyFilterResolver : DefaultContractResolver {
        private static readonly Type[] _ignoreDeclaringTypes = {
            typeof(ScriptableObject), typeof(GameObject), typeof(MonoBehaviour), typeof(Behaviour), typeof(Component),
            typeof(Object)
        };
        private static bool IsIgnoreType(MemberInfo member)
        {
            var result = false;
            for ( var i = 0; i < _ignoreDeclaringTypes.Length; i++ ) {
                result = member.DeclaringType == _ignoreDeclaringTypes[i];
                if ( result ) {
                    break;
                }
            }
            return result;
        }
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {

            JsonProperty prop               = base.CreateProperty(member, memberSerialization);
            bool         isGetProperty      = member.MemberType == MemberTypes.Property && !prop.Writable;
            bool         isUobjectRefOnly   = typeof(IUnityObjectReference).IsAssignableFrom(member.DeclaringType);
            bool         getPropRuleIsValid = !isGetProperty;

            if ( !prop.HasMemberAttribute ) {
                prop.Ignored = !getPropRuleIsValid || isUobjectRefOnly || IsIgnoreType(member);
            }

            return prop;
        }
    }
}