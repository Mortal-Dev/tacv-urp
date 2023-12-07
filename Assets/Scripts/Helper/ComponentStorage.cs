using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

public static class ComponentStorage
{
    static readonly Dictionary<Entity, List<IComponentData>> componentStorage = new();

    public static T GetComponentFromStorage<T>(Entity entity, bool removeComponentFromStorage) where T : IComponentData
    {
        if (!componentStorage.TryGetValue(entity, out List<IComponentData> entityComponents))
            return default;

        T componentData = (T)entityComponents.FirstOrDefault(x => x.GetType().Equals(typeof(T)));

        if (removeComponentFromStorage) componentStorage[entity].Remove(componentData);

        if (entityComponents.Count == 0) componentStorage.Remove(entity);

        return componentData;
    }

    public static void AddComponentToStorage<T>(Entity entity, T component) where T : IComponentData
    {
        if (componentStorage.TryGetValue(entity, out List<IComponentData> components))
        {
            components.Add(component);
            return;
        }

        componentStorage.Add(entity, new List<IComponentData> { component });
    }
}