using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.GameScreen;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

namespace Game.GameInput
{
    public class InputManager : MonoBehaviour
    {
        static public InputManager Instance { get; private set; }

        PlayerInput playerInput;

        InputAction pauseAction;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction fireAction;
        InputAction fireAltAction;

        public Vector2 InputDirection { get; private set; }
        public Vector2 LookDirection { get; private set; }
        public bool JumpInput { get; private set; }
        public bool FireInput { get; private set; }
        public bool FireAlternate { get; private set; }
        public bool AttackInput { get; private set; }

        public float MouseSensitivity { get; set; }

        void Awake()
        {
            playerInput = GetComponent<PlayerInput>();

            moveAction = playerInput.actions["Move"];
            lookAction = playerInput.actions["Look"];
            jumpAction = playerInput.actions["Jump"];
            fireAction = playerInput.actions["Fire"];
            fireAltAction = playerInput.actions["FireAlternate"];
            pauseAction = playerInput.actions["Pause"];

            Init();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Instance = this;
        }

        void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device)
        {
            if (change == InputUserChange.ControlSchemeChanged)
            {
                string scheme = user.controlScheme.ToString();
                int index = scheme.IndexOf("(");
                if (index >= 0)
                    scheme = scheme.Substring(0, index);

                //Debug.Log("InputManager: Control scheme changed to " + scheme);
            }
        }

        public void ChangeToUIActionMap()
        {
            if (playerInput.currentActionMap.name != "UI")
                playerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ChangeToGameActionMap()
        {
            if (playerInput.currentActionMap.name != "Player")
                playerInput.SwitchCurrentActionMap("Player");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void OnMoveAction(InputAction.CallbackContext context)
        {
            SetInputDirection(context.ReadValue<Vector2>());
        }

        void OnMoveActionStop(InputAction.CallbackContext context)
        {
            SetInputDirection(Vector2.zero);
        }

        void OnLookAction(InputAction.CallbackContext context)
        {
            SetLookDirection(context.ReadValue<Vector2>());
        }

        void OnLookActionStop(InputAction.CallbackContext context)
        {
            SetLookDirection(Vector2.zero);
        }

        void OnJumpAction(InputAction.CallbackContext context)
        {
            if (context.performed)
                SetJumpInput(true);
        }

        void OnJumpActionStop(InputAction.CallbackContext context)
        {
            SetJumpInput(false);
        }

        void OnPauseAction(InputAction.CallbackContext context)
        {
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.PauseScreen);
            //Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.DeathScreen);
        }

        void OnFireAction(InputAction.CallbackContext context)
        {
            if (context.performed)
                SetFireInput(true);
        }

        void OnFireActionStop(InputAction.CallbackContext context)
        {
            SetFireInput(false);
        }

        void OnFireAltAction(InputAction.CallbackContext context)
        {
            if (context.performed)
                SetFireAlternateInput(true);
        }

        void OnFireAltActionStop(InputAction.CallbackContext context)
        {
            SetFireAlternateInput(false);
        }

        void SetInputDirection(Vector2 direction)
        {
            InputDirection = direction;
        }

        void SetLookDirection(Vector2 direction)
        {
            LookDirection = direction * MouseSensitivity;
        }

        void SetJumpInput(bool status)
        {
            JumpInput = status;
        }

        void SetFireInput(bool status)
        {
            FireInput = status;
        }

        void SetFireAlternateInput(bool status)
        {
            FireAlternate = status;
        }

        void Init()
        {
            MouseSensitivity = 0.5f;

            InputUser.onChange += OnInputUserChange;

            moveAction.performed += OnMoveAction;
            moveAction.canceled += OnMoveActionStop;

            lookAction.performed += OnLookAction;
            lookAction.canceled += OnLookActionStop;

            jumpAction.performed += OnJumpAction;
            jumpAction.canceled += OnJumpActionStop;

            fireAction.performed += OnFireAction;
            fireAction.canceled += OnFireActionStop;

            fireAltAction.performed += OnFireAltAction;
            fireAltAction.canceled += OnFireAltActionStop;

            pauseAction.performed += OnPauseAction;
        }

        void OnDestroy()
        {
            InputUser.onChange -= OnInputUserChange;

            moveAction.performed -= OnMoveAction;
            moveAction.canceled -= OnMoveActionStop;

            lookAction.performed -= OnLookAction;
            lookAction.canceled -= OnLookActionStop;

            jumpAction.performed -= OnJumpAction;
            jumpAction.canceled -= OnJumpActionStop;

            fireAction.performed -= OnFireAction;
            fireAction.canceled -= OnFireActionStop;

            fireAltAction.performed -= OnFireAltAction;
            fireAltAction.canceled -= OnFireAltActionStop;

            pauseAction.performed -= OnPauseAction;
        }
    }
}
