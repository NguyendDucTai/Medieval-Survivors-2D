# BÁO CÁO TỔNG QUAN TÀI NGUYÊN (ASSET CATALOG & SUBMISSION REPORT)
## Môn học: PRU213 - Xây Dựng Dự Án 2D Game Roguelike Vampire Survivors
**Họ và tên sinh viên / Nhóm thực hiện:** PRU213 Game Dev Team  
**Định dạng chuẩn dự án:** 2D Top-Down Retro Pixel Art, Point Filter (no blur), 16 Pixels Per Unit (PPU).  
**Tập tin nén gửi kèm:** `PRU213_Assets_Submission.zip`

---

## 1. MỤC TIÊU VÀ NGUYÊN TẮC THIẾT KẾ ĐỒ HỌA
1. **Thống nhất độ phân giải (Pixel Uniformity):** Toàn bộ nhân vật, quái vật, vũ khí và vật phẩm tuân thủ lưới pixel chuẩn 16x16 (Boss 32x32 hoặc 16x16 grid), đảm bảo PPU = 16.
2. **Độ sắc nét & tương phản cao (High-Contrast Bold Pixel Art):** Không sử dụng dải màu mờ ảo subpixel hay bilinear filter gây nhoè hình. Mọi chi tiết đều có đường viền (outline) sẫm màu, màu sắc nổi bật, độ bão hòa cao đúng phong cách Arcade 16-bit.
3. **Hoạt ảnh hoàn chỉnh (Frame-by-Frame Animations):**
   - **Nhân vật người chơi:** Hoạt ảnh Đứng im (Idle - 2 frames), Di chuyển (Run - 4 frames), và Tử trận (Die - 4 frames).
   - **Kẻ địch:** Đầy đủ hoạt ảnh Di chuyển (Move / Hop / Fly - 4 frames), Tấn công (Attack - 4 frames), và Bị tiêu diệt (Die - 4 frames).
4. **Hệ thống Animation Controller chuẩn Unity:** Toàn bộ SpriteSheet được cắt tự động, tích hợp sẵn Animator State Machine (`Idle`, `Run`, `Move`, `Attack`, `Die`) với AnyState Transition tức thì.

---

## 2. CẤU TRÚC THƯ MỤC DỰ ÁN (PROJECT STRUCTURE)

```text
Assets/
├── Sprites/
│   ├── Characters/      # Hero_Knight, Hero_Mage, Hero_Ranger (Idle, Run, Die 64x16)
│   ├── Enemies/         # Slime, Skeleton, Goblin, Bat, Boss Golem (Walk, Attack, Die 64x16)
│   ├── Weapons/         # HolySword, Fireball, Staff, Arrow, Slash VFX
│   ├── Collectibles/    # ExpGem (5 loại), HealthPotion, ManaPotion, Chest, Magnet, Coin
│   ├── Environment/     # Grass, Stone Path, Tree, Rock Obstacles
│   └── UI/              # WoodFrame Panel, HealthBar, ExpBar, Button, Heart
├── Animations/
│   ├── Characters/      # Knight, Mage, Ranger (.anim & .controller có trạng thái Die)
│   └── Enemies/         # Slime, Skeleton, Goblin, Bat, Boss Golem (.anim & .controller đủ Move, Attack, Die)
├── Audio/
│   └── SFX/             # sfx_shoot, sfx_hit, sfx_gem_pickup, sfx_levelup, sfx_player_death
├── Prefabs/             # Player, Enemy_Slime, Skeleton, Goblin, Bat, BossGolem, ExpGem, Fireball
├── Scenes/              # SampleScene.unity
└── Scripts/             # Toàn bộ mã nguồn C# điều khiển gameplay, AI và Animation
```

---

## 3. DANH MỤC CHI TIẾT TÀI NGUYÊN ĐỒ HỌA (SPRITE CATALOG)

### 3.1. Nhân Vật Người Chơi (Characters) & Bộ Hoạt Ảnh SpriteSheets
| Tên File | Kích thước | PPU | Chế độ Sprite | Vai trò & Hoạt ảnh trong Gameplay |
| :--- | :---: | :---: | :---: | :--- |
| `Hero_Knight_Idle.png` | 32x16 | 16 | Multiple (2 frames) | **Hiệp Sĩ - Đứng im (Idle)**: Thở nhấp nhô, tư thế cảnh giác giáp bạc áo choàng đỏ. |
| `Hero_Knight_Run.png` | 64x16 | 16 | Multiple (4 frames) | **Hiệp Sĩ - Di chuyển (Run)**: Bước sải chân dũng mãnh, áo choàng đỏ bay dập dờn. |
| `Hero_Knight_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Hiệp Sĩ - Tử trận (Die)**: Loạng choạng ngã khụy gối, gục ngã và thanh kiếm cắm cọc vinh quang. |
| `Hero_Mage_Idle.png` | 32x16 | 16 | Multiple (2 frames) | **Pháp Sư - Đứng im (Idle)**: Lơ lửng thở nhẹ, quả cầu ma thuật phát sáng xanh biếc. |
| `Hero_Mage_Run.png` | 64x16 | 16 | Multiple (4 frames) | **Pháp Sư - Di chuyển (Run)**: Tà áo choàng tím bay ngược, lướt ma thuật tiến lên. |
| `Hero_Mage_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Pháp Sư - Tử trận (Die)**: Mất kiểm soát mana, nón rơi và tan biến thành các hạt bụi ma thuật. |
| `Hero_Ranger_Idle.png` | 32x16 | 16 | Multiple (2 frames) | **Xạ Thủ - Đứng im (Idle)**: Lông vũ đỏ trên nón đung đưa, tư thế cung thủ canh gác. |
| `Hero_Ranger_Run.png` | 64x16 | 16 | Multiple (4 frames) | **Xạ Thủ - Di chuyển (Run)**: Bước chạy phóng khoáng, dẻo dai của thợ săn rừng xanh. |
| `Hero_Ranger_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Xạ Thủ - Tử trận (Die)**: Trúng đòn gục ngã, buông cung và nằm yên nghỉ trên thảm cỏ. |

### 3.2. Kẻ Địch (Enemies) & Trọn Bộ Hoạt Ảnh (Di chuyển - Tấn công - Tử trận)
| Tên File | Kích thước | PPU | Chế độ Sprite | Hành vi, Hoạt ảnh & Trạng thái |
| :--- | :---: | :---: | :---: | :--- |
| **SLIME** | | | | |
| `Slime_Hop.png` | 64x16 | 16 | Multiple (4 frames) | **Di chuyển (Move)**: Co giãn đàn hồi nhún nhảy áp sát người chơi. |
| `Slime_Attack.png` | 64x16 | 16 | Multiple (4 frames) | **Tấn công (Attack)**: Phồng to cơ thể, gai nhớt bung ra gây sát thương va chạm. |
| `Slime_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Tử trận (Die)**: Xẹp lún bong bóng nhớt, vỡ nát thành vũng dịch thể tan biến. |
| **SKELETON** | | | | |
| `Skeleton_Walk.png` | 64x16 | 16 | Multiple (4 frames) | **Di chuyển (Move)**: Khung xương trắng rảo bước hành quân tiến công. |
| `Skeleton_Attack.png` | 64x16 | 16 | Multiple (4 frames) | **Tấn công (Attack)**: Giơ cao cánh tay xương vuốt nhọn bổ chém người chơi. |
| `Skeleton_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Tử trận (Die)**: Các đốt sống gãy vụn, hộp sọ rụng và tan thành đống xương trắng. |
| **GOBLIN** | | | | |
| `Goblin_Walk.png` | 64x16 | 16 | Multiple (4 frames) | **Di chuyển (Move)**: Khom lưng lén lút chạy nhanh bám đuổi người chơi. |
| `Goblin_Attack.png` | 64x16 | 16 | Multiple (4 frames) | **Tấn công (Attack)**: Giơ gậy gỗ gộc đập mạnh về phía trước. |
| `Goblin_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Tử trận (Die)**: Bật ngửa ra sau, rơi gậy và nằm bất động trên đất. |
| **SHADOW BAT** | | | | |
| `Bat_Fly.png` | 64x16 | 16 | Multiple (4 frames) | **Di chuyển (Fly)**: Đập cánh nhịp nhàng lượn sóng ziczac trên không. |
| `Bat_Attack.png` | 64x16 | 16 | Multiple (4 frames) | **Tấn công (Attack)**: Bổ nhào cắm răng nanh đỏ hút máu người chơi. |
| `Bat_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Tử trận (Die)**: Cánh khép quặt, rơi xoay vòng xuống đất và tan thành làn khói đen. |
| **BOSS GOLEM** | | | | |
| `Boss_Golem_Walk.png` | 64x16 | 16 | Multiple (4 frames) | **Di chuyển (Move)**: Thân đá khổng lồ dậm bước nặng nề, lõi cổ ngữ nhấp nháy. |
| `Boss_Golem_Attack.png` | 64x16 | 16 | Multiple (4 frames) | **Tấn công (Attack)**: Giơ hai nắm đấm đá khổng lồ nện đất tạo sóng xung kích cổ ngữ. |
| `Boss_Golem_Die.png` | 64x16 | 16 | Multiple (4 frames) | **Tử trận (Die)**: Lõi năng lượng tắt ngúm, cơ thể nứt toác và sụp đổ thành đống đá cổ. |

### 3.3. Vũ Khí, Đạn & Hiệu Ứng (Weapons & VFX)
| Tên File | Kích thước | PPU | Tính năng kỹ năng |
| :--- | :---: | :---: | :--- |
| `Weapon_HolySword.png` | 16x16 | 16 | Kiếm ánh sáng xoay quanh nhân vật chém gạt quái áp sát. |
| `Weapon_Fireball.png` | 16x16 | 16 | Quả cầu lửa tự động nhắm bắn quái gần nhất, nổ xuyên thấu. |
| `Weapon_MagicStaff.png` | 16x16 | 16 | Gậy phép bắn ra các tia ma thuật tím theo nhiều hướng. |
| `Weapon_Arrow.png` | 16x16 | 16 | Mũi tên gỗ bay thẳng với vận tốc cao và độ chính xác tuyệt đối. |
| `VFX_Slash.png` | 16x16 | 16 | Hiệu ứng vệt chém bán nguyệt ánh sáng xanh khi tấn công. |

### 3.4. Vật Phẩm Thu Thập (Collectibles) - Đã Remaster Nét & Đẹp Chuẩn Retro Pixel Art
| Tên File | Kích thước | PPU | Giá trị / Hiệu ứng |
| :--- | :---: | :---: | :--- |
| `ExpGem_Blue.png` | 16x16 | 16 | Ngọc kinh nghiệm lam (+10 EXP) - Cắt giác đa diện, viền tương phản cao & đốm sáng lấp lánh. |
| `ExpGem_Green.png` | 16x16 | 16 | Ngọc kinh nghiệm lục (+25 EXP) - Ngọc lục bảo từ quái Goblin. |
| `ExpGem_Red.png` | 16x16 | 16 | Ngọc kinh nghiệm đỏ (+50 EXP) - Hồng ngọc bảo thạch từ quái Skeleton. |
| `ExpGem_Purple.png` | 16x16 | 16 | Ngọc kinh nghiệm tím (+100 EXP) - Thạch anh tím từ quái Shadow Bat. |
| `ExpGem_Gold.png` | 16x16 | 16 | Ngọc kinh nghiệm hoàng kim (+250 EXP) - Kim cương citrine từ Boss Golem. |
| `HealthPotion.png` | 16x16 | 16 | Bình dược máu hồi phục (+35 HP) - Chai thủy tinh cong, nút bần, ánh quang & bọt khí nổi. |
| `ManaPotion.png` | 16x16 | 16 | Bình dược ma năng (+50 Mana / Speed) - Thuốc ma pháp xanh biếc huyền ảo. |
| `Chest_Gold.png` | 16x16 | 16 | Rương báu vật hoàng kim - Nắp vòm gỗ sồi, nẹp kim loại vàng, ổ khóa & khe mở 3D. |
| `Magnet.png` | 16x16 | 16 | Nam châm ma thuật - Cực nam mạ bạc chrome, thân đỏ sơn bóng & tia sét điện từ tích hợp. |
| `Coin_Gold.png` | 16x16 | 16 | Đồng tiền vàng cổ tích lũy - Điêu khắc viền nổi và ngôi sao ánh kim. |

### 3.5. Môi Trường (Environment)
| Tên File | Kích thước | PPU | Ứng dụng bản đồ |
| :--- | :---: | :---: | :--- |
| `Grass_Tile.png` | 16x16 | 16 | Thảm cỏ xanh liền mạch (Seamless Tile) làm nền map vô tận. |
| `Stone_Path_Tile.png` | 16x16 | 16 | Gạch lát đường đá cổ tạo lối mòn và khu vực đặc biệt. |
| `Tree_Obstacle.png` | 32x32 | 16 | Cây cổ thụ bóng râm làm chướng ngại vật cản đường di chuyển. |
| `Rock_Obstacle.png` | 16x16 | 16 | Tảng đá cuội tự nhiên có gắn Collider cản quái vật và đạn. |

### 3.6. Giao Diện Người Dùng (User Interface)
| Tên File | Kích thước | PPU | Chức năng UI |
| :--- | :---: | :---: | :--- |
| `WoodFrame_Panel.png` | 32x32 | 16 | Khung gỗ phong cách Trung Cổ (9-Slice) cho bảng Level-Up & Pause. |
| `HealthBar_Fill.png` | 32x8 | 16 | Thanh hiển thị máu người chơi (màu đỏ tươi có viền bóng). |
| `ExpBar_Fill.png` | 32x8 | 16 | Thanh hiển thị tiến độ thăng cấp (màu xanh ngọc lam). |
| `Button_Normal.png` | 32x16 | 16 | Nút bấm chọn kỹ năng nâng cấp và khởi động lại game. |
| `Heart_Icon.png` | 16x16 | 16 | Biểu tượng mạng sống / chỉ số sinh lực trên HUD. |

---

## 4. DANH MỤC ÂM THANH HIỆU ỨNG (AUDIO SFX)

| Tên File | Định dạng | Thời lượng | Kênh / Tần số | Sự kiện kích hoạt (Event) |
| :--- | :---: | :---: | :---: | :--- |
| `sfx_shoot.wav` | 16-bit PCM WAV | 0.15s | Mono 22.05 kHz | Kích hoạt khi Player bắn đạn cầu lửa / phóng tên. |
| `sfx_hit.wav` | 16-bit PCM WAV | 0.12s | Mono 22.05 kHz | Kích hoạt khi đạn trúng quái hoặc người chơi nhận sát thương. |
| `sfx_gem_pickup.wav` | 16-bit PCM WAV | 0.20s | Mono 22.05 kHz | Kích hoạt khi ngọc EXP bay vào người chơi và được thu thập. |
| `sfx_levelup.wav` | 16-bit PCM WAV | 0.45s | Mono 22.05 kHz | Kích hoạt khi thanh EXP đầy và xuất hiện bảng chọn nâng cấp. |
| `sfx_player_death.wav` | 16-bit PCM WAV | 0.80s | Mono 22.05 kHz | Kích hoạt khi người chơi cạn máu (Game Over). |

---

## 5. HƯỚNG DẪN TEST & KIỂM TRA TRONG UNITY EDITOR

1. **Kiểm tra SpriteSheet Slicing:** Mở Unity Editor -> Menu `Tools` -> Bấm `Cắt và Gộp SpriteSheets Toàn Bộ Nhân Vật & Enemies`.
2. **Kiểm tra hoạt ảnh Nhân vật:**
   - Menu `Tools / Đổi Nhân Vật Thành: Hiệp Sĩ (Knight)`
   - Menu `Tools / Đổi Nhân Vật Thành: Pháp Sư (Mage)`
   - Menu `Tools / Đổi Nhân Vật Thành: Xạ Thủ (Ranger)`
   - Bấm Play (▶️): Bấm W/A/S/D để xem hoạt ảnh Chạy (Run), buông tay để xem Đứng im (Idle). Khi cạn máu sẽ tự động kích hoạt hoạt ảnh Tử trận (Die).
3. **Kiểm tra hoạt ảnh Enemies:**
   - Mở cửa sổ Animation (`Window -> Animation -> Animation`), chọn quái vật bất kỳ trong scene hoặc thư mục `Assets/Prefabs/` để xem từng clip `Move`, `Attack`, `Die`.
   - Khi quái vật chạm vào người chơi, hoạt ảnh `Attack` sẽ kích hoạt theo nhịp đánh. Khi quái vật hết máu, hoạt ảnh `Die` sẽ phát trọn vẹn trước khi biến mất và rơi ra ExpGem.