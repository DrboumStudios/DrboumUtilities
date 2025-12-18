using System;
using System.Diagnostics;
using Object = UnityEngine.Object;

namespace Drboum.Utilities.Interfaces
{
    public interface ICreateAsset
    {
        bool CanCreateAsset(Object parentObject, Type type);
        Object CreateInstance(Object parentObject, Type type);
    }
}