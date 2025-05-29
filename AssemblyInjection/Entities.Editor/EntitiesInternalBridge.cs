using UnityEditor;
using UnityEngine;

namespace Unity.Entities.Editor
{
    public static class EntitiesInternalBridge
    {
        public struct EntityWorldInfo
        {
            public Entity Entity;
            public World World;
            public ScriptableObject EntitySelectionProxy;
        }

        public static bool TryGetEntityWorldInfoFromSelectionContext(out EntityWorldInfo entityWorldInfo)
        {
            bool getEntitySelectionProxy = TryGetEntitySelectionProxy(out EntitySelectionProxy entitySelectionProxy);
            if ( getEntitySelectionProxy )
            {
                entityWorldInfo = new EntityWorldInfo {
                    Entity = entitySelectionProxy.Entity,
                    World = entitySelectionProxy.World,
                    EntitySelectionProxy = entitySelectionProxy
                };
            }
            else
            {
                entityWorldInfo = default;
            }
            return getEntitySelectionProxy;
        }

        private static bool TryGetEntitySelectionProxy(out EntitySelectionProxy entitySelectionProxy)
        {
            return (entitySelectionProxy = (Selection.activeObject as EntitySelectionProxy)) || (entitySelectionProxy = Selection.activeContext as EntitySelectionProxy);
        }
    }
}