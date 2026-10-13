using UnityEngine;

public class EnemyCollision : MonoBehaviour
{
    // Kích hoạt ngay khi Collider khác đi vào vùng va chạm của bẫy / quái
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem đối tượng va chạm có phải là Player hay không dựa vào Tag
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player đã chạm vào bẫy và bị tiêu diệt ngay lập tức!");
            
            // Hủy nhân vật Player ngay lập tức
            Destroy(other.gameObject);
        }
    }
}
