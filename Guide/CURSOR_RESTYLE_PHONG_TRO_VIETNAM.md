# CURSOR TASK: Rebuild / Restyle phòng trọ Việt Nam - Unity Low Poly

## Mục tiêu
Restyle lại căn phòng hiện tại thành **phòng trọ sinh viên Việt Nam**, ấm, đời thường, có ánh sáng tự nhiên từ cửa sổ. Không rebuild gameplay logic. Chỉ chỉnh môi trường, vật thể, ánh sáng, collider, vị trí asset.

## Quy tắc bắt buộc
- Không để bất kỳ object nào lơ lửng.
- Không để object overlap nhau.
- Không đặt item xuyên tường, xuyên sàn, xuyên bàn, xuyên giường.
- Mọi object đặt trên sàn phải có đáy chạm mặt sàn.
- Mọi object đặt trên bàn/giường/kệ phải có đáy chạm đúng mặt phẳng bên dưới.
- Giữ camera gameplay top-down hiện tại, nhưng bố cục phải đọc rõ hơn.
- Không sửa UI gameplay, không sửa logic collect item, không sửa script nếu không cần.
- Sau khi đặt object, kiểm tra bằng Scene View ở nhiều góc.

---

# 1. Floor - nền gạch hoa văn Việt Nam

## Yêu cầu visual
Thay nền tối hiện tại bằng nền **gạch bông/gạch men Việt Nam** kiểu phòng trọ cũ.

Style:
- Gạch vuông 40x40 hoặc 50x50.
- Màu nền: kem, vàng nhạt, nâu đất, xanh xám nhạt.
- Hoa văn đơn giản: hình thoi, hoa 4 cánh, đường viền lặp lại.
- Không quá rối, vì gameplay cần nhìn rõ item.

## Cách dựng kỹ thuật trong Unity
Tạo vật liệu mới:
- Name: `MAT_Vietnamese_Pattern_Tile_Floor`
- Base color: warm cream / light beige.
- Smoothness thấp, khoảng 0.25 - 0.35.
- Metallic = 0.

Nếu chưa có texture:
- Tạo pattern bằng mesh tile lặp lại hoặc dùng texture procedural đơn giản.
- Có thể tạo các plane vuông mỏng đặt sát sàn, mỗi tile là một ô.
- Tile phải nằm đúng trên mặt sàn, không cao quá 0.01 unit để tránh z-fighting.

Nếu dùng texture:
- Texture repeat/tilling: 6x6 hoặc 8x8 tùy kích thước phòng.
- Pattern phải align theo trục X/Z của căn phòng.

## Không được
- Không dùng nền đen hoặc nền gỗ tối cho toàn phòng.
- Không dùng pattern quá dày khiến item bị chìm.

---

# 2. Portrait / meme wall - ảnh treo tường không viền

## Mục tiêu
Dựng một khu vực tường có các ảnh portrait/meme Việt Nam lấy từ thư mục:

`Assets/Images/Portrait/`

Ví dụ ảnh: Anh Độ Mixi, Rì Đỗ, hoặc các ảnh meme người dùng bỏ vào folder đó.

## Cách dựng kỹ thuật
Với mỗi ảnh trong `Assets/Images/Portrait/`:
1. Import texture as Sprite hoặc Texture2D.
2. Tạo Quad/Plane mỏng trên tường.
3. Gán material Unlit hoặc Standard với texture ảnh.
4. Không tạo frame/viền.
5. Đặt ảnh sát tường nhưng không xuyên tường.

Thông số đề xuất:
- Offset khỏi tường: 0.015 - 0.03 unit.
- Scale portrait: khoảng 0.45m x 0.65m hoặc tùy tỉ lệ ảnh.
- Height từ sàn: tâm ảnh ở khoảng 1.45m - 1.65m.
- Rotation phải song song với mặt tường.

## Vị trí đề xuất
- Tường bên phải gần bàn học hoặc tường trống dễ nhìn từ camera.
- Sắp xếp dạng collage 2 hàng, 3 cột nếu có nhiều ảnh.

## Kiểm tra
- Không để ảnh bay khỏi tường.
- Không để ảnh chồng lên cửa sổ, tủ, kệ.
- Không để ảnh bị ngược mặt hoặc bị mirror sai.

---

# 3. Vali mở ở giữa phòng

## Mục tiêu
Tạo một chiếc vali đang mở ở giữa phòng, thể hiện nhân vật đang chuẩn bị về quê.

## Vị trí
Đặt vali ở vùng giữa phòng, nhưng tránh block đường đi gameplay:
- Không overlap với giường.
- Không overlap với ghế.
- Không overlap với bàn học.
- Không che item collect chính.

Đề xuất vị trí:
- Ở khoảng trống giữa giường và bàn, hơi lệch xuống dưới một chút theo camera.

## Cách dựng nếu không có asset vali mở
Tạo bằng primitive mesh:

### Phần thân vali dưới
- Cube scale khoảng: X = 0.9, Y = 0.18, Z = 0.55.
- Material: nâu da / cam đất / xanh rêu tối.
- Bo góc nhẹ nếu có thể.
- Đặt đáy vali chạm sàn.

### Phần nắp vali mở
- Cube mỏng scale khoảng: X = 0.9, Y = 0.08, Z = 0.55.
- Pivot hoặc rotation đặt như nắp mở 95-110 độ.
- Nắp phải gắn vào cạnh sau của thân vali, không được lơ lửng rời ra.
- Rotation quanh trục local X sao cho nhìn giống đang bật mở.

### Bản lề
- 2 cylinder nhỏ hoặc cube nhỏ ở cạnh sau.
- Màu kim loại tối.
- Bản lề phải chạm cả thân vali và nắp vali.

### Tay kéo / quai xách
- Dùng torus/cylinder/cube nhỏ.
- Đặt ở mặt trước vali.
- Không quá lớn.

### Đồ trong vali
Thêm vài vật đơn giản:
- Áo gấp: cube mỏng bo góc, màu trắng/cam/xanh.
- Quần gấp: cube mỏng màu xanh đậm.
- Khăn: plane/cube mỏng màu vàng nhạt.

Tất cả đồ phải nằm trong lòng vali, không xuyên qua thân vali.

## Collider
- Vali có collider đơn giản box collider.
- Nếu gây cản gameplay, set vali là decorative hoặc collider nhỏ vừa phải.

---

# 4. Cửa sổ mở toang + lớp kính

## Mục tiêu
Tạo cửa sổ lớn đang mở để ánh sáng chiếu vào phòng. Cần mô tả kỹ thuật rõ ràng, không chỉ “mở cửa sổ”.

## Cấu trúc cửa sổ
Cửa sổ gồm 4 phần:
1. Khung cửa gắn vào tường.
2. Hai cánh cửa mở sang hai bên.
3. Một tấm kính trong suốt nằm trong khung.
4. Vùng ánh sáng chiếu vào phòng.

## Khung cửa
- Tạo frame bằng 4 thanh cube mỏng.
- Material: gỗ nâu sáng hoặc gỗ cũ.
- Khung phải nằm sát tường, không bay ra khỏi tường.
- Kích thước đề xuất: rộng 1.6m - 2.0m, cao 1.2m - 1.5m.
- Đặt ở tường phía trên/phải căn phòng theo góc nhìn camera hiện tại.

## Hai cánh cửa mở toang
Tạo mỗi cánh bằng một plane/cube mỏng:
- Cánh trái xoay ra ngoài khoảng 100-120 độ.
- Cánh phải xoay ra ngoài khoảng 100-120 độ.
- Mỗi cánh phải có cạnh bản lề chạm vào khung cửa.
- Không đặt cánh cửa lơ lửng giữa không khí.
- Không để cánh cửa xuyên qua tường.

Gợi ý kỹ thuật:
- Tạo empty object làm pivot tại cạnh bản lề.
- Parent cánh cửa vào pivot.
- Rotate pivot theo trục Y để mở cánh.
- Cánh trái rotate khoảng -105 độ.
- Cánh phải rotate khoảng +105 độ.

## Lớp kính
Tạo một plane hoặc cube rất mỏng nằm trong khung cửa:
- Name: `Window_Glass_Pane`
- Material: transparent glass.
- Alpha: 0.18 - 0.28.
- Smoothness: 0.6 - 0.8.
- Màu: xanh nhạt / trắng xanh rất nhẹ.
- Tấm kính nằm trong khung, không chắn hoàn toàn ánh sáng.
- Không cần phản chiếu mạnh.

Nếu render pipeline không hiển thị transparency:
- Set material Surface Type = Transparent.
- Blend Mode = Alpha.
- Disable shadow casting cho glass nếu kính làm phòng bị tối.

## Bên ngoài cửa sổ
Nếu thấy khoảng trống đen ngoài cửa sổ, thêm backdrop đơn giản:
- Sky color plane xanh nhạt/vàng nhạt.
- Một vài silhouette cây chuối/cây xanh low-poly.
- Không cần quá chi tiết.

---

# 5. Ánh sáng từ cửa sổ

## Mục tiêu ánh sáng
Ánh sáng dịu, ấm, chiếu từ cửa sổ vào trong phòng, tạo cảm giác buổi sáng / chiều nhẹ ở Việt Nam.

## Hướng sáng
Theo góc nhìn người chơi trong nhà:
- Cửa sổ nằm phía trên/góc phải.
- Ánh sáng đi từ ngoài cửa sổ vào trong phòng.
- Hướng chiếu khoảng 90 độ so với mặt cửa sổ, tức là chiếu thẳng từ cửa vào trong phòng.
- Ánh sáng đổ về phía giữa phòng, chạm tới vali và một phần giường/sàn.

## Cách dựng kỹ thuật
Tạo hoặc chỉnh Directional Light:
- Name: `Sunlight_From_Window`
- Color: warm yellow / soft orange.
- Intensity: 0.7 - 1.2 tùy scene.
- Rotation: chỉnh để tia sáng đi từ cửa sổ phía trên/góc phải vào trung tâm phòng.
- Shadows: Soft Shadows.
- Shadow strength: 0.35 - 0.55.

Thêm Area Light hoặc Spot Light ở ngoài cửa sổ:
- Name: `Window_Soft_Area_Light`
- Type: Area Light nếu dùng URP/HDRP hỗ trợ, nếu không thì dùng Spot Light.
- Position: ngay ngoài cửa sổ, cao ngang tâm cửa.
- Direction: nhìn vào giữa phòng.
- Color: warm cream.
- Intensity: 1.0 - 2.5, chỉnh cho không cháy sáng.
- Range: đủ tới giữa phòng.
- Spot angle nếu dùng Spot: 50 - 70 độ.
- Shadows mềm hoặc tắt shadow nếu bị noise.

## Light beam / vệt nắng trên sàn
Tạo một plane trong suốt trên sàn để giả lập vệt nắng:
- Name: `Sunlight_Floor_Patch`
- Shape: hình thang hoặc rectangle dài.
- Material: transparent warm yellow.
- Alpha: 0.12 - 0.22.
- Đặt sát sàn, cao hơn sàn 0.005 - 0.01 unit.
- Hướng từ cửa sổ kéo vào giữa phòng.
- Không để z-fighting với sàn.

Có thể thêm vài vệt bụi sáng:
- Dùng particle nhẹ hoặc vài plane alpha rất mờ.
- Không làm rối gameplay.

## Ambient
- Ambient color: warm beige, không quá tối.
- Không để phòng bị cháy sáng trắng.
- Không để góc phòng đen hoàn toàn.

---

# 6. Bố cục phòng sau restyle

## Giường
- Giữ giường nhưng đảm bảo chạm sàn.
- Gối nằm trên giường, không lơ lửng.
- Có thể thêm chăn gấp nhẹ để giống phòng trọ.

## Bàn học
- Giữ bàn học bên phải.
- Các item trên bàn phải nằm đúng trên mặt bàn.
- Laptop/sách/cốc không được xuyên mặt bàn.

## Tủ/kệ
- Kê sát tường.
- Không che cửa sổ.
- Không overlap với portrait.

## Vật dụng Việt Nam có thể thêm
Thêm vừa phải, không clutter:
- Lịch bloc treo tường.
- Quạt đứng hoặc quạt bàn.
- Ổ điện kéo dài.
- Dép nhựa gần cửa.
- Thùng mì / hộp đồ sinh viên.
- Balo hoặc túi đồ gần vali.

---

# 7. Kiểm tra vị trí object - bắt buộc

Sau khi hoàn thành, chạy checklist:

## Không lơ lửng
Với mọi object chính:
- Raycast từ object xuống sàn/mặt bàn/mặt giường.
- Nếu khoảng cách > 0.02 unit thì hạ object xuống.

## Không overlap
Kiểm tra AABB/collider:
- Vali không overlap giường/bàn/ghế.
- Portrait không overlap cửa sổ/tủ.
- Cánh cửa sổ không xuyên tường.
- Item collect không bị che hoặc nằm trong vật khác.

## Gameplay readability
- Người chơi vẫn thấy đường đi rõ.
- Item collect vẫn dễ thấy.
- UI không bị ảnh hưởng.
- Camera Game view Full HD 1920x1080 vẫn đẹp.

---

# 8. Output yêu cầu sau khi làm xong
Sau khi restyle xong:
1. Chụp screenshot Game View 1920x1080.
2. Chụp screenshot Scene View từ góc cửa sổ.
3. Chụp screenshot Scene View từ trên xuống.
4. Báo lại các object đã thêm/chỉnh.
5. Báo nếu có asset nào không tìm thấy trong project.

---

# 9. Tiêu chí đạt
Scene cuối phải giống:
- Phòng trọ sinh viên Việt Nam.
- Có vibe chuẩn bị về quê ăn Tết.
- Có nền gạch hoa văn Việt Nam.
- Có ảnh meme/portrait trên tường.
- Có vali mở ở giữa phòng.
- Có cửa sổ mở toang với kính trong suốt.
- Có ánh sáng vàng dịu chiếu từ cửa sổ vào phòng.

Không được giống:
- Prototype tối màu.
- Phòng trống kỹ thuật.
- Asset đặt bừa.
- Vật thể bay/lơ lửng/chồng chéo.
