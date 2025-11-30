using FMOD.Studio;
using Game.Audio;
using Game.EntityMovement;
using Game.GameInput;
using System.Collections.Generic;
using UnityEngine;
using Game.CharacterEnemy;

namespace Game.CharacterPlayer
{
    public class PlayerMovement : Movement
    {
        [SerializeField] protected Animator Animator;

        Vector2 input = Vector2.zero;
        Vector2 lookInput = Vector2.zero;

        bool jumpInput = false;
        bool okToPerformJump = true;

        bool moving = false;
        bool grounded = false;

        // Mouse sensitivity settings
        [SerializeField] float horizontalSensitivity = .2f;
        [SerializeField] float verticalSensitivity = .2f;

        // Camera tilt effect
        float cameraTiltAngle = 0f;
        float maxTiltAngle = 2f;
        float tiltSpeed = 7f;

        EventInstance playerFootSteps;
        EventInstance enemyClose;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        EnemySpawner enemySpawner;

        protected override void Awake()
        {
            base.Awake();

            //Signals.Get<Environment_Changed>().AddListener(ReactToEnvironmentChange);
        }

        void Start()
        {
            playerFootSteps = AudioManager.Instance.CreateEventInstance(AudioManager.Instance.AudioDataInstance.PlayerFootsteps);
            enemyClose = AudioManager.Instance.CreateEventInstance(AudioManager.Instance.AudioDataInstance.PlayerDanger);

            initialPosition = transform.position;
            initialRotation = _rotationTarget.transform.localRotation;

            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        protected override void Update()
        {
            RegisterInput();
            base.Update();
            HandleRotation();

            HandleJump();

            CheckClosestEnemyAndPlaySound();
        }

        void RegisterInput()
        {
            if (_okToMove)
            {
                input = InputManager.Instance.InputDirection;
                lookInput = InputManager.Instance.LookDirection;
                jumpInput = InputManager.Instance.JumpInput;

                if(!jumpInput && grounded)
                {
                    okToPerformJump = true;
                }
            }

            Vector3 moveDir = input;
            moveDir.z = moveDir.y;
            moveDir.y = 0;

            SetMovementInput(moveDir.normalized);
        }

        public void ReactToGameTimePause()
        {
            playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);
            moving = false;
            Animator?.SetBool("Moving", moving);
        }

        public Vector3 LastTickPosition { get; private set; }
        public Vector3 CurrentTickPosition { get; private set; }

        private Vector3 _verticalVelocity;

        protected override void MoveCharacter()
        {
            if (forceGrounded)
                return;

            LastTickPosition = _rb.position;

            Vector3 moveDir = _moveDirection;

            if (grounded && _verticalVelocity.y < 0f)
                _verticalVelocity.y = 0f; // reset when touching ground

            // Apply gravity manually
            //_verticalVelocity += Physics.gravity * Time.deltaTime; // multiply for stronger gravity

            // Camera-relative movement
            Transform cam = Camera.main.transform;
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 relativeMove =
                camForward * moveDir.z +
                camRight * moveDir.x;

            float speed = 5f;
            Vector3 horizontalMovement = relativeMove.normalized * speed * Time.deltaTime;

            // Combine horizontal + vertical movement
            Vector3 movement = horizontalMovement + _verticalVelocity * Time.deltaTime;

            _rb.MovePosition(_rb.position + movement);

            CurrentTickPosition = _rb.position;
            FootStepsHandling(moveDir);
        }

        protected virtual void FootStepsHandling(Vector3 movement)
        {
            if (movement != Vector3.zero && grounded)
            {
                PLAYBACK_STATE playbackState;
                playerFootSteps.getPlaybackState(out playbackState);
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                    playerFootSteps.start();
            }
            else if (movement == Vector3.zero || !grounded)
            {
                playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        void HandleRotation()
        {
            float mouseX = lookInput.x * horizontalSensitivity;
            float mouseY = lookInput.y * verticalSensitivity;

            // Rotate horizontally and vertically together
            Vector3 currentEuler = _rotationTarget.transform.localEulerAngles;
            currentEuler.y += mouseX;
            currentEuler.x -= mouseY; // invert Y for typical FPS feel

            // Optional clamp to prevent flipping upside down
            if (currentEuler.x > 180f) currentEuler.x -= 360f;
            currentEuler.x = Mathf.Clamp(currentEuler.x, -85f, 85f);

            // Camera tilt effect based on horizontal movement
            float targetTilt = -input.x * maxTiltAngle; // Negative so right movement tilts right
            cameraTiltAngle = Mathf.Lerp(cameraTiltAngle, targetTilt, tiltSpeed * Time.deltaTime);
            currentEuler.z = cameraTiltAngle;

            _rotationTarget.transform.localEulerAngles = currentEuler;
        }

        bool forceGrounded = false;
        TimeLeftClock airTime;
        void HandleJump()
        {
            CheckGrounded();

            if (grounded)
            {
                if (okToPerformJump && jumpInput)
                {
                    Jump();
                }
            }
            else if (airTime.IsTimeOver())
            {
                forceGrounded = true;
            }
        }

        void Jump()
        {
            Vector3 velocity = _rb.linearVelocity;
            velocity.y = 0f;
            _rb.linearVelocity = velocity;

            // Apply upward impulse
            float jumpForce = 5f;
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);

            okToPerformJump = false;

            if (airTime == null)
            {
                airTime = new TimeLeftClock(2.0f);
            }
            else
            {
                airTime.ResetTimer();
            }

            AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.PlayerJump, transform.position);
        }

        void CheckGrounded()
        {
            var groundLayer = LayerMask.GetMask("Ground");
            float rayLength = 1.1f;
            Vector3 rayOrigin = transform.position;
            Vector3 rayDirection = Vector3.down;

            grounded = Physics.Raycast(rayOrigin, rayDirection, rayLength, groundLayer);

            Color rayColor = grounded ? Color.green : Color.red;
            Debug.DrawRay(rayOrigin, rayDirection * rayLength, rayColor, 1.0f);

            if (forceGrounded)
            {
                forceGrounded = !grounded;
            }
        }

        //void ReactToEnvironmentChange()
        //{
        //    var groundLayer = LayerMask.GetMask("Ground");
        //    float rayDistance = 100000000f;
        //    Vector3 origin = transform.position + Vector3.up * 0.1f; // slightly above player

        //    RaycastHit hit;

        //    // 1. Check downward
        //    if (Physics.Raycast(origin, Vector3.down, out hit, rayDistance, groundLayer))
        //    {
        //        Vector3 pos = transform.position;
        //        pos.y = hit.point.y; // set player on top of ground
        //        transform.position = pos;
        //        return;
        //    }

        //    // 2. Check upward
        //    if (Physics.Raycast(origin, Vector3.up, out hit, rayDistance, groundLayer))
        //    {
        //        Vector3 pos = transform.position;
        //        pos.y = hit.point.y;
        //        transform.position = pos;
        //        return;
        //    }
        //}

        public void Respawn()
        {
            // Reset position
            transform.position = initialPosition;
            _rb.position = initialPosition;

            // Reset rotation
            _rotationTarget.transform.localRotation = initialRotation;

            // Reset velocities
            _rb.linearVelocity = Vector3.zero;
            _verticalVelocity = Vector3.zero;

            // Reset camera tilt
            cameraTiltAngle = 0f;

            // Reset input states
            input = Vector2.zero;
            lookInput = Vector2.zero;
            jumpInput = false;
            okToPerformJump = true;
            moving = false;

            // Stop footsteps
            playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);

            // Reset animator
            Animator?.SetBool("Moving", false);
        }

        void CheckClosestEnemyAndPlaySound()
        {
            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            if (Time.timeScale != 0)
            {
                foreach (Transform enemy in enemySpawner.Enemies)
                {
                    if (enemy == null) continue;

                    float dist = Vector3.Distance(transform.position, enemy.position);

                    if (dist < closestDistance)
                    {
                        closestDistance = dist;
                        closestEnemy = enemy;
                    }
                }
            }

            float soundDist = 8f;
            if (closestEnemy != null && closestDistance < soundDist)
            {
                PLAYBACK_STATE playbackState;
                enemyClose.getPlaybackState(out playbackState);
                if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
                    enemyClose.start();
                else
                {
                    float pitchValue = 1 - ((closestDistance - 2.5f) / (soundDist - 2.5f));
                    float dangerVolumeValue = 1 - ((closestDistance - 2.5f) / (soundDist - 2.5f));

                    enemyClose.setParameterByName("Pitch", pitchValue);
                    enemyClose.setParameterByName("Danger", dangerVolumeValue);
                }
            }
            else
            {
                enemyClose.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }

        protected override void OnDisable()
        {
            playerFootSteps.stop(STOP_MODE.ALLOWFADEOUT);
            enemyClose.stop(STOP_MODE.ALLOWFADEOUT);

            base.OnDisable();
        }

        //void OnDestroy()
        //{
        //    //Signals.Get<Environment_Changed>().RemoveListener(ReactToEnvironmentChange);
        //}
    }
}
