using UnityEngine;

namespace Game.EntityMovement
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] protected GameObject _rotationTarget;
        [SerializeField] float _rotationFix = 0f;

        protected Rigidbody _rb;
        protected CapsuleCollider _collider;

        // Inputs
        protected Vector3 _moveDirection;
        protected bool _dashInput = false;

        // Knockback
        [SerializeField] public bool DebugKnockbacImmunity = false;
        protected bool _applyingKnockBack = false;
        protected Vector3 _knockDirection;
        protected float _knockSpeed;

        protected Vector3 MovingDirection => _moveDirection;
        protected Vector3 _inputDirection;

        //protected bool _touchingObstacle = false;
        //protected Vector2 _lastValidPosition = Vector2.zero;
        protected Vector3 _lastNonZeroInput = Vector3.zero;

        //protected List<TimeLeftClock> _dashChargeTimers = new List<TimeLeftClock>();
        //protected TimeLeftClock _dashTimer;
        protected int _currentDashes = 0;

        protected bool _okToMove = true;

        //public ActionSignal Dash { get; } = new();
        //public ActionSignal DashReady { get; } = new();

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            // TODO: Not all oweners are characters
            //_owner = GetComponent<Character>();

            //if (_owner?.tag == "Player")
            //    _usesPhysicsMovement = true;

            _lastNonZeroInput = -transform.forward;

            //var handler = UnityHelpers.GetOrAdd<SubscriptionManagerHandler>(gameObject);
            //handler.SubscribeForGameTicks(this, TickGroup.Movement);
        }

        //protected virtual void Start()
        //{
        //    var handler = UnityHelpers.GetOrAdd<SubscriptionManagerHandler>(gameObject);

        //    // NOTE: Is it possible to have 0 at the start?
        //    if (_stats.DashChargesMax > 0)
        //    {
        //        //for (int i = 0; i < (int)_stats.DashChargesMax; ++i)
        //        //    _dashChargeTimers.Add(new TimeLeftClock(_stats.DashCooldown, false, true));

        //        _dashTimer = new TimeLeftClock(_stats.DashCooldown, false, true);
        //        _currentDashes = (int)_stats.DashChargesMax;

        //        handler.Subscribe(_stats.DashChargesObs, ReactToDashChargesChange);
        //        handler.Subscribe(_stats.DashCooldownObs, ReactToDashCooldownChange);
        //    }

        //    handler.Subscribe(_stats.CharacterStatusObs, ReactToCharacterStatus);
        //}

        protected virtual void Update()
        {
            MoveCharacter();
            RotateCharacter(_moveDirection, 0.01f);
        }

        //public void OnGameTick()
        //{
        //    TickUpdate();
        //    //Debug.DrawRay(_rotationTarget.transform.position, ModelFacingDirection() * 2f, Color.green, 2f);
        //}

        //protected virtual void TickUpdate()
        //{
        //    if (_stats == null)
        //    {
        //        Log.Error("CharacterMovement: Stats can't be null. Disabling update!");
        //        enabled = false;
        //        return;
        //    }

        //    if (_stats.IsAlive)
        //    {
        //        CheckDashCooldown();

        //        if (_dashInput)
        //        {
        //            TryToDash();

        //            // Make sure that dash moves character
        //            //if (_stats.IsDashing && _moveDirection == Vector3.zero)
        //                _moveDirection = _lastNonZeroInput;
        //        }

        //        _stats.ChangeMovingSpeed(_moveDirection.sqrMagnitude);

        //        MoveCharacter();

        //        //if (!_applyingKnockBack)
        //        //RotateCharacter();

        //        //_lastValidPosition = transform.position;

        //        if (_moveDirection != Vector3.zero)
        //        {
        //            //if (!_stats.IsDashing)
        //                RotateCharacter(_moveDirection, _stats.RotationSpeed);

        //            //transform.forward = _moveDirection;
        //            //Debug.DrawLine(transform.position, transform.position + _moveDirection, Color.red, 1.0f);
        //        }

        //        // NOTE: 3D Change // Relevant on 3d?
        //        //if (_moveDirection != Vector2.zero)
        //        //if (_inputDirection != Vector3.zero)
        //        //{
        //        //    Vector2 up = (Vector2)transform.position + Vector2.up - (Vector2)transform.position;
        //        //    Vector2 currentDir = (Vector2)transform.position + _inputDirection - (Vector2)transform.position;

        //        //    //Debug.DrawLine(transform.position, (Vector2)transform.position + up * 5.0f, Color.red, 1.0f);
        //        //    //Debug.DrawLine(transform.position, (Vector2)transform.position + currentDir * 5.0f, Color.blue, 1.0f);

        //        //    //float angleDifference = Vector3.Angle(currentDir, up);
        //        //    float relativeAngleDifference = Vector2.SignedAngle(currentDir, up);

        //        //    if (relativeAngleDifference < 0)
        //        //        relativeAngleDifference += 360f;

        //        //    //Debug.Log(relativeAngleDifference);

        //        //    LastDirectionAngleObs.Value = relativeAngleDifference;
        //        //}
        //    }
        //}

        protected virtual void MoveCharacter()
        {
            //if (_applyingKnockBack)
            //{
            //    // NOTE: Tarkista toimiiko knockback kun semmoinen tulee pelaajaan.
            //    // EnemyMovement hoitaa vihollisten knockbackin erilailla
            //    _rb.velocity = (_moveDirection * _stats.CurrentMovingSpeed) + (_knockDirection * _knockSpeed);
            //}
            //else
            //{
            //    _rb.velocity = _moveDirection * _stats.CurrentMovingSpeed;
            //}
            //return;

            Vector3 movementPerTick;
            float movingSpeed = 10f;

            if (_applyingKnockBack)
            {
                // Total knockback-influenced movement this tick
                movementPerTick = (_moveDirection * movingSpeed) + (_knockDirection * _knockSpeed);
            }
            else
            {
                movementPerTick = _moveDirection * movingSpeed;
            }

            //Debug.Log(movementPerTick);

            // Scale movement by tick interval to maintain correct speed (distance = speed * time)
            float delta = Time.deltaTime; // GameCore.TickManager.TickInterval;
            Vector3 newPosition = _rb.position + movementPerTick * delta;

            _rb.MovePosition(newPosition);

            //if (_touchingObstacle && _moveDirection != Vector3.zero)
            //{
            //    // NOTE: Hahmo koskee esteeseen. Korjataan hahmon liike niin, ett� se
            //    // alkaa liikkumaan esteen suuntaisesti.

            //    //Debug.DrawRay(transform.position, _rb.velocity.normalized, Color.green, 5);
            //    LayerMask mask = LayerMask.GetMask("MapBounds");
            //    RaycastHit2D hit = Physics2D.Raycast(transform.position, _rb.velocity.normalized, 1, mask);
            //    if (hit.collider != null) /*, _rb.velocity.magnitude * Time.fixedDeltaTime))*/
            //    {
            //        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("MapBounds"))
            //        {
            //            // Find the line from the gun to the point that was clicked.
            //            //Vector3 incomingVec = hit.point - (Vector2)transform.position;

            //            //Use the point's normal to calculate the reflection vector.
            //            //Vector3 reflectVec = Vector3.Reflect(incomingVec, hit.normal);

            //            // Draw lines to show the incoming "beam" and the reflection.
            //            //Debug.DrawLine(transform.position, hit.point, Color.red, 10);
            //            //Debug.DrawRay(hit.point, reflectVec, Color.blue, 10);

            //            Vector3 incomingVec = hit.point - (Vector2)transform.position;

            //            // Esteen suuntainen vektori
            //            Vector3 vec = incomingVec - Vector3.Dot(incomingVec, (Vector3)hit.normal) * (Vector3)hit.normal;
            //            _rb.velocity = vec.normalized * _stats.CurrentMovingSpeed;
            //        }
            //    }
            //}
        }

        protected virtual void RotateCharacter(Vector3 moveDirection, float rotationSpeed)
        {
            if (moveDirection.sqrMagnitude < 0.001f)
                return;

            // Desired facing rotation
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            targetRotation *= Quaternion.Euler(0, _rotationFix, 0);

            // Remove tilt on X and Z
            Vector3 euler = targetRotation.eulerAngles;
            euler.x = 0f;
            euler.z = 0f;
            targetRotation = Quaternion.Euler(euler);

            // Calculate current angle difference
            float angleDiff = Quaternion.Angle(_rotationTarget.transform.rotation, targetRotation);
            const float rotationThreshold = 0.5f; // degrees — how close counts as "facing the right way"

            // Skip rotation if already facing close enough
            if (angleDiff < rotationThreshold)
                return;

            // Smooth rotation per frame — uses deltaTime to keep consistent speed
            _rotationTarget.transform.rotation = Quaternion.RotateTowards(
                _rotationTarget.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        public void SetMovementInput(Vector3 input)
        {
            if (_okToMove)
            {
                //if (!_stats.IsDashing && !_applyingKnockBack)
                //    _moveDirection = input;

                _moveDirection = input;

                if (_inputDirection != Vector3.zero)
                    _lastNonZeroInput = _inputDirection;

                _inputDirection = input;
            }
        }

        protected void SetDashInput()
        {
            if (_okToMove)
            {
                _dashInput = true;
            }
        }

        //public void RemoveKnockBack()
        //{
        //    // There can be multiple effects applying effect
        //    //if (!_stats.IsEffectedByStatusEffect(Status.KnockBack))
        //    if(!_owner.IsEffectedByBuff(BuffType.Knockback))
        //        _applyingKnockBack = false;
        //}

        public Vector3 GetCurrentMovingDirection()
        {
            return MovingDirection;
        }

        //public float DashCooldownRemaining()
        //{
        //    if (_dashTimer != null)
        //        return 1 - _dashTimer.PercentagePassed();
        //    else
        //        return 0;

        //    //TimeLeftClock oldestTimer = null;
        //    //for (int i = 0; i < _dashChargeTimers.Count; ++i)
        //    //{
        //    //    if(oldestTimer == null || oldestTimer.IsTimeOver())
        //    //        oldestTimer = _dashChargeTimers[i];
        //    //    else
        //    //    {
        //    //        if(!_dashChargeTimers[i].IsTimeOver() && _dashChargeTimers[i].PercentagePassed() > oldestTimer.PercentagePassed())
        //    //            oldestTimer = _dashChargeTimers[i];
        //    //    }
        //    //}

        //    //if(oldestTimer != null)
        //    //    return 1 - oldestTimer.PercentagePassed();
        //    //else
        //    //    return 0;
        //}

        protected virtual void TryToDash()
        {
            // override
        }

        //protected virtual void CheckDashCooldown()
        //{
        //    if (_currentDashes < _stats.DashChargesMax)
        //    {
        //        if (_dashTimer.IsTimeOver())
        //        {
        //            ++_currentDashes;
        //            DashReady.Invoke();

        //            if (_currentDashes < _stats.DashChargesMax)
        //                _dashTimer.ResetTimer();
        //        }
        //    }
        //}

        void ReactToDashChargesChange(float oldValue, float newValue)
        {
            //for (int i = _dashChargeTimers.Count; i < newValue; ++i)
            //    _dashChargeTimers.Add(new TimeLeftClock(_stats.DashCooldown, false, true));

            if(oldValue < newValue)
                ++_currentDashes;
            else if (oldValue > newValue)
                --_currentDashes;
        }

        void ReactToDashCooldownChange(float oldValue, float newValue)
        {
            //for (int i = 0; i < _dashChargeTimers.Count; ++i)
            //    _dashChargeTimers[i].ChangeTimeToTrack(newValue, false);

            //Debug.Log("Dash cooldown is now: " + newValue);

            //_dashTimer.ChangeTimeToTrack(newValue, false);
        }


        public Vector3 ModelFacingDirection()
        {
            if (_rotationTarget == null) return Vector3.forward;

            // Flip the rotationFix sign
            Quaternion rot = _rotationTarget.transform.rotation * Quaternion.Euler(0f, -_rotationFix, 0f);

            return rot * Vector3.forward;
        }

        protected Vector3 GetCameraRelativeDirection(Vector3 inputDir)
        {
            if (inputDir == Vector3.zero) return Vector3.zero;

            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 worldDir = camForward * inputDir.z + camRight * inputDir.x;
            return worldDir.normalized;
        }

        public void TeleportToPositon(Vector3 position)
        {
            _rb.position = position;
        }

        public void RotateCharacterSnap(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - _rotationTarget.transform.position;
            direction.y = 0f;

            if (direction == Vector3.zero) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation *= Quaternion.Euler(0, _rotationFix, 0);

            _rotationTarget.transform.rotation = targetRotation;
        }

        protected virtual void OnDisable()
        {
            //LastDirectionAngleObs.Value = 0.0f;
            _applyingKnockBack = false;
        }
    }
}
