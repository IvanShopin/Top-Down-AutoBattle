using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private int maxHealth = 100;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private int _currentHealth;

    public int CurrentHealth => _currentHealth;
    public int MaxHealth => maxHealth;

    public System.Action<int, int> OnHealthChanged; // (current, max)
    public System.Action OnDied;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _currentHealth = maxHealth;
    }

    private void Update()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        _moveInput = _moveInput.normalized;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0) return;

        _currentHealth = Mathf.Max(0, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, maxHealth);

        if (_currentHealth == 0)
        {
            OnDied?.Invoke();
        }
    }
}
