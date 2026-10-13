# 🛡️ PRU213 - BỘ NHỚ DỰ ÁN VĨNH VIỄN (PROJECT MEMORY & RULES)
> **Dành cho AI Agent:** Đọc và tuân thủ NGHIÊM NGẶT toàn bộ nội dung trong file này ở TẤT CẢ các phiên làm việc (sessions). Đây là bộ nhớ chuẩn và duy nhất của dự án. Không tự ý thay đổi phong cách mỹ thuật hoặc cấu trúc cốt lõi nếu không có yêu cầu trực tiếp từ người dùng.

---

## 🎮 1. THÔNG TIN DỰ ÁN (PROJECT OVERVIEW)
* **Tên môn học:** PRU213 - Lập trình Game 2D với Unity (FPT University).
* **Tên dự án:** Medieval Survivors 2D.
* **Kho mã nguồn (GitHub Repo):** [https://github.com/NguyendDucTai/Medieval-Survivors-2D](https://github.com/NguyendDucTai/Medieval-Survivors-2D)
* **Thể loại Gameplay:** 2D Top-Down Action Roguelike / Survival (phong cách *Vampire Survivors*).
  * Di chuyển 8 hướng linh hoạt.
  * Vũ khí tự động kích hoạt / nhắm bắn kẻ địch trong phạm vi.
  * Quái vật bám đuổi theo bầy đàn với số lượng tăng dần theo thời gian.
  * Nhặt ngọc kinh nghiệm (ExpGem) rơi ra khi quái chết để lên cấp và chọn kỹ năng nâng cấp.
* **Bối cảnh (Theme):** Medieval Fantasy (Trung Cổ Giả Tưởng: Hiệp sĩ, Pháp sư, Cung thủ đối đầu Slime, Skeleton, Goblin, Dơi, Boss Golem).

---

## 🎨 2. QUY CHUẨN ĐỒ HỌA BẮT BUỘC (STRICT ART STYLE DIRECTIVE)
* **Phong cách chuẩn gốc:** **16x16 Retro Pixel Art** đồng nhất toàn dự án.
* **Thông số Texture Import trong Unity:**
  * **Pixels Per Unit (PPU):** `16` (áp dụng cho toàn bộ nhân vật, quái vật, vũ khí, vật phẩm, tilemap).
  * **Filter Mode:** `Point (no filter)` (tuyệt đối không dùng Bilinear hay Trilinear để tránh mờ nhòe pixel).
  * **Compression:** `None` / `Uncompressed`.
  * **Sprite Mode:** `Multiple` (cắt theo lưới 16x16 per frame) cho sprite sheets, hoặc `Single` cho biểu tượng đơn.
* ⚠️ **NGUYÊN TẮC BẤT BIẾN:**
  * **TUYỆT ĐỐI KHÔNG TỰ Ý ĐỔI SANG ĐỒ HỌA HD, 128x128 HAY 256x256.**
  * Mọi asset tạo thêm phải tuân thủ đúng bảng màu pixel và kích thước chuẩn 16x16 (hoặc 32x32 cho Boss).

---

## ⚙️ 3. MÔI TRƯỜNG KỸ THUẬT (TECHNICAL STACK)
* **Unity Version:** Unity 6 (`6000.4.12f1`).
* **Render Pipeline:** Universal Render Pipeline 2D (URP 2D).
* **Vật lý 2D:** Sử dụng thuộc tính `rb.linearVelocity` (chuẩn Unity 6, tuyệt đối không dùng `rigidbody2D.velocity` đã lỗi thời).
* **Input System:** Hỗ trợ tương thích đồng thời cả `UnityEngine.InputSystem` (New Input) và Fallback `Input.GetAxisRaw` (Legacy Input).

---

## 📁 4. CẤU TRÚC ASSETS & SCRIPTS HIỆN TẠI (RESTORED STATE)
1. **Nhân vật người chơi (`Assets/Sprites/Characters/`):**
   * **Hiệp sĩ (Knight):** `Hero_Knight_Idle.png` (32x16, 2 frames), `Hero_Knight_Run.png` (64x16, 4 frames), `Hero_Knight_Die.png` (64x16, 4 frames).
   * **Pháp sư (Mage):** `Hero_Mage_Idle.png` (32x16, 2 frames), `Hero_Mage_Run.png` (64x16, 4 frames), `Hero_Mage_Die.png` (64x16, 4 frames).
   * **Cung thủ (Ranger):** `Hero_Ranger_Idle.png` (32x16, 2 frames), `Hero_Ranger_Run.png` (64x16, 4 frames), `Hero_Ranger_Die.png` (64x16, 4 frames).
2. **Kẻ địch (`Assets/Sprites/Enemies/`):**
   * **Slime:** `Slime_Hop.png`, `Slime_Attack.png`, `Slime_Die.png`.
   * **Skeleton:** `Skeleton_Walk.png`, `Skeleton_Attack.png`, `Skeleton_Die.png`.
   * **Goblin:** `Goblin_Walk.png`, `Goblin_Attack.png`, `Goblin_Die.png`.
   * **Bat:** `Bat_Fly.png`, `Bat_Attack.png`, `Bat_Die.png`.
   * **Boss Golem:** `Boss_Golem_Walk.png`, `Boss_Golem_Attack.png`, `Boss_Golem_Die.png`.
3. **Vũ khí & Vật phẩm:**
   * Vũ khí: `Weapon_HolySword.png`, `Weapon_Fireball.png`, `Weapon_Arrow.png`, `Weapon_MagicStaff.png`, `VFX_Slash.png`.
   * Vật phẩm: `ExpGem_Blue`, `ExpGem_Red`, `ExpGem_Green`, `HealthPotion`, `Magnet`.
4. **Scenes & Prefabs:**
   * Scene chính: `Assets/Scenes/SampleScene.unity`.
   * Prefabs: `Assets/Prefabs/Player.prefab`, `Enemy_Slime.prefab`, `Enemy_Skeleton.prefab`, `Enemy_Goblin.prefab`, `Enemy_Bat.prefab`, `Enemy_BossGolem.prefab`, `ExpGem.prefab`, `Projectile_Fireball.prefab`.
5. **Mã nguồn Scripts (`Assets/Scripts/`):**
   * Player: `PlayerController.cs`, `PlayerStats.cs`, `CameraFollow.cs`.
   * Enemies: `EnemyAI.cs`, `EnemyStats.cs`.
   * Weapons: `WeaponBase.cs`, `Projectile.cs`.
   * Core & UI: `GameManager.cs`, `UIManager.cs`, `ExpGem.cs`.

---

## 🔄 5. QUY TRÌNH QUẢN LÝ MÃ NGUỒN VỚI GIT (GIT WORKFLOW)
* Dự án đã được khởi tạo Git repo kết nối với GitHub.
* Đã cấu hình `.gitignore` chuẩn Unity (loại trừ `Library/`, `Temp/`, `Logs/`, `UserSettings/`).
* **Lệnh Rollback nhanh khi có lỗi:**
  ```powershell
  git restore .
  git clean -fd
  ```
* **Lệnh lưu mốc mới sau khi hoàn thành tính năng:**
  ```powershell
  git add .
  git commit -m "Mô tả tính năng"
  git push origin main
  ```
