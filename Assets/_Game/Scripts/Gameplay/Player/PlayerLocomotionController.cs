using UnityEngine;
using UnityEngine.InputSystem;

namespace ChatGPT_Detective
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerLocomotionController : MonoBehaviour
    {
        private class GoalStatusArgs
        {
            public bool m_status;
        }

        [SerializeField] private InputActionReference m_moveInputAction;

        [Header("Player")]
        [Tooltip("ProcessLocomotion speed of the character in m/s")]
        [SerializeField] private float m_moveSpeed = 2.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        [SerializeField] private float m_rotationSmoothTime = 0.12f;

        [SerializeField] private float m_locomotionBlendTime = 1.6f;

        [SerializeField] private SoundEffectsHandler m_footstepsSoundHandler;

        [Header("Animation")]
        [SerializeField] private string m_speedAnimParameter = "Speed";

        [SerializeField] private string m_stoppedParameter = "Stopped";

        // Player
        private float m_targetRotation;

        private float m_rotationVelocity;

        // Animation Hashes
        private int m_animHashSpeed;

        private int m_animHashStopped;

        private Animator m_animator;

        private CharacterController m_controller;

        private GameObject m_mainCamera;
        
        private Interpolator<float> m_locomotionBlendInterpolator;

        private Interpolator<Quaternion> m_interactionViewInterpolator;

        private Vector2 m_moveInputToProcess;

        private Vector2 m_lastMoveInput;

        private float m_interactionViewInterpTime;

        private bool m_isInputDisabled;

        public float InteractionViewInterpTime
        {
            set => m_interactionViewInterpTime = value;
        }

        private void Awake()
        {
            // Get a reference to our main camera
            if (m_mainCamera == null)
            {
                m_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            m_animator = GetComponent<Animator>();

            m_controller = GetComponent<CharacterController>();
        }

        private void Start()
        {
            AssignAnimationHashes();

            m_locomotionBlendInterpolator = new Interpolator<float>(0f, 1f, m_locomotionBlendTime, Mathf.Lerp);

            m_interactionViewInterpolator = new Interpolator<Quaternion>(Quaternion.identity, Quaternion.identity,
                m_interactionViewInterpTime, Quaternion.Slerp, InterruptLocomotion,
                onReachedDefault: () => m_isInputDisabled = false);
        }

        private void Update()
        {
            if (!m_isInputDisabled)
            {
                HandleLocomotionBlending();
                ProcessLocomotion();
            }

            if (m_interactionViewInterpolator.Interpolating)
            {
                transform.rotation = m_interactionViewInterpolator.Update();
            }
        }

        private void AssignAnimationHashes()
        {
            m_animHashSpeed = Animator.StringToHash(m_speedAnimParameter);
            m_animHashStopped = Animator.StringToHash(m_stoppedParameter);
        }

        private void HandleLocomotionBlending()
        {
            Vector2 currentMoveInput = m_moveInputAction.action.ReadValue<Vector2>();

            bool beganMove = m_lastMoveInput == Vector2.zero && currentMoveInput != Vector2.zero;

            if (beganMove)
            {
                m_animator.SetBool(m_animHashStopped, false);
                m_locomotionBlendInterpolator.Toggle(true);
            }
            else if (m_locomotionBlendInterpolator.Interpolating)
            {
                float currentMag = m_locomotionBlendInterpolator.Update();

                if (currentMoveInput != Vector2.zero)
                {
                    m_moveInputToProcess = currentMoveInput.normalized * currentMag;
                }
            }
            else
            {
                m_moveInputToProcess = currentMoveInput;
            }

            bool endedMove = m_lastMoveInput != Vector2.zero && currentMoveInput == Vector2.zero;

            if (endedMove)
            {
                m_animator.SetBool(m_animHashStopped, true);
                m_interactionViewInterpolator.Reset();
            }

            m_lastMoveInput = currentMoveInput;
        }

        private void ProcessLocomotion()
        {
            float targetSpeed = m_moveInputToProcess == Vector2.zero ? 0f : m_moveSpeed;

            // A reference to the player's current horizontal velocity
            float currentSpeed = new Vector3(m_controller.velocity.x, 0.0f, m_controller.velocity.z).magnitude;

            float inputMagnitude = m_moveInputToProcess.magnitude;

            // Accelerate or decelerate to target speed
            float speed = m_moveSpeed * inputMagnitude;

            // Round speed to 3 decimal places
            speed = Mathf.Round(speed * 1000f) / 1000f;

            // Normalize input direction
            Vector3 inputDirection = new Vector3(m_moveInputToProcess.x, 0.0f, m_moveInputToProcess.y).normalized;

            if (m_moveInputToProcess != Vector2.zero)
            {
                m_targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  m_mainCamera.transform.eulerAngles.y;

                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, m_targetRotation, ref m_rotationVelocity,
                    m_rotationSmoothTime);

                // Rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, m_targetRotation, 0.0f) * Vector3.forward;

            // Move the player
            m_controller.Move(targetDirection.normalized * (speed * Time.deltaTime));

            m_animator.SetFloat(m_animHashSpeed, speed);
        }

        public void ToggleInteractionView(bool enable, Vector3 viewPosition = new Vector3())
        {
            if (enable)
            {
                m_interactionViewInterpolator.DefaultVal = transform.rotation;

                Vector3 lookDirection = viewPosition - transform.position;

                m_interactionViewInterpolator.TargetVal = Quaternion.LookRotation(lookDirection);

                m_interactionViewInterpolator.Toggle(true);
            }
            else
            {
                m_interactionViewInterpolator.Toggle(false);
            }
        }

        private void InterruptLocomotion()
        {
            m_isInputDisabled = true;

            m_animator.SetBool(m_animHashStopped, true);
            m_animator.SetFloat(m_animHashSpeed, 0f);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                m_footstepsSoundHandler.PlaySound();
            }
        }
    }
}
