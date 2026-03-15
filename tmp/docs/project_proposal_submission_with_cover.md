---
title: "ĐỀ XUẤT DỰ ÁN"
---

Tên đề tài: MyShop Gaming Accessories POS

Môn học: Lập trình Windows

Nhóm sinh viên

21127645 - Lê Minh

21127224 - Nguyễn Vũ Bách

Năm học: 2025 - 2026

```{=openxml}
<w:p><w:r><w:br w:type="page"/></w:r></w:p>
```
# ĐỀ XUẤT DỰ ÁN

## 1. Tên dự án

**MyShop Gaming Accessories POS**

- Mô tả dự án: MyShop Gaming Accessories POS là phần mềm bán hàng trên nền tảng WinUI 3 dành cho cửa hàng phụ kiện gaming tại thị trường Việt Nam. Hệ thống hỗ trợ đăng nhập, cấu hình kết nối cơ sở dữ liệu PostgreSQL, quản lí danh mục và sản phẩm, tạo đơn hàng, đồng bộ tồn kho, theo dõi dashboard tổng quan, thống kê doanh thu và lưu thiết lập làm việc của người dùng.
- Số giờ làm việc hiệu dụng: 20 giờ

## 2. Các thành viên

| STT | MSSV | Họ và tên |
| --- | --- | --- |
| 1 | 21127645 | Lê Minh |
| 2 | 21127224 | Nguyễn Vũ Bách |

## 3. Khảo sát các phần mềm tương tự

### 3.1. KiotViet

- Đặc điểm: KiotViet là phần mềm quản lí bán hàng phổ biến tại Việt Nam, hỗ trợ bán tại quầy, quản lí hàng hóa, theo dõi tồn kho, quản lí đơn hàng và báo cáo doanh thu theo nhiều tiêu chí.
- Ưu điểm: Giao diện quen thuộc với mô hình bán lẻ, thao tác nhanh, có nhiều báo cáo thực tế và phù hợp với cửa hàng quy mô nhỏ đến vừa.
- Nhược điểm: Nhiều chức năng mở rộng đi kèm chi phí sử dụng; mức tùy biến giao diện và quy trình xử lí cho một cửa hàng chuyên biệt chưa cao.

### 3.2. Sapo POS

- Đặc điểm: Sapo POS là giải pháp quản lí bán hàng tích hợp giữa bán tại cửa hàng và quản lí vận hành, chú trọng quản lí hàng hóa, đơn hàng, khách hàng và báo cáo.
- Ưu điểm: Hỗ trợ quản lí bán hàng khá toàn diện, có khả năng theo dõi tồn kho tốt, báo cáo rõ ràng và phù hợp với mô hình bán lẻ hiện đại.
- Nhược điểm: Hệ thống có nhiều chức năng phục vụ đa ngành nên có thể gây cảm giác nặng với cửa hàng chỉ cần nghiệp vụ POS cốt lõi; một số tính năng nâng cao đòi hỏi thời gian làm quen.

### 3.3. POS365

- Đặc điểm: POS365 là phần mềm bán hàng tập trung vào thao tác tại quầy, quản lí hàng hóa, đơn bán, doanh thu và kiểm soát tồn kho.
- Ưu điểm: Quy trình bán hàng đơn giản, dễ tiếp cận, phù hợp cho cửa hàng cần triển khai nhanh và tập trung vào nghiệp vụ POS cơ bản.
- Nhược điểm: Khả năng mở rộng quy trình và mức độ tinh chỉnh kiến trúc không cao bằng hướng tự xây dựng phần mềm; giao diện và báo cáo chuyên biệt theo từng ngành còn hạn chế.

### 3.4. Bảng so sánh

| Tiêu chí | KiotViet | Sapo POS | POS365 | Đề tài đề xuất |
| --- | --- | --- | --- | --- |
| Quản lí sản phẩm | Có | Có | Có | Có |
| Quản lí đơn hàng | Có | Có | Có | Có |
| Báo cáo doanh thu | Có | Có | Có | Có |
| Phân trang và tìm kiếm | Có | Có | Có | Có |
| Giao diện dễ sử dụng | Tốt | Tốt | Khá | Hướng đến đơn giản, tập trung tác vụ chính |
| Hỗ trợ cấu hình | Có | Có | Có | Có cấu hình kết nối và thiết lập làm việc |

Qua khảo sát có thể thấy các phần mềm POS phổ biến đều tập trung vào ba nhóm chức năng chính gồm quản lí sản phẩm, quản lí đơn hàng và báo cáo doanh thu. Đề tài được đề xuất kế thừa các nhóm chức năng cốt lõi này, đồng thời tinh gọn phạm vi cho đúng ngữ cảnh cửa hàng phụ kiện gaming.

## 4. Xác định các kiểu dữ liệu cơ bản

### 4.1. Kiểu dữ liệu cho quản lí danh mục và sản phẩm

| Kiểu dữ liệu | Thực thể hiện tại | Diễn giải |
| --- | --- | --- |
| Category | Category | Lưu thông tin nhóm sản phẩm như Gaming Keyboard, Gaming Mouse, Gaming Headset, Mousepad, Streaming Gear. |
| Product | Product | Lưu mã SKU, tên sản phẩm, hãng, giá nhập, giá bán, số lượng tồn, mô tả, hình ảnh và các thông tin mô tả kỹ thuật của phụ kiện gaming. |

Trong hệ thống hiện tại, `Category` được dùng để nhóm sản phẩm theo loại hàng hóa. `Product` là đối tượng trung tâm của nghiệp vụ bán hàng vì vừa phục vụ tra cứu tồn kho vừa tham gia trực tiếp vào quá trình tạo đơn hàng.

### 4.2. Kiểu dữ liệu cho quản lí giao dịch bán hàng

| Kiểu dữ liệu | Thực thể hiện tại | Diễn giải |
| --- | --- | --- |
| Order | Order | Lưu thời gian tạo đơn, trạng thái đơn và tổng giá trị thanh toán. |
| Detail | OrderItem | Lưu từng dòng hàng trong đơn, bao gồm sản phẩm, số lượng, đơn giá bán và thành tiền. |
| Product | Product | Là dữ liệu tham chiếu để xác định mặt hàng được bán và cập nhật tồn kho tương ứng. |

Mối quan hệ dữ liệu của hệ thống là một đơn hàng có nhiều dòng chi tiết, mỗi dòng chi tiết tham chiếu đến một sản phẩm. Cấu trúc này phù hợp với bài toán POS vì cho phép vừa theo dõi lịch sử bán hàng vừa thống kê sản phẩm bán chạy.

## 5. Đề xuất các tính năng chính

### 5.1. Các tính năng sao chép dự kiến từ các ứng dụng đã khảo sát

| STT | Tính năng | Mô tả ngắn | Số giờ |
| --- | --- | --- | ---: |
| 1 | Đăng nhập và lưu cấu hình kết nối cơ sở dữ liệu | Cho phép người dùng đăng nhập, lưu thông tin truy cập và cấu hình kết nối PostgreSQL khi khởi động hệ thống. | 2 |
| 2 | Dashboard tổng quan | Hiển thị tổng số sản phẩm, số mặt hàng sắp hết, số đơn trong ngày và doanh thu trong ngày. | 3 |
| 3 | Quản lí sản phẩm | Thêm, sửa, xóa và xem chi tiết sản phẩm phụ kiện gaming theo danh mục. | 3 |
| 4 | Phân trang, lọc và tìm kiếm sản phẩm | Hỗ trợ tìm theo tên, SKU, hãng, khoảng giá, danh mục và sắp xếp dữ liệu. | 2 |
| 5 | Import dữ liệu sản phẩm từ Excel | Hỗ trợ nạp nhanh dữ liệu hàng hóa từ tệp Excel để tiết kiệm thời gian khởi tạo danh mục. | 1 |
| 6 | Quản lí đơn hàng và đồng bộ tồn kho | Tạo đơn, cập nhật trạng thái, thêm dòng sản phẩm và tự động trừ tồn kho khi đơn được xử lí. | 4 |
| 7 | Báo cáo thống kê doanh thu và sản phẩm bán chạy | Tổng hợp doanh thu theo mốc thời gian và theo dõi nhóm sản phẩm bán tốt trong khoảng ngày chọn. | 3 |
|  | **Tổng cộng** |  | **18** |

### 5.2. Tính năng cải tiến hoặc mới dự kiến

| STT | Tính năng | Mô tả ngắn | Số giờ |
| --- | --- | --- | ---: |
| 1 | Áp dụng MVVM kết hợp service và repository | Tách riêng giao diện, xử lí nghiệp vụ và truy cập dữ liệu để mã nguồn dễ bảo trì và mở rộng. | 1 |
| 2 | Lưu màn hình làm việc cuối và số dòng mỗi trang | Giúp người dùng quay lại đúng ngữ cảnh thao tác và tối ưu trải nghiệm khi duyệt danh sách sản phẩm. | 1 |
|  | **Tổng cộng** |  | **2** |

## 6. Kế hoạch làm việc nhóm

### 6.1. Kênh trao đổi giữa các thành viên của nhóm

Nhóm sử dụng Zalo hoặc Messenger để trao đổi nhanh hằng ngày, dùng Google Meet khi cần họp chốt yêu cầu hoặc xử lí lỗi khó. Mã nguồn được quản lí trên GitHub để theo dõi lịch sử thay đổi, còn Trello được dùng để chia task theo tuần và theo tiến độ từng thành viên.

### 6.2. Qui trình tạo ra mã nguồn cho một tính năng

Quy trình thực hiện một tính năng được thống nhất như sau:

1. Cả nhóm trao đổi yêu cầu và chốt phạm vi chức năng cần làm.
2. Nhóm trưởng hoặc người phụ trách tạo task trên Trello và issue tương ứng trên GitHub.
3. Thành viên được giao việc tạo nhánh riêng để lập trình tính năng.
4. Sau khi hoàn thành, thành viên tự kiểm tra thủ công các luồng chính liên quan.
5. Mã nguồn được đẩy lên GitHub để thành viên khác đọc và góp ý.
6. Sau khi chỉnh sửa theo góp ý, tính năng mới được hợp nhất vào nhánh chung.
7. Cả nhóm kiểm tra lại hệ thống sau tích hợp để bảo đảm không ảnh hưởng đến các chức năng đã có.

### 6.3. Qui trình đảm bảo chất lượng phần mềm

Nhóm áp dụng quy trình đảm bảo chất lượng phù hợp với dự án WinUI 3 ở mức đồ án môn học. Trước hết, mỗi chức năng đều được kiểm tra thủ công theo đúng luồng sử dụng thực tế như đăng nhập, thêm sản phẩm, tạo đơn hàng, lọc dữ liệu và xem báo cáo. Khi phát hiện lỗi, thành viên phụ trách sửa lỗi phải kiểm tra lại đúng chức năng vừa sửa và các chức năng liên quan trực tiếp như tồn kho, tổng tiền đơn hàng hoặc dữ liệu báo cáo.

Ngoài ra, sau mỗi lần tích hợp mã nguồn, nhóm sẽ thực hiện kiểm tra hồi quy ở các màn hình chính gồm Dashboard, Products, Orders, Reports và Settings để bảo đảm chức năng cũ vẫn chạy đúng. Mọi thay đổi quan trọng đều được ghi nhận qua commit và task tương ứng nhằm giúp việc theo dõi, đối chiếu và khôi phục bối cảnh làm việc trở nên rõ ràng hơn.

