using System;
using System.Diagnostics;
using Drboum.Utilities.EditorHybrid;
using Drboum.Utilities.Interfaces;
using JetBrains.Annotations;
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

}