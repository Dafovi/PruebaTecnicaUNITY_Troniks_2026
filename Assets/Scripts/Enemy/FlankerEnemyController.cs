using UnityEngine;

public class FlankerEnemyController : EnemyBaseController
{
    [SerializeField] private float _fleeThreshold = 0.3f;

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

        MoveToBackstabPosition(GetChaseMoveFactor());
    }

    public override void Flee()
    {
        if (Stats == null || IsDead)
        {
            return;
        }

        if (!CanMove)
        {
            return;
        }

        MoveAwayFromTarget(_runFactor);
    }

    public override bool ShouldFlee()
    {
        return !IsDead && HealthPercent <= _fleeThreshold;
    }

    public override bool ShouldRecoverFromFlee()
    {
        return HasRecoveredSafeDistance();
    }

    public override void RecoverAndResume()
    {
        base.RecoverAndResume();
    }

    protected override float GetChaseMoveFactor()
    {
        if (IsDead || !CanMove)
            return 0f;

        return _walkFactor;
    }

    protected override bool ShouldDrag()
    {
        return false;
    }
}