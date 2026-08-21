using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
public class WalkAnimation : MonoBehaviour
{
    [Tooltip("Animator Bool 参数名，移动时设为 true")] [SerializeField]
    private string m_moveParamName = "IsMoving";

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        bool isMoving = keyboard.wKey.isPressed || keyboard.aKey.isPressed
                                                || keyboard.sKey.isPressed || keyboard.dKey.isPressed;

        m_animator.SetBool(m_moveParamName, isMoving);
    }
}