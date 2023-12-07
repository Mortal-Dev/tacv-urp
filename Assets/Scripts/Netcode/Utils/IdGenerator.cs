using System;
using System.Collections.Generic;

public class IdGenerator
{
    private ulong currentId;
    private readonly HashSet<ulong> usedIds;
    private readonly Queue<ulong> recycledIds;

    public IdGenerator()
    {
        currentId = 0;
        usedIds = new HashSet<ulong>();
        recycledIds = new Queue<ulong>();
    }

    public ulong GenerateId()
    {
        ulong newId;

        if (recycledIds.Count > 0)
        {
            newId = recycledIds.Dequeue();
        }
        else
        {
            newId = currentId;
            currentId++;
        }

        usedIds.Add(newId);
        return newId;
    }

    public void DisposeId(ulong id)
    {
        if (usedIds.Remove(id))
        {
            recycledIds.Enqueue(id);
        }
    }

    public bool IsIdInUse(ulong id)
    {
        return usedIds.Contains(id);
    }
}