using System;
using Unity.Collections;
using Unity.Entities;

namespace GameProject.EntitiesInjected
{
    public static unsafe class EntitiesPackageBridge
    {
        public static Type GetSystemType(this ref SystemHandle handle, WorldUnmanaged worldUnmanaged)
        {
            return worldUnmanaged.GetTypeOfSystem(handle);
        }
#pragma warning disable EA0016
        public static unsafe T GetManagedSystemInstance<T>(this ref SystemHandle handle, WorldUnmanaged worldUnmanaged, out SystemState systemState)
            where T : ComponentSystemBase
        {
            SystemState* s = worldUnmanaged.ResolveSystemState(handle);
            systemState = *s;
            if ( s != null )
            {
                if ( s->m_ManagedSystem.IsAllocated )
                {
                    return s->m_ManagedSystem.Target as T;
                }
            }
            return null;
        }
#pragma warning restore EA0016
        public static void* GetDynamicComponentPtr(in ArchetypeChunk chunk, ref DynamicComponentTypeHandle componentTypeHandle)
        {
            return chunk.GetDynamicComponentDataPtr(ref componentTypeHandle);
        }

        public static void CopyEntities(EntityManager entityManager, NativeArray<Entity> srcEntities, NativeArray<Entity> outputEntities)
        {
            entityManager.CopyEntitiesInternal(srcEntities, outputEntities);
        }
    }
}