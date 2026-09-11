using UnityEngine;

public class HookController : MonoBehaviour
{
    public float rotationSpeed = 60f;
    public float maxAngle = 85f; 
    public float moveSpeed = 10f;
    
    public LineRenderer rope; 
    public Animator minerAnim; 

    private float currentAngle = 0f;
    private int direction = 1;
    private bool isShooting = false;
    private bool isRetracting = false;

    private Vector3 startPosition;
    private GameObject hookedItem;
    private float originalMoveSpeed;

    void Start()
    {
        startPosition = transform.position;
        originalMoveSpeed = moveSpeed;
        
        if (rope != null)
        {
            rope.positionCount = 2; 
        }
    }

    void Update()
    {
        DrawRope();

        if (!isShooting && !isRetracting)
        {
            RotateHook();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isShooting = true;
            }
        }
        else
        {
            MoveHook();
        }
    }

    void RotateHook()
    {
        currentAngle += rotationSpeed * direction * Time.deltaTime;
        if (currentAngle >= maxAngle || currentAngle <= -maxAngle)
        {
            direction *= -1;
        }
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }

    void MoveHook()
    {
        if (isShooting)
        {
            // Móc bay xuống
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            
            // Dịch tọa độ thực tế sang tỷ lệ màn hình Camera (từ 0.0 đến 1.0)
            Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
            
            // Ép giới hạn: Chạm mép dưới (y < 0), mép trái (x < 0) hoặc mép phải (x > 1) thì thu dây
            if (viewportPos.y < 0f || viewportPos.x < 0f || viewportPos.x > 1f)
            {
                isShooting = false;
                isRetracting = true;
            }
        }
        else if (isRetracting)
        {
            transform.position = Vector3.MoveTowards(transform.position, startPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPosition) < 0.1f)
            {
                transform.position = startPosition;
                isRetracting = false;
                moveSpeed = originalMoveSpeed;

                if (hookedItem != null)
                {
                    ItemData item = hookedItem.GetComponent<ItemData>();
                    if (item != null)
                    {
                        Debug.Log("Đã kiếm được số tiền: " + item.itemValue);
                    }
                    Destroy(hookedItem);
                }

                if (minerAnim != null) minerAnim.SetBool("isPulling", false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isShooting)
        {
            ItemData item = other.GetComponent<ItemData>();
            if (item != null)
            {
                isShooting = false;
                isRetracting = true;
                hookedItem = other.gameObject;
                
                hookedItem.transform.SetParent(transform);
                hookedItem.transform.position = transform.position; 
                
                moveSpeed = originalMoveSpeed / item.weight;

                if (minerAnim != null) minerAnim.SetBool("isPulling", true);
            }
        }
    }

    void DrawRope()
    {
        if (rope != null)
        {
            rope.SetPosition(0, startPosition);
            rope.SetPosition(1, transform.position);
        }
    }
}