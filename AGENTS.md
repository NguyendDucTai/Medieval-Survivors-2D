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

## 📁 4. CẤU TRÚC ASSETS & SCRIPTS HIỆN TẠI (UP-TO-DATE ARCHITECTURE)
1. **Nhân vật người chơi (5 Heroes - `Assets/Sprites/Characters/`):**
   * **Hiệp sĩ (Sir Roland):** Cận chiến quét kiếm Thánh Kiếm (Holy Broadsword), giáp cao, chống chịu.
   * **Sát thủ Bóng Đêm (Valen / Shadow Knight):** Lưỡi Đao Hắc Ám (Dark Crescent) chém cực nhanh, xuyên giáp.
   * **Pháp sư Nguyên Tố (Ignis / Mage):** Phóng Cầu Lửa Hỏa Ngục (Fireball) tự tìm mục tiêu và nổ AOE.
   * **Đại Pháp Sư Sét (Aurelius / Archmage):** Gậy Thần Bão Tố (Lightning Staff) giáng sấm sét diện rộng.
   * **Cung thủ Bão Gió (Sylph / Ranger / Archer):** Cung Tên Bão Tố (Wind Bow / Storm Bow) bắn tầm xa xuyên thấu.
   * *Lưu ý ảnh chân dung Hero Selection:* Ưu tiên lấy ảnh chân dung 1 frame (`Hero_Knight.png`, `Hero_Mage.png`, `Hero_Ranger.png`). Nếu hero không có ảnh chân dung riêng lẻ (Rogue, Archmage), code tự động lấy Frame 0 từ hoạt ảnh `Idle` với `FilterMode.Point`.

2. **Giao diện Menu Chính & Hub Chọn Màn (`Assets/Scripts/UI/MainMenuManager.cs`):**
   * **Màn hình Tiêu đề (Title Screen):** Tiêu đề động lượn sóng nhẹ, hiệu ứng ánh sáng hào quang, các nút bấm tùy biến (Start, Heroes, Maps, Options) gắn `UIButtonJuice`.
   * **Modal Chọn Hero (Hero Select):** Khung `Frame_HeroSelection.png`, ảnh chân dung lớn bên trái (floating nhẹ, không breathing để giữ pixel sắc nét), 5 ô slot mini bên dưới hiển thị ảnh thu nhỏ của 5 hero, bảng thông tin chỉ số/đặc tính chữ tự xuống dòng (`Wrap`) vừa vặn khung giấy da, nút Confirm khớp 100% với nút vẽ sẵn.
   * **Modal Chọn Bản Đồ (Map Select):** Khung `Frame_MapSelection.png`, preview map trung tâm giữ nguyên độ sắc nét pixel (không rung lắc/thở), 2 mũi tên chuyển map ◀ và ▶, bảng thông tin giấy da chữ nâu sẫm tương phản cao, hỗ trợ 3 map:
     1. Emerald Meadow (Rừng cỏ xanh - Tiêu chuẩn)
     2. Castle Ruins (Tàn tích lâu đài - Khó, +50% Vàng)
     3. Infernal Volcano (Núi lửa địa ngục - Ác mộng, +150% Vàng)
   * **Modal Cài Đặt (Options):** Khung `Frame_Options.png`, thanh trượt chỉnh âm lượng Nhạc nền (Music) và Hiệu ứng (SFX), nút gạt Tắt tiếng (Mute), hướng dẫn phím điều khiển `[WASD / ESC / Chuột]`.

3. **Hệ thống Hiệu ứng & Nước ép Game (`Assets/Scripts/UI/UIButtonJuice.cs`):**
   * Tự động tạo phản hồi xúc giác cho UI: phóng to khi rê chuột (`hoverScale`), nảy khi nhấn (`pressScale`), phát âm thanh (`click`, `pickup`), thở (`breathing`), chuyển động lắc lư (`floating`), nhấp nháy hào quang (`colorPulse`).

4. **Hệ thống Gameplay Nâng Cao (Meta-Progression & Wave Director):**
   * **Meta-Progression (`Assets/Scripts/MetaProgression/`):** Nâng cấp chỉ số vĩnh viễn trong Shop bằng Vàng kiếm được sau mỗi trận (Sát thương, Giáp, Máu tối đa, Tốc độ, Nhặt đồ, May mắn).
   * **Tiến hóa Vũ khí (Weapon Evolutions):** Vũ khí tối thượng khi kết hợp trang bị (Excalibur, Meteor, Singularity, Tempest Arrow).
   * **8 Cổ vật Bị động (Passive Relics - `Assets/Sprites/Passives/`):** Iron Armor, Wind Quiver, Spell Tome, Clover, Crown, Blood Chalice, Greed Coin, Magnet Talisman.
   * **Đạo diễn Đợt Quái (`Assets/Scripts/Wave/WaveManager.cs`):** Thiết kế đợt quái tự động tăng tiến độ khó trong 20 phút sinh tồn, triệu hồi Boss Golem ở các mốc thời gian cao trào.

5. **Camera & Vật lý:**
   * `CameraFollow.cs`: Tách biệt hoàn toàn độ rung chấn thương (Trauma shake) khỏi tọa độ camera, bật nội suy `Rigidbody2D.interpolation` giúp camera di chuyển siêu mượt, không giật hình.

---

## 🔄 5. QUY TRÌNH QUẢN LÝ MÃ NGUỒN VỚI GIT (GIT WORKFLOW)
* Dự án đã được đồng bộ với GitHub: `https://github.com/NguyendDucTai/Medieval-Survivors-2D`
* Đã cấu hình `.gitignore` chuẩn Unity (loại trừ `Library/`, `Temp/`, `Logs/`, `UserSettings/`).
* Nhánh chính: `main`. Toàn bộ tính năng đã được đẩy lên GitHub qua 5 commits chuẩn `feat(...)`.
* ⚠️ **QUY TẮC PUSH GITHUB:**
  * **TUYỆT ĐỐI KHÔNG TỰ Ý CHẠY `git push` LÊN GITHUB NẾU NGƯỜI DÙNG CHƯA YÊU CẦU.**
  * Chỉ commit ở local branch khi hoàn thành tính năng.
  * **CHỈ ĐƯỢC PUSH khi người dùng yêu cầu trực tiếp** (ví dụ: "push lên github đi", "push code").
* **Lệnh Rollback nhanh khi có lỗi:**
  ```powershell
  git restore .
  git clean -fd
  ```
* **Lệnh lưu mốc mới ở local:**
  ```powershell
  git add .
  git commit -m "feat/fix: mô tả tính năng"
  ```
