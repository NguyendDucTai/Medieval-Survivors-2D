using UnityEngine;
using TMPro; // Script điều khiển được TextMeshPro

public class PlayerCollectionScore : MonoBehaviour
{
    // Biến tham chiếu giao diện Text từ Unity
    public TextMeshProUGUI scoreText; 

    private int playerScore = 0;

    private void Start()
    {
        // Khởi tạo hiển thị ban đầu khi game vừa chạy
        UpdateScoreUI();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra nếu va chạm trúng đối tượng có Tag là Reward
        if (other.CompareTag("Reward"))
        {
            // 1. Tăng điểm
            playerScore++;
            Debug.Log("Đã nhặt được vật phẩm! Điểm hiện tại: " + playerScore);

            // 2. Cập nhật số điểm mới lên màn hình UI
            UpdateScoreUI();

            // 3. Hủy vật phẩm thưởng
            Destroy(other.gameObject);
        }
    }

    // Hàm phụ trợ giúp cập nhật nội dung chữ hiển thị điểm
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Điểm: " + playerScore;
        }
    }
}
