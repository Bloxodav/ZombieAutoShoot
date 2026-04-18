using System.Collections.Generic;
using UnityEngine;

public static class ZombieFactionRegistry
{
    private static readonly List<ZombieAI> _enemies = new List<ZombieAI>(64);
    private static readonly List<ZombieAI> _allies = new List<ZombieAI>(16);

    public static void Register(ZombieAI z)
    {
        if (z.Faction == ZombieFaction.Enemy)
        {
            if (!_enemies.Contains(z)) _enemies.Add(z);
        }
        else
        {
            if (!_allies.Contains(z)) _allies.Add(z);
        }
    }

    public static void Unregister(ZombieAI z)
    {
        _enemies.Remove(z);
        _allies.Remove(z);
    }

    public static Transform GetNearestEnemy(Transform from)
        => GetNearest(_enemies, from);

    public static Transform GetNearestAlly(Transform from)
        => GetNearest(_allies, from);

    private static Transform GetNearest(List<ZombieAI> list, Transform from)
    {
        Transform best = null;
        float bestDist = float.MaxValue;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i] == null || !list[i].gameObject.activeInHierarchy || list[i].IsDead)
            {
                list.RemoveAt(i);
                continue;
            }

            float d = (list[i].transform.position - from.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = list[i].transform;
            }
        }

        return best;
    }

    public static IReadOnlyList<ZombieAI> Enemies => _enemies;
    public static IReadOnlyList<ZombieAI> Allies => _allies;

    public static void Clear()
    {
        _enemies.Clear();
        _allies.Clear();
    }
}