using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour 
{
    public static PlayerInput Input;

    public static Vector2 Movement;
    public static bool JumpWasPressed;
    public static bool JumpWasReleased;

    private InputAction moveInput;
    private InputAction jumpInput;

    private InputActionMap gameplayMap;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();

        moveInput = Input.actions["Move"];
        jumpInput = Input.actions["Jump"];
    }

    private void Update()
    {
        Movement = moveInput.ReadValue<Vector2>();
        JumpWasPressed = jumpInput.WasPressedThisFrame();
        JumpWasReleased = jumpInput.WasReleasedThisFrame();
    }
}
