using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class BotController : NetworkBehaviour
{
	public bool ByPassServerCheck = false;
	private BotMaster _botMaster;
	private Animator _animator;
	private AudioSource _audiosource;
	private CharacterController _characterController;
	private CollisionFlags _collisionFlags;
	private float _dodgeRange = 10;
	private NavMeshAgent _navMeshAgent;
	private NavMeshHit _navHit;
	private bool _firingNow = false;
	private float _dodgeRate = 2f;
	private float _nextDodge;
	private BotNavPause _botNavPause;
	private bool _isFiring = false;
	private bool _isDodging = false;
	private float _verticalvalue, _horizontalvalue;
	private Vector3 _lastPosition = Vector3.zero;
	private bool _isMoving = false;
	private bool _isBotDead = false;
    private FootStepManager _footStepManager;
    private float _lastStepTime = 0;
    private float _stepDuration = 0.36f;


    void Start () {
		_navMeshAgent = GetComponent<NavMeshAgent>();
		_botNavPause = GetComponent<BotNavPause>();
		_nextDodge = -1;
	}

	void OnEnable()
	{
		SetInitialReferences();
		_botMaster.EventEnemyWalk += SetAnimationToMove;
		_botMaster.EventEnemyReachNavTarget += SetAnimationToIdle;
		_botMaster.EventEnemyHealthDeduction += SetAnimationToStruck;
		_botMaster.EventEnemyAttack += SetAnimationToAttack;
		_botMaster.EventEnemyLostTarget += StopAttacking;
		_botMaster.EventEnemyDie += KillBot;
		_botMaster.EventEnemyRespawn += RespawnBot;
		// _botMaster.EventEnemySelectWeapon += weaponselect; //TODO in Network Manager after assignment
	}

	void OnDisable()
	{
		_botMaster.EventEnemyWalk -= SetAnimationToMove;
		_botMaster.EventEnemyReachNavTarget -= SetAnimationToIdle;
		_botMaster.EventEnemyHealthDeduction -= SetAnimationToStruck;
		_botMaster.EventEnemyAttack -= SetAnimationToAttack;
		_botMaster.EventEnemyLostTarget -= StopAttacking;
		_botMaster.EventEnemyDie -= KillBot;
		_botMaster.EventEnemyRespawn -= RespawnBot;
		//_botMaster.EventEnemySelectWeapon -= weaponselect; //TODO in Network Manager after assignment
	}

	private void KillBot()
	{
		_isBotDead = true;
		_botMaster.GetComponent<BotAttack>().enabled = false;
		_botMaster.GetComponent<BotNavWandering>().enabled = false;
		_botMaster.GetComponent<BotDetection>().enabled = false;
		_botMaster.GetComponent<BotNavPursue>().enabled = false;
		_botMaster.GetComponent<BotNavDestinationReched>().enabled = false;
		_botMaster.GetComponent<NavMeshAgent>().enabled = false;
	}

	private void RespawnBot()
	{
		_isBotDead = false;
		_botMaster.GetComponent<NavMeshAgent>().enabled = true;
		_botMaster.GetComponent<BotDetection>().enabled = true;
		_botMaster.GetComponent<BotNavPursue>().enabled = true;
		_botMaster.GetComponent<BotNavDestinationReched>().enabled = true;
		_botMaster.GetComponent<BotNavWandering>().enabled = true;
		_botMaster.GetComponent<BotAttack>().enabled = true;
	}

	private void SetInitialReferences()
	{
		_botMaster = GetComponent<BotMaster>();
		_audiosource = GetComponent<AudioSource>();
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();
		_footStepManager = GetComponentInChildren<FootStepManager>();
	}

	void FixedUpdate () {
		
		if(_isMoving && !_isBotDead)
		{
			HandleMovements();
			HandleFootStepSound();
		}
	}

	void SetAnimationToMove(Vector3 motionVector)
	{
		if (_animator != null && _animator.enabled)
		{
			_lastPosition = transform.position;
			_isMoving = true;
		}
	}

	void SetAnimationToIdle()
	{
		if (_animator != null && _animator.enabled && !_isDodging)
		{
			_isMoving = false;
			_animator.SetFloat("InputMagnitude", 0f);
			_animator.SetBool("Sprint", false);
			_animator.SetBool("IsStopLU", true);
			_animator.SetFloat("VerAimAngle", 15);
			_animator.SetFloat("HorAimAngle", 0);
			_verticalvalue = 0;
			_horizontalvalue = 0;
		}
		else if(_isDodging)
		{
			RandomDodgeTarget();
		}
	}



	void SetAnimationToStruck(int damage)
	{
		if (_animator != null && _animator.enabled && Time.time > _botMaster.nextStruck)
		{
		   
		}
	}

	void StopAttacking(Transform lastSeenTransform)
	{
		_firingNow = false;
	}


	void SetAnimationToAttack(bool isFiring)
	{
		if (_animator != null && _animator.enabled && isFiring)
		{
			_isFiring = isFiring;
			if (_nextDodge < Time.time && !_isDodging)
			{
				_nextDodge = Time.time + _dodgeRate;
				RandomDodgeTarget();
			}
		}
		else if(!isFiring)
		{
			_isDodging = false;
		}
	}

	void RandomDodgeTarget()
	{
		if (!isServer)
		{
			return;
		}

		Vector3 randomPoint;
		do
		{
			randomPoint = transform.position + Random.insideUnitSphere * _dodgeRange;
		}
		while(!(NavMesh.SamplePosition(randomPoint, out _navHit, 10f, GameConstants.WalkableLayer) && Vector3.Distance(transform.position, _navHit.position) > 2));
			_isDodging = true;
			_botMaster.isOnRoute = true;
			_navMeshAgent.SetDestination(_navHit.position);
			_botMaster.CallEventEnemyWalk(_navHit.position);
	}

	private void HandleFootStepSound()
	{
		if (Time.time - _lastStepTime > _stepDuration)
		{
			_footStepManager.PlayFootSteps();
			_lastStepTime = Time.time;
		}
	}


    private void HandleMovements()
    {
        if (!isServer && !ByPassServerCheck)
        {
            return;
        }

        var currentPosition = transform.position;
        Vector3 movDirection = transform.InverseTransformDirection(_navMeshAgent.velocity);
        _horizontalvalue = movDirection.x;
        _verticalvalue = movDirection.z;
        SetDirection(currentPosition);
        _lastPosition = currentPosition;
    }

	void SetDirection(Vector3 input)
	{
		_animator.SetFloat("InputMagnitude", 1f);
		_animator.SetBool("IsStopLU", false);
		_animator.SetFloat("Horizontal", _horizontalvalue);
		_animator.SetFloat("Vertical", _verticalvalue);
		_animator.SetBool("Sprint", false);
	}

	void DisableAnimator()
	{
		if (_animator)
		{
			_animator.enabled = false;
		}
		_audiosource.enabled = false;
		this.enabled = false;
	}

	private void OnAnimatorMove()
	{
		_navMeshAgent.speed = (_animator.deltaPosition / Time.deltaTime).magnitude;
	}
}
