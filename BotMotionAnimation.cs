using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class BotMotionAnimation : NetworkBehaviour
{
    [SerializeField] private float _assignPositionDuraion = 0.2f;
    [SerializeField] private float _positionDelta = 0.02f;
    private Animator _animator;
    private Transform _avatarTransform;
    private float _lastPositionAssignTime = 0;
    private Vector3 _lastPosition = Vector3.zero;

    private void OnEnable()
    {
        _avatarTransform = transform;
        _animator = GetComponent<Animator>();
    }

    // Use this for initialization
    void Start()
    {

    }

    void FixedUpdate()
    {
        var position = _avatarTransform.position;
        position.y = 0;

        Vector2 motionVector = Vector2.zero;

        var headShift = (position - _lastPosition);
        motionVector = RotateVector(_avatarTransform.eulerAngles.y, new Vector2(headShift.x, headShift.z));
        motionVector.x = motionVector.x < _positionDelta && motionVector.x > -_positionDelta ? 0 : motionVector.x;
        motionVector.y = motionVector.y < _positionDelta && motionVector.y > -_positionDelta ? 0 : motionVector.y;

        _animator.SetFloat(AnimationParameters.MecanimX, motionVector.x);
        _animator.SetFloat(AnimationParameters.MecanimZ, motionVector.y);
        _animator.SetFloat(AnimationParameters.MecanimInputMaginitude, motionVector.x == 0 && motionVector.y == 0 ? 0 : 1);

        if (motionVector.sqrMagnitude > 0)
        {
            var angle = Vector2.Angle(Vector2.up, motionVector);
            if (motionVector.x < 0)
            {
                angle *= -1f;
            }

            _animator.SetFloat(AnimationParameters.MecanimWalkStopAngle, angle);
            _animator.SetFloat(AnimationParameters.MecanimWalkStartAngle, angle);
            _animator.SetFloat(AnimationParameters.MecanimInputAngle, angle);
        }

        _animator.SetBool(AnimationParameters.MecanimStopIfRightFootIsUpInAir, false);
        _animator.SetBool(AnimationParameters.MecanimStopIfLeftFootIsUpInAir, false);
        _animator.SetBool(
            _animator.GetFloat(AnimationParameters.MecanimRightFootUpInAirRatio) > 0.5f ?
            AnimationParameters.MecanimStopIfRightFootIsUpInAir :
            AnimationParameters.MecanimStopIfLeftFootIsUpInAir,
            motionVector.x == 0 && motionVector.y == 0
            );

        if (Time.time - _lastPositionAssignTime > _assignPositionDuraion)
        {
            _lastPositionAssignTime = Time.time;
            _lastPosition = position;
        }
    }

    public Vector2 RotateVector(float angle, Vector2 pt)
    {
        float a = angle * Mathf.PI / 180.0f;
        float cosa = Mathf.Cos(a), sina = Mathf.Sin(a);
        return new Vector2(pt.x * cosa - pt.y * sina, pt.x * sina + pt.y * cosa);
    }
}