using System;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using UnityEngine;

public static unsafe class EntitiesInternalBridge
{
    public static EntityManager GetEntityManager(this IBaker baker)
    {
        return baker._State.World.EntityManager;
    }

    public static void BakeAuthoringWith<TOtherBaker>(this IBaker sourceBaker)
        where TOtherBaker : IBaker, new()
    {
        BakeAuthoringWith(sourceBaker, new TOtherBaker());
    }

    public static void BakeAuthoringWith<TOtherBaker>(this IBaker sourceBaker, TOtherBaker baker)
        where TOtherBaker : IBaker
    {
        baker.InvokeBake(sourceBaker._State);
    }

    public static bool TryGetLinkedGroupEntityBakingData(this EntityManager entityManager, Entity entity, out DynamicBuffer<Entity> linkedGroupEntityBakingDataAsEntity)
    {
        bool hasBuffer = entityManager.HasBuffer<LinkedEntityGroupBakingData>(entity);
        linkedGroupEntityBakingDataAsEntity = hasBuffer
            ? entityManager.GetBuffer<LinkedEntityGroupBakingData>(entity, true).Reinterpret<Entity>()
            : default;
        return hasBuffer;
    }

    public static void AddTransformCompanionComponent(this EntityManager entityManager, Entity entity, UnityObjectRef<Transform> writeTransform, UnityObjectRef<GameObject> transformGameObject)
    {
        entityManager.AddComponent<CompanionLink, CompanionLinkTransform>(entity);
        entityManager.SetComponentData(entity, new CompanionLink {
            Companion = transformGameObject,
        });
        entityManager.SetComponentData(entity, new CompanionLinkTransform {
            CompanionTransform = writeTransform,
        });
    }

    public static void AddTransformCompanionComponent(this EntityManager entityManager, NativeArray<Entity> entities, ReadOnlySpan<UnityObjectRef<Transform>> writeTransforms, ReadOnlySpan<UnityObjectRef<GameObject>> transformGameObjects)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        Assert.AreEqual(entities.Length, writeTransforms.Length);
        Assert.AreEqual(entities.Length, transformGameObjects.Length);
#endif
        entityManager.AddComponent<CompanionLink, CompanionLinkTransform>(entities);
        for ( var index = 0; index < entities.Length; index++ )
        {
            Entity entity = entities[index];
            entityManager.SetComponentData(entity, new CompanionLink {
                Companion = transformGameObjects[index],
            });
            entityManager.SetComponentData(entity, new CompanionLinkTransform {
                CompanionTransform = writeTransforms[index],
            });
        }
    }
}