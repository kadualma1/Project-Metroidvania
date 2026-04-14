using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour 
{
    public static PlayerInput Input;

    public static Vector2 Movement;

    private InputAction moveInput;

    private InputActionMap gameplayMap;

    private void Awake()
    {
        Input = GetComponent<PlayerInput>();

        moveInput = Input.actions["Move"];
    }

    private void Update()
    {
        Movement = moveInput.ReadValue<Vector2>();
    }
}
