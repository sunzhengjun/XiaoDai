using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("移动")]
    [Tooltip("步速尺度")]
    public float walkSpeed = 4.5f;

    [Tooltip("转向平滑时间（值越小转弯越顺畅）")]
    public float turnSmoothTime = 0.1f;

    [Header("视角")]
    [Tooltip("确认移动方向")]
    public Camera mainCamera;

    // 跳跃
    [Header("跳跃")]
    [Tooltip("跳跃高度")]
    public float jumpHeight = 1.5f;

    [Tooltip("重力加速度")]
    public float gravity = -9.81f;

    [Header("地面检测")]
    [Tooltip("地面检测向下偏移量，避免高速下落时漏判")]
    public float groundCheckOffset = 0.05f;

    [Tooltip("被视为地面的层级掩码")]
    public LayerMask groundLayers = ~0;

    private float verticalVelocity;

    //
    private CharacterController characterController;

    // 转向平滑系数
    private float turnSmoothVelocity;
    private Vector3 moveDirection;

    private void Start()
    {
        // 获取角色控制器
        characterController = GetComponent<CharacterController>();

        // 远拉主目录相机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("未找到相机，无法检测方向");
            }
        }
    }

    private void Update()
    {
        // 移动逻辑
        HandleWalking();
        HandleJumpAndGravity();
    }

    /// <summary>
    /// WASD组合实现旋转并移动
    /// </summary>
    private void HandleWalking()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // 计算方向向量，确保仅介于水平面
        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            CalculateMoveDirection(inputDirection);

            float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
            );
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    /// <summary>
    /// 按导航相机计算移动方向
    /// </summary>
    private void CalculateMoveDirection(Vector3 inputDir)
    {
        if (mainCamera != null)
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            Vector3 cameraRight = mainCamera.transform.right;

            cameraForward.y = 0f;
            cameraRight.y = 0f;

            cameraForward.Normalize();
            cameraRight.Normalize();

            moveDirection = (cameraForward * inputDir.z + cameraRight * inputDir.x).normalized;
        }
        else
        {
            moveDirection = inputDir;
        }
    }

    /// <summary>
    /// 采集跳跃和重力逻辑
    /// </summary>
    private void HandleJumpAndGravity()
    {
        bool isGrounded = characterController.isGrounded || IsGrounded();

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection * walkSpeed;
        velocity.y = verticalVelocity;
        
        // 优先依据当前帧的预期移动更新碰撞状态，防止地面判定延迟导致持续下落
        CollisionFlags collisionFlags = characterController.Move(velocity * Time.deltaTime);

        if ((collisionFlags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
    }

    /// <summary>
    /// 通过额外的球体检测辅助 CharacterController 的地面判定，防止持续下坠
    /// </summary>
    /// <returns>是否接触到地面</returns>
    private bool IsGrounded()
    {
        if (characterController == null)
        {
            return false;
        }

        Bounds bounds = characterController.bounds;
        float radius = characterController.radius;

        // 在角色底部附近放置检测球，稍微向下偏移以覆盖 skinWidth
        Vector3 sphereCenter = bounds.center +
                               Vector3.down * (bounds.extents.y - radius + groundCheckOffset);

        return Physics.CheckSphere(
            sphereCenter,
            radius,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }

    // Scene图中显示角色检测范围
    private void OnDrawGizmosSelected()
    {
        if (characterController == null)
        {
            characterController = GetComponent<CharacterController>();
        }

        if (characterController == null)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position + Vector3.up * characterController.height / 2,
            new Vector3(
                characterController.radius * 2,
                characterController.height,
                characterController.radius * 2
            )
        );

        Gizmos.color = Color.yellow;
        Bounds bounds = characterController.bounds;
        float radius = characterController.radius;
        Vector3 sphereCenter = bounds.center +
                               Vector3.down * (bounds.extents.y - radius + groundCheckOffset);
        Gizmos.DrawWireSphere(sphereCenter, radius);
    }
}
