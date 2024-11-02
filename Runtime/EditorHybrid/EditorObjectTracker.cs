using System;
using System.Collections.Generic;
using Drboum.Utilities.Runtime.Attributes;
using UnityEngine;

namespace Drboum.Utilities.Runtime.EditorHybrid
{
    /// <summary>
    ///     Detect if a gameobject is being copied or duplicated while identifying it uniquely.
    ///     is used to bind a scene object to an asset for example
    /// </summary>
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class EditorObjectTracker : EditorCallBackMonoBehaviour<EditorObjectTracker>
    {
#if UNITY_EDITOR

        [SerializeField] [InspectorReadOnly] internal GuidWrapper assetInstanceGuid;
        [SerializeField] [InspectorReadOnly] internal int instanceId;
        [SerializeField] [InspectorReadOnly] internal string assetInstanceReadableName;

        internal bool skipDuplication;
        internal bool _duplicate;
        internal bool _created;

        internal List<EventInstanceWrapper> OnDuplicateEvents = new List<EventInstanceWrapper>(4);
        internal List<EventInstanceWrapper> OnCreateComponentEvents = new List<EventInstanceWrapper>(4);
        internal List<GameObjectNameChangedListener> OnGameObjectNameChangedEvents = new List<GameObjectNameChangedListener>(4);

        public GuidWrapper AssetInstanceGuid => assetInstanceGuid;

        internal void _onDuplicate()
        {
            foreach ( EventInstanceWrapper eventInstanceWrapper in OnDuplicateEvents )
            {
                eventInstanceWrapper.Execute(eventInstanceWrapper.Instance);
            }
        }

        internal void _onCreateComponent()
        {
            foreach ( EventInstanceWrapper eventInstanceWrapper in OnDuplicateEvents )
            {
                eventInstanceWrapper.Execute(eventInstanceWrapper.Instance);
            }
        }

        internal void _onGameObjectNameChanged(string oldName)
        {
            foreach ( var eventInstanceWrapper in OnGameObjectNameChangedEvents )
            {
                eventInstanceWrapper.Execute(eventInstanceWrapper.Instance, oldName);
            }
        }
#endif
    }

    public readonly struct GameObjectNameChangedListener : IEquatable<GameObjectNameChangedListener>
    {
        public readonly Component Instance;
        public readonly Action<Component, string> Execute;

        public GameObjectNameChangedListener(Component instance, Action<Component, string> execute)
        {
            Instance = instance;
            Execute = execute;
        }

        public bool Equals(GameObjectNameChangedListener other)
        {
            return Equals(Instance, other.Instance);
        }

        public override bool Equals(object obj)
        {
            return obj is GameObjectNameChangedListener other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Instance != null ? Instance.GetHashCode() : 0);
        }
    }

    public readonly struct EventInstanceWrapper : IEquatable<EventInstanceWrapper>
    {
        public readonly Component Instance;
        public readonly Action<Component> Execute;

        public EventInstanceWrapper(Component instance, Action<Component> execute)
        {
            Instance = instance;
            Execute = execute;
        }

        public bool Equals(EventInstanceWrapper other)
        {
            return Equals(Instance, other.Instance);
        }

        public override bool Equals(object obj)
        {
            return obj is EventInstanceWrapper other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Instance != null ? Instance.GetHashCode() : 0) * 397);
            }
        }
    }
}