using UnityEngine;

[RequireComponent(typeof(EnemyBaseController))]
public class EnemyStateMachine : MonoBehaviour
{
    private EnemyBaseController _enemyController;
    private EnemyState _currentState;

    public EnemyState CurrentState => _currentState;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyBaseController>();
    }

    private void Start()
    {
        SetState(EnemyState.Chase);
    }

    public void Tick()
    {
        if (_enemyController == null || _enemyController.Stats == null)
            return;

        if (_enemyController.IsDead)
        {
            SetState(EnemyState.Dead);
        }

        switch (_currentState)
        {
            case EnemyState.Chase:
                TickChase();
                break;

            case EnemyState.Attack:
                TickAttack();
                break;

            case EnemyState.Flee:
                TickFlee();
                break;

            case EnemyState.Dead:
                break;
        }
    }

    public void SetState(EnemyState newState)
    {
        _currentState = newState;
    }

    private void TickChase()
    {
        if (_enemyController.ShouldFlee())
        {
            SetState(EnemyState.Flee);
            return;
        }

        if (_enemyController.IsTargetInAttackRange())
        {
            SetState(EnemyState.Attack);
            return;
        }

        _enemyController.MoveByState();
    }

    private void TickAttack()
    {
        if (_enemyController.ShouldFlee())
        {
            SetState(EnemyState.Flee);
            return;
        }

        if (!_enemyController.IsTargetInAttackRange())
        {
            SetState(EnemyState.Chase);
            return;
        }

        _enemyController.StopMovingAndFaceTarget();
        _enemyController.Attack();
    }

    private void TickFlee()
    {
        if (_enemyController.IsDead)
        {
            SetState(EnemyState.Dead);
            return;
        }

        if (_enemyController.ShouldRecoverFromFlee())
        {
            _enemyController.RecoverAndResume();
            SetState(EnemyState.Chase);
            return;
        }

        _enemyController.Flee();
    }
}