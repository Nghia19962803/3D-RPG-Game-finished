using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpgAdventure
{
    public class PlayerController : MonoBehaviour , IAttackAnimListener, IMessageReceiver
    {
        public static PlayerController Instance
        {
            get
            {
                return s_Instance;
            }
        }

        public bool IsRespawning { get { return m_IsRespawning; } }

        public MeleeWeapon meleeWeapon;
        public float maxForwardSpeed = 8f;
        public float rotationSpeed;
        public float m_MaxRotationSpeed = 1200;
        public float m_MinRotationSpeed = 800;
        public float gravity = 20f;
        public Transform attackHand;

        [Header("Vertical Speed Check")]
        public float jumpSpeed = 10;
        public float m_VerticalSpeed;
        public bool m_IsGrounded;
        public bool m_ReadyToJump;
        private float k_StickingGravityProportion = 0.3f;
        private bool m_InCombo;
        private float k_JumpAbortSpeed = 10f;

        private static PlayerController s_Instance;
        private PlayerInput m_PlayerInput;
        private Damageable m_Damageable;
        private CharacterController m_ChController;
        private Animator m_Animator;
        private CameraController m_CameraController;
        private HubManager m_HubManger;
        private Quaternion m_TargetRotation;

        private AnimatorStateInfo m_CurrentStateInfo;
        private AnimatorStateInfo m_NextStateInfo;
        private bool m_IsAnimatorTransitioning;
        private bool m_IsRespawning;

        private float m_DesiredForwardSpeed;
        private float m_ForwardSpeed;
        const float k_Acceleration = 20f;
        const float k_Deceleration = 35f;

        // Animator parameter hashes
        private readonly int m_HashAirborneVerticalSpeed = Animator.StringToHash("AirborneVerticalSpeed");
        private readonly int m_HashGrounded = Animator.StringToHash("Grounded");
        private readonly int m_HashForwardSpeed = Animator.StringToHash("ForwardSpeed");
        private readonly int m_HashMeleeAttack = Animator.StringToHash("MeleeAttack");
        private readonly int m_HashDeath = Animator.StringToHash("Death");
        private readonly int m_HashStateTime = Animator.StringToHash("StateTime");
        private readonly int m_HashStateRoll = Animator.StringToHash("RollForward");
        
        // Animator Tag Hashes
        private readonly int m_HashBlockInput = Animator.StringToHash("BlockInput");

        private void Awake()
        {
            m_ChController = GetComponent<CharacterController>();
            m_PlayerInput = GetComponent<PlayerInput>();
            m_Animator = GetComponent<Animator>();
            m_Damageable = GetComponent<Damageable>();
            m_CameraController = Camera.main.GetComponent<CameraController>();
            m_HubManger = FindObjectOfType<HubManager>();

            s_Instance = this;  

            m_HubManger.SetMaxHealth(m_Damageable.maxHitPoints);
        }
        private void FixedUpdate()
        {
            CacheAnimationState();
            ComputeForwardMovement();
            ComputeVerticalMovement();
            ComputeRotation();

            if (m_PlayerInput.IsMoveInput)
            {
                float rotationSpeed = Mathf.Lerp(m_MaxRotationSpeed, m_MinRotationSpeed, m_ForwardSpeed / m_DesiredForwardSpeed);
                m_TargetRotation = Quaternion.RotateTowards(
                    transform.rotation,
                    m_TargetRotation,
                    rotationSpeed * Time.fixedDeltaTime);

                transform.rotation = m_TargetRotation;
            }

            m_Animator.ResetTrigger(m_HashMeleeAttack);
            m_Animator.ResetTrigger(m_HashStateRoll);
            if (m_PlayerInput.IsAttack)
            {
                //Debug.Log("Is attacking");
                m_Animator.SetTrigger(m_HashMeleeAttack);
            }

            if (m_PlayerInput.RollForward)
            {
                //Debug.Log("Is attacking");
                m_Animator.SetTrigger(m_HashStateRoll);
            }
        }

        private void OnAnimatorMove()
        {
            if (m_IsRespawning) { return; }

            // Vector3 movement = m_Animator.deltaPosition;
            // movement += m_VerticalSpeed * Vector3.up * Time.fixedDeltaTime;
            // m_ChController.Move(movement);

            Vector3 movement;
            if(m_IsGrounded)
            {
                RaycastHit hit;
                Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);
                if(Physics.Raycast(ray, out hit, 1, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    movement = Vector3.ProjectOnPlane(m_Animator.deltaPosition, hit.normal);

                    Renderer groundRenderer = hit.collider.GetComponentInChildren<Renderer>();

                    //// de sau
                    //m_CurrentWalkingSurface = groundRenderer ? groundRenderer.sharedMaterial : null;
                }
                else
                {
                    movement = m_Animator.deltaPosition;

                    //m_CurrentWalkingSurface = null;
                }
            }
            else
            {
                // If not grounded the movement is just in the forward direction.
                movement = m_ForwardSpeed * transform.forward * Time.deltaTime;
            }

            // Rotate the transform of the character controller by the animation's root rotation.
            m_ChController.transform.rotation *= m_Animator.deltaRotation;

            // Add to the movement with the calculated vertical speed.
            movement += m_VerticalSpeed * Vector3.up * Time.deltaTime;

            // Move the character controller.
            m_ChController.Move(movement);

            // After the movement store whether or not the character controller is grounded.
            m_IsGrounded = m_ChController.isGrounded;

            // If Ellen is not on the ground then send the vertical speed to the animator.
            // This is so the vertical speed is kept when landing so the correct landing animation is played.
            if (!m_IsGrounded)
                m_Animator.SetFloat(m_HashAirborneVerticalSpeed, m_VerticalSpeed);

            // Send whether or not Ellen is on the ground to the animator.
            m_Animator.SetBool(m_HashGrounded, m_IsGrounded);
        }
        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {
            if (type == MessageType.DAMAGED)
            {                          
                m_HubManger.SetHealth((sender as Damageable).CurrentHitPoint);               
                Debug.Log("Current health is: " + (sender as Damageable).CurrentHitPoint);
            }

            if(type == MessageType.DEAD)
            {
                m_IsRespawning = true;
                m_Animator.SetTrigger(m_HashDeath);
                m_HubManger.SetHealth(0);
            }
        }
        public void MeleeAttackStart()
        {
            if (meleeWeapon != null)
            {
                meleeWeapon.BeginAttack();
            }          
        }
        public void MeleeAttackEnd()
        {
            if (meleeWeapon != null)
            {
                meleeWeapon.EndAttack();
            }
        }
        public void StartRespawn()
        {
            transform.position = Vector3.zero;
            m_HubManger.SetHealth(m_Damageable.maxHitPoints);
            m_Damageable.SetInitialHealth();
        }
        public void FinishRespawn()
        {
            m_IsRespawning = false;
        }
        public void UseItemFrom(InventorySlot slot)
        {
            if(meleeWeapon != null)
            {
                if(slot.itemPrefab.name == meleeWeapon.name) { return; }
                else
                {
                    Destroy(meleeWeapon.gameObject);
                }
            }

            meleeWeapon = Instantiate(slot.itemPrefab.transform)
                .GetComponent<MeleeWeapon>();
            meleeWeapon.GetComponent<FixedFollow>().SetFolowee(attackHand);
            //Debug.Log("da cam item");
            meleeWeapon.name = slot.itemPrefab.name;
            meleeWeapon.SetOwner(gameObject);
        }
        private void ComputeVerticalMovement()
        {
            if(!m_PlayerInput.JumpInput && m_IsGrounded)
                m_ReadyToJump = true;

            if (m_IsGrounded)
            {
                // When grounded we apply a slight negative vertical speed to make Ellen "stick" to the ground.
                m_VerticalSpeed = -gravity * k_StickingGravityProportion;

                // If jump is held, Ellen is ready to jump and not currently in the middle of a melee combo...
                if (m_PlayerInput.JumpInput && m_ReadyToJump)
                {
                    // ... then override the previously set vertical speed and make sure she cannot jump again.
                    m_VerticalSpeed = jumpSpeed;
                    m_IsGrounded = false;
                    m_ReadyToJump = false;
                }
            }
            else
            {
                // If Ellen is airborne, the jump button is not held and Ellen is currently moving upwards...
                if (!m_PlayerInput.JumpInput && m_VerticalSpeed > 0.0f)
                {
                    // ... decrease Ellen's vertical speed.
                    // This is what causes holding jump to jump higher that tapping jump.
                    m_VerticalSpeed -= k_JumpAbortSpeed * Time.deltaTime;
                }

                // If a jump is approximately peaking, make it absolute.
                if (Mathf.Approximately(m_VerticalSpeed, 0f))
                {
                    m_VerticalSpeed = 0f;
                }

                m_VerticalSpeed -= gravity * Time.deltaTime;
            }

            
        }

        private void ComputeForwardMovement()
        {
            Vector3 moveInput = m_PlayerInput.MoveInput.normalized;
            m_DesiredForwardSpeed = moveInput.magnitude * maxForwardSpeed;

            float acceleration = m_PlayerInput.IsMoveInput ? k_Acceleration : k_Deceleration;

            m_ForwardSpeed = Mathf.MoveTowards(
                m_ForwardSpeed,
                m_DesiredForwardSpeed,
                Time.fixedDeltaTime * acceleration);

            m_Animator.SetFloat(m_HashForwardSpeed, m_ForwardSpeed);
        }

        private void ComputeRotation()
        {
            Vector3 moveInput = m_PlayerInput.MoveInput.normalized;

            Vector3 cameraDirection = Quaternion.Euler(
                0,
                m_CameraController.PlayerCam.m_XAxis.Value,
                0) * Vector3.forward;

            Quaternion targetRotation;

            if (Mathf.Approximately(Vector3.Dot(moveInput, Vector3.forward), -1.0f))
            {
                targetRotation = Quaternion.LookRotation(-cameraDirection);
            }
            else
            {
                Quaternion movementRotation = Quaternion.FromToRotation(Vector3.forward, moveInput);
                targetRotation = Quaternion.LookRotation(movementRotation * cameraDirection);
            }

            m_TargetRotation = targetRotation;
        }
        private void CacheAnimationState()
        {
            m_CurrentStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            m_NextStateInfo = m_Animator.GetCurrentAnimatorStateInfo(0);
            m_IsAnimatorTransitioning = m_Animator.IsInTransition(0);

            m_Animator.SetFloat(m_HashStateTime, Mathf.Repeat(m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f));
            m_Animator.ResetTrigger(m_HashMeleeAttack);
        }
        private void UpdateInputBlocking()
        {
            bool inputBlocked = m_CurrentStateInfo.tagHash == m_HashBlockInput && !m_IsAnimatorTransitioning;
            inputBlocked |= m_NextStateInfo.tagHash == m_HashBlockInput;
            m_PlayerInput.isPlayerControllerInputBlocked = inputBlocked;
            Debug.Log(inputBlocked);
        }
    }
}

