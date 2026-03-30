using UnityEngine;

public class FlankerEnemyController : EnemyBaseController
{
    [SerializeField] private float _fleeThreshold = 0.3f;

    public override void MoveByState()
    {
        if (IsDead || !CanMove)
            return;

        MoveToBackstabPosition(_walkFactor);
    }

    public override void Flee()
    {
        if (IsDead || !CanMove)
            return;

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

    protected override bool ShouldDrag()
    {
        return false;
    }
}