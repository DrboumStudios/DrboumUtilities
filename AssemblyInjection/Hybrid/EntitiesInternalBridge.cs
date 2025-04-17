using System;
using System.Runtime.CompilerServices;
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

    public static T GetComponentDataAs<T>(this EntityManager entityManager, Entity entity, ComponentType type)
        where T : unmanaged
    {
        Assert.AreEqual(sizeof(T), TypeManager.GetTypeInfo(type.TypeIndex).TypeSize);
        var access = entityManager.GetUncheckedEntityDataAccess();
        var data = access->EntityComponentStore->GetComponentDataWithTypeRO(entity, type.TypeIndex);
        return *(T*)data;
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
            SetTransformCompanionComponent(entityManager, entities[index], writeTransforms[index], transformGameObjects[index]);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTransformCompanionComponent(this EntityManager entityManager, Entity entity, UnityObjectRef<Transform> writeTransform, UnityObjectRef<GameObject> transformGameObject)
    {
        SetCompanionLinkComponent(entityManager, entity, transformGameObject);
        entityManager.SetComponentData(entity, new CompanionLinkTransform {
            CompanionTransform = writeTransform,
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetCompanionLinkComponent(this EntityManager entityManager, Entity entity, UnityObjectRef<GameObject> companion)
    {
        entityManager.SetComponentData(entity, new CompanionLink {
            Companion = companion,
        });
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentType GetCompanionLinkComponent(this EntityManager entityManager)
    {
        return ComponentType.ReadWrite<CompanionLink>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetTransformCompanionComponentTypes(this EntityManager entityManager, ref FixedList128Bytes<ComponentType> buffer)
    {
        buffer.Add(ComponentType.ReadWrite<CompanionLink>());
        buffer.Add(ComponentType.ReadWrite<CompanionLinkTransform>());
    }
}