# 🛡️ PRU213 - ĐỒ ÁN GAME 2D: MEDIEVAL SURVIVORS 2D
> **Môn học:** PRU213 - Lập trình Game 2D với Unity (FPT University)  
> **Thể loại:** 2D Top-Down Roguelike / Survival (Phong cách *Vampire Survivors*)  
> **Công nghệ:** Unity 6 (6000.4.12f1) | URP 2D | 16x16 Retro Pixel Art  

---

## 📌 THÔNG TIN SINH VIÊN & BÀI NỘP

* **Họ và tên:** Nguyễn Đức Tài
* **Lớp:** SE1969
* **Kho lưu trữ GitHub (Source Code):** [https://github.com/NguyendDucTai/Medieval-Survivors-2D](https://github.com/NguyendDucTai/Medieval-Survivors-2D)
* **🌐 LINK MINH CHỨNG GEMINI WEB (AI Assistance):**  
  👉 **[https://share.gemini.google/tD9FdoKm3v4b](https://share.gemini.google/tD9FdoKm3v4b)**  
  *(Minh chứng quá trình trao đổi prompt, tư duy thuật toán và xây dựng tính năng theo yêu cầu môn học)*

---

## 🎮 HƯỚNG DẪN KHỞI CHẠY DỰ ÁN TRONG UNITY

1. **Phiên bản Unity khuyến nghị:** Unity 6 (`6000.4.12f1`) hoặc mới hơn (URP 2D).
2. **Khởi động dự án:**
   * Mở **Unity Hub** -> Chọn **Add project from disk** -> Trỏ đến thư mục dự án đã giải nén.
   * Chờ Unity tự động biên dịch lại thư mục `Library` và import packages (URP, Input System, 2D Tilemap).
3. **Mở Scene Chơi Game:**
   * Trong cửa sổ `Project`, điều hướng đến: **`Assets / Scenes / SampleScene.unity`**.
   * Nhấn nút **▶ Play** trên thanh công cụ Unity để trải nghiệm game.

---

## 🕹️ BẢNG ĐIỀU KHIỂN & PHÍM TẮT (CONTROLS)

| Phím / Thao tác | Chức năng |
| :--- | :--- |
| **W, A, S, D** / **Phím Mũi Tên** | Di chuyển nhân vật 8 hướng linh hoạt |
| **Enter** / **Space** | Bắt đầu trận chiến ngay từ Menu chính |
| **H** | Mở bảng Modal Chọn Tướng (Hero Selection) |
| **M** | Mở bảng Modal Chọn Bản Đồ (Map Selection) |
| **O** | Mở bảng Cài đặt Âm lượng & Hướng dẫn (Options) |
| **ESC** | Đóng popup / Thoát bảng chọn |
| **Phím 1 - 6 (In-game)** | Phím tắt nhanh để đổi trực tiếp qua lại giữa các Hero để chấm điểm/test vũ khí |
| **Chuột Trái (Left Click)** | Nhấn nút giao diện UI, chọn kỹ năng khi Lên cấp (Level Up) |

---

## 🌟 CÁC TÍNH NĂNG NỔI BẬT ĐÃ HOÀN THIỆN (KEY FEATURES)

### 1. Hệ thống Nhân vật (5 Heroes Độc Đáo)
* **Sir Roland (Hiệp sĩ):** Cận chiến Thánh Kiếm (*Holy Broadsword*), giáp dày, chịu đòn cực tốt.
* **Valen (Sát thủ bóng đêm):** Lưỡi Đao Hắc Ám (*Dark Crescent*), tốc độ di chuyển phi phàm, chém xuyên giáp.
* **Ignis (Pháp sư nguyên tố):** Cầu Lửa Hỏa Ngục (*Fireball*), tự tìm mục tiêu và nổ AOE diện rộng.
* **Aurelius (Đại pháp sư sét):** Gậy Thần Bão Tố (*Lightning Staff*), giáng sấm sét hủy diệt từ trên cao.
* **Sylph (Cung thủ bão gió):** Cung Thần Phong (*Storm Bow*), bắn mũi tên xuyên thấu liên hoàn tốc độ cao.

### 2. Kho Vũ khí & Tiến Hóa Vũ Khí Tối Thượng (Weapon Evolutions)
* **6 Vũ khí cơ bản** kết hợp cùng **8 Cổ vật bị động (Passive Relics)**:
  * Thánh Kiếm + Giáp Sắt $\rightarrow$ **Excalibur (Thánh Kiếm Hoàng Gia)**
  * Cầu Lửa + Sách Phép $\rightarrow$ **Meteor Strike (Mưa Thiên Thạch)**
  * Gậy Bão Tố + Nhẫn Ma Thuật $\rightarrow$ **Singularity (Hố Đen Hư Không)**
  * Cung Bão Tố + Ống Tên Thần Gió $\rightarrow$ **Tempest Barrage (Bão Tố Cuồng Phong)**
  * Lưỡi Đao Hắc Ám + Chén Máu $\rightarrow$ **Void Scythe (Lưỡi Hái Hư Không)**

### 3. Đạo diễn Đợt Quái (Wave Director) & Hệ Thống Boss
* Quản lý tiến trình sinh tồn trong 20 phút với tốc độ ra quái tăng tiến liên tục.
* Các sự kiện cao trào: Bầy dơi đột kích (phút 2.5), Hồn ma cổ xưa xâm lăng (phút 3.5), v.v.
* **Boss Golem Đá Nham Thạch:** Máu trâu, có kỹ năng gồng mình vệt đỏ (Telegraph) lướt húc cực mạnh và làm rung chuyển màn hình.

### 4. Hệ Thống Bản Đồ (3 Maps)
1. **Emerald Meadow (Rừng cỏ xanh):** Địa hình thoáng đãng, hệ số Vàng x1.0.
2. **Castle Ruins (Tàn tích lâu đài):** Quái đông hơn 30%, hành lang đá hẹp, hệ số Vàng x1.5.
3. **Infernal Volcano (Núi lửa địa ngục):** Quái trâu và hung hãn gấp đôi, hệ số Vàng x2.5.

### 5. Meta-Progression & Lưu Trữ Dữ Liệu
* **Shop Nâng Cấp Vĩnh Viễn:** Dùng Vàng kiếm được sau mỗi trận để nâng Máu, Giáp, Tốc độ, Sát thương, Nam châm nhặt đồ, Tỷ lệ May mắn, và Hồi sinh.
* **Hệ thống Thành Tựu (Achievements):** Mở khóa phần thưởng khi đạt mốc sống sót và tích lũy vàng.
* **Lưu trữ JSON tự động:** Đồng bộ trạng thái save game liên tục vào `PlayerPrefs`.

### 6. Cảm Giác Game (Game Juice & Polish)
* **CameraFollow:** Nội suy camera mượt mà, phân tách độc lập độ rung chấn thương (Trauma shake) giúp không bị giật lag khung hình.
* **UI Juice:** Nút bấm có hiệu ứng hover scale, click bounce, nhấp nháy hào quang sống động.
* **Floating Damage Text:** Hiển thị sát thương nảy số với Object Pooling tối ưu RAM.
* **AudioManager:** Âm thanh SFX đầy đủ cho các hành động chém, bắn, nhặt ngọc, nổ, tử trận, lên cấp.
