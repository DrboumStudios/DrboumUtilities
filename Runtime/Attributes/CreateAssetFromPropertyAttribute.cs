using System;
using System.Diagnostics;
using System.IO;
using Drboum.Utilities.Interfaces;
using JetBrains.Annotations;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.Attributes
{
    /// <summary>
    /// Adds a create button for ScriptableObject fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    [Conditional("UNITY_EDITOR")]
    public class CreateAssetFromPropertyAttribute : PropertyAttribute
    {
        private readonly Type _customSavePersistentAssetType;
        private readonly Type _customAssetCreatorType;

        public CreateAssetFromPropertyAttribute() : this(typeof(DefaultCreateScriptableObjectInstance), typeof(DefaultSavePersistentAsset))
        { }

        public CreateAssetFromPropertyAttribute(Type createAndConfigureType) : this(typeof(ICreateAsset).IsAssignableFrom(createAndConfigureType) ? createAndConfigureType : typeof(DefaultCreateScriptableObjectInstance), typeof(ISavePersistentAsset).IsAssignableFrom(createAndConfigureType) ? createAndConfigureType : null)
        { }

        public CreateAssetFromPropertyAttribute(Type creatorType, [CanBeNull] Type configureType)
        {
            _customAssetCreatorType = ValidateType(creatorType, typeof(DefaultCreateScriptableObjectInstance), typeof(ICreateAsset));
            _customSavePersistentAssetType = ValidateType(configureType, null, typeof(ISavePersistentAsset));
        }

        private static Type ValidateType(Type validateType, [CanBeNull] Type defaultInstanceType, Type interfaceType)
        {
            if ( validateType == null )
                return defaultInstanceType;

            if ( interfaceType.IsAssignableFrom(validateType) )
                return validateType;

            var s = $"The provided type {validateType.Name} must implement the interface {interfaceType.Name}.";
            if ( defaultInstanceType != null )
            {
                s += $" A default implementation of type {defaultInstanceType.Name} will be provided";
            }
            else
            {
                s += $" Please provide a custom implementation";
            }

            LogHelper.LogErrorMessage(s, nameof(CreateAssetFromPropertyAttribute));
            return defaultInstanceType;
        }

        public ICreateAsset GetInstanceCreator(Object parentObject)
        {
            return GetCustomImplementationIfNotDefault<ICreateAsset, DefaultCreateScriptableObjectInstance>(_customAssetCreatorType, parentObject);
        }

        public ISavePersistentAsset GetConfigurePersistentAsset(Object parentObject)
        {
            return GetCustomImplementationIfNotDefault<ISavePersistentAsset, DefaultSavePersistentAsset>(_customSavePersistentAssetType, parentObject);
        }

        public static TInterface GetCustomImplementationIfNotDefault<TInterface, TDefaultImplementation>(Type customAssetCreatorType, Object parentObject)
            where TDefaultImplementation : TInterface
        {
            return customAssetCreatorType == typeof(TDefaultImplementation) && parentObject is TInterface parentCreateAsset
                ? parentCreateAsset
                : (TInterface)Activator.CreateInstance(customAssetCreatorType);
        }
    }

    public struct DefaultCreateScriptableObjectInstance : ICreateAsset
    {
        public bool CanCreateAsset(Object parentObject, Type type)
        {
#if UNITY_EDITOR
            return typeof(ScriptableObject).IsAssignableFrom(type);
#else
            return true;
#endif
        }

        public Object CreateInstance(Object parentObject, Type type)
        {
#if UNITY_EDITOR
            switch ( type )
            {
                case var t when typeof(ScriptableObject).IsAssignableFrom(t):
                {
                    return CreatedInstanceImpl(parentObject, type);
                }

                default:
                    throw new NotSupportedException($"Default implementation of {nameof(ICreateAsset)} does not support creating instances of {type.Name}. Please provide a custom implementation.");
            }
#else
            return CreatedInstanceImpl(parentObject, type);
#endif
        }

        private static Object CreatedInstanceImpl(Object parentObject, Type type)
        {
            var createdInstance = ScriptableObject.CreateInstance(type);
            createdInstance.name = $"{parentObject.name}_{createdInstance.GetType().Name}";
            return createdInstance;
        }
    }

    public struct DefaultSavePersistentAsset : ISavePersistentAsset
    {
        public void SaveAsset(Object parentObject, Object createdInstance)
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(parentObject);
            SaveCreatedInstanceToDatabase(parentObject, createdInstance, Path.GetDirectoryName(assetPath));
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void SaveCreatedInstanceToDatabase<T>(Object parentObject, T newInstance, string assetPath, string fileExtension = "asset")
            where T : Object
        {
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(newInstance, Path.Combine(assetPath, $"{newInstance.name}.{fileExtension}"));
            EditorUtility.SetDirty(parentObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(newInstance);
#endif
        }
    }
}