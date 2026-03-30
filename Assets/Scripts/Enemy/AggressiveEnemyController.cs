using UnityEngine;

public class AggressiveEnemyController : EnemyBaseController
{
    public override void MoveByState()
    {
        if (IsDead || !CanMove)
            return;

        if (HealthPercent > 0.5f)
        {
            MoveDirect(_walkFactor);
            return;
        }

        if (HealthPercent > 0.2f)
        {
            MoveZigZag(_runFactor);
            return;
        }

        MoveDirect(_dragFactor);
    }

    public override void Flee()
    {
        MoveByState();
    }

    public override bool ShouldFlee()
    {
        return false;
    }

    public override bool ShouldRecoverFromFlee()
    {
        return false;
    }

    protected override bool ShouldDrag()
    {
        return !IsDead && HealthPercent <= 0.2f;
    }
}