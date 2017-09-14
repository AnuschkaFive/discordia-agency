using UnityEngine;

/// <summary>
/// Based on https://www.youtube.com/watch?v=v0zVBtZpB-8&index=5&list=PLFt_AvWsXl0ctd4dgE1F8g3uec4zKNRV0.
/// </summary>
public interface IDamageable
{
    void TakeHit(int damage, RaycastHit2D hit);

    bool IsPlayer();
}
