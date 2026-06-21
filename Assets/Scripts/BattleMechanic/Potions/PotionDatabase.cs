// This script handles the lookup table from PotionId to PotionData asset.
// Made by Vonce Chew 

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A ScriptableObject holding every PotionData asset, looked up by id.
/// </summary>
[CreateAssetMenu(menuName = "Zaria/Potion Database")]
public class PotionDatabase : ScriptableObject
{
    public List<PotionData> potions = new List<PotionData>();

    private Dictionary<PotionId, PotionData> _lookup;

    /// <summary>
    /// Build the lookup once, on first use.
    /// </summary>
    private void EnsureBuilt()
    {
        if (_lookup != null) return;
        _lookup = new Dictionary<PotionId, PotionData>();
        foreach (var p in potions)
            if (p != null) _lookup[p.id] = p;
    }

    /// <summary>
    /// Find the data for a potion id. Returns null and warns if the
    /// asset isn't in the list, rather than throwing.
    /// </summary>
    /// <param name="id">The potion to look up.</param>
    public PotionData Get(PotionId id)
    {
        EnsureBuilt();
        if (_lookup.TryGetValue(id, out var data)) return data;
        Debug.LogWarning($"PotionDatabase has no entry for {id}.");
        return null;
    }
}