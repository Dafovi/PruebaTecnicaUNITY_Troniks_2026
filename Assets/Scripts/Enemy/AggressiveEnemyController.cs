using UnityEngine;

public class AggressiveEnemyController : EnemyBaseController
{
    public override void MoveByState()
    {
        if (Stats == null || IsDead)
        {
            return;
        }

        if (!CanMove)
        {
            return;
        }

        float moveFactor = GetChaseMoveFactor();

        if (HealthPercent > 0.5f)
        {
            MoveDirect(moveFactor);
            return;
        }

        if (HealthPercent > 0.2f)
        {
            MoveZigZag(moveFactor);
            return;
        }

        MoveDirect(moveFactor);
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

    protected override float GetChaseMoveFactor()
    {
        if (IsDead || !CanMove)
            return 0f;

        if (HealthPercent > 0.5f)
            return _walkFactor;

        if (HealthPercent > 0.2f)
            return _runFactor;

        return _dragFactor;
    }

    protected override bool ShouldDrag()
    {
        return !IsDead && HealthPercent <= 0.2f;
    }
}