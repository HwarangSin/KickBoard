using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables: Movement
    private Vector2 _input;
    private CharacterController _characterController;
    private Vector3 _direction;
    [SerializeField] private float speed;
    #endregion
    #region Variables: Rotation
    [SerializeField] private float smoothTime = 0.05f;
    private float _currentVelocity;

    #endregion
    #region Variables: Gravity
    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;
    #endregion
    #region Variables: Jump
    [SerializeField] private float jumpPower;
    private int _numberOfJumps;
    [SerializeField] private int maxNumberOfJumps = 2;
    #endregion


    public float attackRange = 1.5f;   // ���� ����
    public float attackDuration = 0.5f; // ���� �ִϸ��̼� ���� �ð�
    public LayerMask enemyLayer;       // �� ���̾�
    private bool isAttacking = false;  // ���� ������ üũ
    [SerializeField] private Animator animator;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();

        if (Input.GetMouseButtonDown(0) && !isAttacking && IsGrounded())
            StartCoroutine(Attack());
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && _velocity < 0.0f)
        {
            _velocity = -1.0f;
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }
        
        _direction.y = _velocity;
    }
    private void ApplyRotation()
    {
        if (_input.sqrMagnitude == 0) return;
        if (isAttacking) return;

        var targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg;
        var angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _currentVelocity, smoothTime);
        transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);
    }
    private void ApplyMovement()
    {
        if (isAttacking) return;

        _characterController.Move(_direction * speed * Time.deltaTime);
    }

    public void Move(InputAction.CallbackContext context)
    {
        _input = context.ReadValue<Vector2>();
        _direction = new Vector3(_input.x, 0.0f, _input.y);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!IsGrounded() && _numberOfJumps >= maxNumberOfJumps) return;
        if( _numberOfJumps == 0 ) StartCoroutine(WaitForLanding());

        _numberOfJumps++;
        _velocity = jumpPower;
    }

    private IEnumerator WaitForLanding()
    {
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(IsGrounded);

        _numberOfJumps = 0;
    }

    private bool IsGrounded() => _characterController.isGrounded;

    IEnumerator Attack()
    {
        isAttacking = true; // ���� ���� (�̵� ����)

        // ���콺 Ŭ�� ��ġ ���
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            Vector3 attackDirection = (hit.point - transform.position).normalized;
            attackDirection.y = 0; // Y�� ����

            // ���� �������� ĳ���� ȸ��
            transform.forward = attackDirection;
        }

        // ���� �ִϸ��̼� ����
        //animator.SetTrigger("Attack");

        // ���� ���� �� �� ����
        yield return new WaitForSeconds(attackDuration * 0.3f); // �ִϸ��̼��� ���� ����� �������� Ÿ�� ����
        DetectEnemies();

        // ���� ���� �� �̵� ����
        yield return new WaitForSeconds(attackDuration * 0.7f);
        isAttacking = false;
    }

    void DetectEnemies()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * attackRange, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            Debug.Log($"�� {enemy.name} Ÿ��!");
            // ���⼭ ������ �������� �ִ� ���� �߰� ����
        }
    }
}
