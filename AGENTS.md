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
1. **Nhân vật người chơi (6 Heroes - `Assets/Sprites/Characters/`):**
   * **Hiệp sĩ (Sir Roland):** Cận chiến quét kiếm Thánh Kiếm (Holy Broadsword), giáp cao, chống chịu.
   * **Sát thủ Bóng Đêm (Valen / Shadow Knight):** Lưỡi Đao Hắc Ám (Dark Crescent) chém cực nhanh, xuyên giáp.
   * **Pháp sư Nguyên Tố (Ignis / Mage):** Phóng Cầu Lửa Hỏa Ngục (Fireball) tự tìm mục tiêu và nổ AOE.
   * **Đại Pháp Sư Sét (Aurelius / Archmage):** Gậy Thần Bão Tố (Lightning Staff) giáng sấm sét diện rộng.
   * **Cung thủ Bão Gió (Sylph / Ranger):** Cung Tên Bắn Tỉa (Wind Bow) bắn tầm xa xuyên thấu.
   * **Xạ thủ Thần Phong (Aeris / Archer):** Cung Thần Phong (Storm Bow) bắn liên hoàn bão tố siêu tốc.
   * *⚠️ NGUYÊN TẮC BẤT BIẾN VỀ SPRITES NHÂN VẬT:*
     * Giữ nguyên vẹn đúng 27 file spritesheet gốc (9 bộ x 3 trạng thái `die`, `idle`, `run`).
     * **TUYỆT ĐỐI KHÔNG TẠO THÊM FILE PNG ĐƠN LẺ** trong `Assets/Sprites/Characters/`.
     * Toàn bộ ảnh chân dung và slot chọn tướng trong Hero Selection được **cắt trực tiếp từ Frame 0 của hoạt ảnh `_Idle.png`** tương ứng để đảm bảo 100% đồng nhất giữa menu và khi nhân vật di chuyển in-game.

2. **Giao diện Menu Chính & Hub Chọn Màn (`Assets/Scripts/UI/MainMenuManager.cs`):**
   * **Màn hình Tiêu đề (Title Screen):** Tiêu đề động lượn sóng nhẹ, hiệu ứng ánh sáng hào quang, các nút bấm tùy biến (Start, Heroes, Maps, Options) gắn `UIButtonJuice`.
   * **Modal Chọn Hero (Hero Select):** Khung `Frame_HeroSelection.png`, ảnh chân dung lớn bên trái (floating nhẹ), 6 ô slot mini bên dưới hiển thị ảnh thu nhỏ của 6 hero (kèm viền sáng rỗng bao quanh, không che nhân vật), 2 mũi tên chuyển tướng ◀ và ▶ mượt mà, bảng thông tin chỉ số vừa vặn khung giấy da, nút Confirm khớp 100% với nút vẽ sẵn.
   * **Modal Chọn Bản Đồ (Map Select):** Khung `Frame_MapSelection.png`, preview map trung tâm giữ nguyên độ sắc nét pixel, 2 mũi tên chuyển map ◀ và ▶, bảng thông tin giấy da chữ nâu sẫm tương phản cao, hỗ trợ 4 map:
     1. Emerald Meadow (Rừng cỏ xanh - Tiêu chuẩn)
     2. Scorching Desert (Sa mạc bão cát - Thử thách vừa, +30% Vàng)
     3. Castle Ruins (Tàn tích lâu đài - Khó, +50% Vàng)
     4. Infernal Volcano (Núi lửa địa ngục - Ác mộng, +150% Vàng)
   * **Hệ thống Nền Game Vô Tận (`Assets/Scripts/Environment/MapEnvironmentManager.cs`):** Tự động thay thế nền gạch đá cũ bằng hình ảnh bức tranh của map đã chọn, nhân bản thành lưới **5x5 tấm map liền kề** và tự động cuộn vòng vô tận (Infinite Looping) theo bước chân người chơi.
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

---

## 🗺️ 6. LỘ TRÌNH PHÁT TRIỂN & DANH SÁCH USE CASES (ROADMAP & USE CASES - 8 TUẦN)
> **Quy tắc thực hiện:** Triển khai tuần tự từng Use Case theo thứ tự từ UC-01 đến UC-08. Mỗi khi hoàn thành một UC, kiểm tra biên dịch không lỗi và commit local để đánh dấu mốc.

### 📌 GIAI ĐOẠN 1 (Tuần 1 – 2): Chiều Sâu Build Đồ & Hoàn Thiện Vòng Lặp Cốt Lõi
* **UC-01: Hoàn thiện Ma trận Tiến hóa Vũ khí & Bảng Công thức (Evolution Recipe UI)**
  * **Tác nhân (Actor):** Người chơi trong trận đấu.
  * **Mô tả:** Mở rộng hệ thống để toàn bộ 6 vũ khí gốc của 6 tướng đều có nhánh tiến hóa tối thượng khi kết hợp đúng với 1 trong 8 Passive Relics ở cấp tối đa:
    1. `Holy Broadsword` (Sir Roland) + `Iron Armor` / `Crown` $\rightarrow$ **Excalibur**
    2. `Fireball` (Ignis) + `Spell Tome` $\rightarrow$ **Meteor**
    3. `Lightning Staff` (Aurelius) + `Magnet Talisman` $\rightarrow$ **Singularity**
    4. `Wind Bow` (Sylph) + `Wind Quiver` $\rightarrow$ **Tempest Arrow**
    5. `Dark Crescent` (Valen) + `Blood Chalice` $\rightarrow$ **Crimson Reaper** *(Bổ sung mới: Lưỡi hái hút máu theo % sát thương)*
    6. `Storm Bow` (Aeris) + `Clover` $\rightarrow$ **Astral Barrage** *(Bổ sung mới: Mưa tên chí mạng toàn màn hình)*
  * **Trải nghiệm UI:** Khi bảng chọn kỹ năng xuất hiện lúc lên cấp, hiển thị biểu tượng "Gợi ý ghép đồ" dưới các Passive Relics đã nhặt để người chơi không phải nhớ công thức.

* **UC-02: Mở Rương Chiến Lợi Phẩm từ Boss Golem (Boss Chest Roulette)**
  * **Tác nhân:** Người chơi tiêu diệt Boss Golem ở các mốc cao trào trong `WaveManager.cs`.
  * **Luồng xử lý:** Khi Boss Golem bị hạ, rơi ra Rương Kho Báu 16x16. Khi nhặt rương, game tạm dừng và kích hoạt modal mở rương (tận dụng hiệu ứng `colorPulse`, `floating` và âm thanh `pickup` từ `UIButtonJuice.cs`).
  * **Tỷ lệ nổ thưởng:** Dựa vào chỉ số May mắn (Luck) trong `MetaProgression` và cổ vật `Clover`, rương có tỷ lệ nổ 1, 3 hoặc 5 nâng cấp cùng lúc kèm lượng Vàng lớn. Rương là **điều kiện duy nhất** để kích hoạt Tiến hóa Vũ khí.

---

### 📌 GIAI ĐOẠN 2 (Tuần 3 – 4): Tận Dụng Tài Nguyên Ẩn & Hệ Thống Sưu Tầm (Unlock Loop)
* **UC-03: Mở khóa 3 Anh Hùng Ẩn (Secret Heroes Unlock)**
  * **Tác nhân:** Người chơi hoàn thành các cột mốc thử thách tại Menu chọn tướng (`MainMenuManager.cs`).
  * **Kho tài nguyên có sẵn:** Tận dụng 3 bộ spritesheet còn dư trong 27 file gốc (`cung_thu_*`, `hero_*`, `phap_su_*`), cắt trực tiếp Frame 0 của hoạt ảnh `_Idle.png` để hiển thị lên UI Hero Selection (tuân thủ 100% nguyên tắc không tạo thêm file PNG đơn lẻ trong `Assets/Sprites/Characters/`).
  * **Cơ chế:** Ban đầu 3 tướng này hiển thị bóng đen (Silhouette) trong `Frame_HeroSelection.png` kèm điều kiện mở khóa:
    * **Tướng thứ 7:** Sống sót đủ 20 phút tại map Scorching Desert.
    * **Tướng thứ 8:** Tiêu diệt tổng cộng 10 Boss Golem qua các trận đấu.
    * **Tướng thứ 9:** Đạt mốc 50.000 Vàng tích lũy trong phần MetaProgression (hoặc dùng cổ vật Greed Coin đạt cấp tối đa trong 5 trận).

* **UC-04: Sách Cổ Thư & Hệ Thống Thành Tựu (Grimoire & Milestones)**
  * **Tác nhân:** Người chơi truy cập từ Menu chính (`MainMenuManager.cs`).
  * **Luồng xử lý:** Theo dõi thống kê toàn cục (lưu bằng JSON hoặc `PlayerPrefs`): tổng số Slime, Skeleton, Goblin, Dơi, Boss Golem đã tiêu diệt.
  * **Phần thưởng:** Cứ mỗi mốc diệt quái (1.000 $\rightarrow$ 10.000 $\rightarrow$ 50.000), người chơi nhận được Vàng thưởng và mở khóa nội tại ẩn (+2% sát thương lên loại quái đó).

---

### 📌 GIAI ĐOẠN 3 (Tuần 5 – 6): Khác Biệt Hóa 4 Bản Đồ & Hệ Thống Nguyền Rủa (Mid-to-Late Game)
* **UC-05: Tương Tác Môi Trường Đặc Trưng trên 4 Bản Đồ (Map-Specific Hazards)**
  * **Tác nhân:** Hệ thống `MapEnvironmentManager.cs` kết hợp cùng `WaveManager.cs`.
  * **Cơ chế theo map:**
    * **Emerald Meadow (Tiêu chuẩn):** Định kỳ mỗi 3 phút mọc các bụi cây cổ thụ 16x16; phá hủy rơi bình máu hoặc nam châm hút toàn bộ ExpGem.
    * **Scorching Desert (+30% Vàng):** Mỗi 4 phút nổi cơn Bão Cát (Particle System pixel vuông) đẩy lùi nhẹ `rb.linearVelocity` theo hướng gió.
    * **Castle Ruins (+50% Vàng):** Cổ ngữ Triệu Hồi trên mặt đất; đứng trong vòng tròn 10 giây triệu hồi Tinh anh Skeleton/Goblin rơi lượng lớn Vàng và ExpGem.
    * **Infernal Volcano (+150% Vàng):** Vệt dung nham nứt cảnh báo trước 2 giây rồi phun lửa gây sát thương cho cả người chơi lẫn quái vật (dụ quái/boss vào lửa).

* **UC-06: Hệ Thống Cấp Độ Nguyền Rủa (Curse / Torment Tiers I – X)**
  * **Tác nhân:** Người chơi chọn độ khó bổ sung tại Modal Chọn Bản Đồ (`Frame_MapSelection.png`) trước khi vào trận.
  * **Cơ chế:** Bật từ Bậc 1 đến Bậc 10 Nguyền Rủa trên mỗi map:
    * Mỗi bậc tăng +15% máu/tốc độ quái, tăng số lượng quái sinh ra từ `WaveManager.cs`, hoặc triệu hồi 2 Boss Golem cùng lúc.
    * Bù lại, mỗi bậc cộng thêm +25% Vàng và Kinh nghiệm (4 map x 10 bậc = 40 cấp độ thử thách).

---

### 📌 GIAI ĐOẠN 4 (Tuần 7 – 8): Nội Dung Cuối Game (Endgame) & Chơi Lại Vô Hạn
* **UC-07: Chế Độ Vô Tận (Endless Mode) & Săn Tiền Thưởng Vàng (Gold Fever)**
  * **Tác nhân:** Người chơi sống sót qua mốc 20 phút của `WaveManager.cs`.
  * **Cơ chế:** Ở phút 20 hiển thị lựa chọn: "Rút lui an toàn (Victory)" hoặc "Bước vào Hư Không (Endless)".
  * Từ phút 20 trở đi, quái tăng cấp số nhân về máu và tốc độ. Mọi trang bị đạt cấp tối đa có thể mở khóa Limit Break bằng Vàng ngay trong trận.
  * Lưu kỷ lục Thời gian sống sót và DPS của từng tướng lên màn hình Hero Select.

* **UC-08: Thử Thách Biến Số Hàng Ngày (Daily Modifier Run)**
  * **Tác nhân:** Chế độ "Thử thách Hôm nay" tại Menu chính.
  * **Cơ chế:** Sử dụng `System.DateTime.Now.Date` làm Random Seed tạo bài toán cố định mỗi ngày (ví dụ: Ignis tại Castle Ruins với quái x2 tốc độ, khởi đầu có sẵn Spell Tome & Magnet Talisman; hoặc Sir Roland 1 HP nhưng gấp đôi Giáp).
  * Phần thưởng x3 Vàng.

---

### 💡 CƠ CHẾ TIÊU VÀNG META-PROGRESSION (GOLD SINKS TRÁNH LẠM PHÁT)
1. **Quyền Năng Thao Túng Lên Cấp:** Mua thêm Đổ lại (Reroll), Phong ấn (Banish), Khóa (Lock) trong Shop Meta.
2. **Rèn Tinh Thông Vũ Khí Khởi Đầu (Weapon Mastery):** Nâng cấp vũ khí riêng từng Hero từ Đồng $\rightarrow$ Bạc $\rightarrow$ Vàng $\rightarrow$ Huyền Thoại (thêm hiệu ứng đẩy lùi, xuyên thấu, nổ lan).
3. **Bùa Chú Khởi Đầu (Consumable Run Blessings):** Tiêu Vàng mua bùa dùng 1 lần trước trận (bắt đầu Level 3, tăng 50% tỉ lệ rơi ExpGem hiếm).
