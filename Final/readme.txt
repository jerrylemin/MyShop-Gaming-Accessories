THÔNG TIN ĐỒ ÁN
===============

Tên đồ án: MyShop Gaming Accessories POS
Môn: Lập trình Windows

Thành viên nhóm:
- Lê Minh - MSSV: 21127645
- Nguyễn Vũ Bách - MSSV: 21127224


CẤU TRÚC THƯ MỤC FINAL
======================

1. Source code
- Chứa mã nguồn chính của ứng dụng được copy từ thư mục gốc.
- Đã loại bỏ các thư mục/tập tin trung gian nặng hoặc không cần nộp như .git, .vs, bin, obj, docs AI và các file markdown audit/validation do AI tạo.
- Không chứa setup.exe vì nhóm sẽ tự tạo lại installer khi cần.

2. Release
- Chứa thư mục App là bản thực thi đã biên dịch ra từ mã nguồn.
- File chạy chính: Release\App\ProjectTest.exe.
- Không kèm setup.exe trong lần nộp này theo yêu cầu tách riêng việc tạo installer.

3. readme.txt
- File mô tả thông tin nhóm, chức năng, hướng dẫn chạy và phân công công việc.

4. script_quay_video_demo_5_phut.txt
- Kịch bản quay video test toàn bộ chức năng app theo từng phân quyền.


CÁC CHỨC NĂNG ĐÃ THỰC HIỆN
==========================

1. Đăng nhập và phân quyền
- Đăng nhập bằng tài khoản demo admin / MyShop123!, moderator / MyShop123!, sale / MyShop123!.
- Phân quyền theo vai trò Admin, Moderator, Sale.
- Lưu thông tin đăng nhập đã mã hóa ở local storage.

2. Quản lý sản phẩm gaming accessories
- Danh sách sản phẩm có hình ảnh, SKU, hãng sản xuất, giá nhập, giá bán, tồn kho, danh mục.
- Thêm, sửa, xóa, xem chi tiết sản phẩm.
- Tìm kiếm, lọc theo danh mục/giá, sắp xếp và phân trang.
- Import sản phẩm từ Excel.
- Đóng gói sẵn hình ảnh gaming products trong Assets/GamingProducts.

3. Quản lý danh mục
- Xem danh sách danh mục.
- Thêm, sửa, xóa danh mục.
- Liên kết danh mục với sản phẩm.

4. Quản lý đơn hàng POS
- Tạo đơn hàng từ nhiều sản phẩm.
- Cập nhật số lượng, giá, trạng thái đơn hàng.
- Tự động trừ/hoàn tồn kho khi tạo, sửa, hủy, xóa đơn.
- Tìm kiếm, lọc ngày, sắp xếp, phân trang đơn hàng.
- Xuất hóa đơn PDF.
- Khi nhập số điện thoại khách hàng chưa tồn tại, app hiển thị hộp thoại nhỏ để nhập tên khách hàng mới, tự lưu khách hàng vào database rồi tiếp tục tạo đơn.
- Sau khi tạo đơn có thể dùng nút Refresh để cập nhật danh sách nếu cần.

5. Quản lý khách hàng và loyalty
- Thêm, sửa, xóa khách hàng.
- Tìm kiếm theo tên, số điện thoại, email.
- Lưu điểm tích lũy, tổng chi tiêu, lịch sử mua hàng.
- Liên kết khách hàng với đơn hàng.
- Ghi nhận giao dịch điểm loyalty.

6. Khuyến mãi
- Có bảng khuyến mãi và mã khuyến mãi demo.
- Hỗ trợ tính giảm giá theo luồng logic trong đơn hàng.

7. Dashboard
- Tổng sản phẩm, sản phẩm sắp hết hàng.
- Đơn hàng hôm nay, doanh thu hôm nay.
- Đơn hàng gần đây, top sản phẩm bán chạy.
- Biểu đồ thống kê doanh thu.
- Có nút Refresh để cập nhật dữ liệu từ database.

8. Reports
- Báo cáo doanh thu và lợi nhuận theo ngày, tuần, tháng, năm.
- Top sản phẩm bán chạy.
- Tỷ trọng doanh số sản phẩm.
- Hoa hồng nhân viên bán hàng.
- ML.Net insight/forecast doanh thu và gợi ý restock.

9. GraphQL
- Trang GraphQL demo query sản phẩm, đơn hàng, báo cáo.
- Có mutation demo lưu sản phẩm và đơn hàng.

10. Plugins
- Trang Plugins để đọc plugin local.
- Có sample plugin project.

11. Cài đặt và cấu hình
- Settings cho số dòng mỗi trang, login saved credentials, LLM config, backup/restore, license activation.
- Backup database tự dò pg_dump/pg_restore trong PATH hoặc thư mục PostgreSQL phổ biến, đồng thời cho phép browse chọn thư mục bin PostgreSQL.
- Backup database cho phép browse vị trí lưu file backup.
- Restore database cho phép browse chọn file backup/dump để phục hồi.
- Database setup window hiển thị khi app không kết nối được PostgreSQL.

12. License activation
- Có trạng thái license đang kích hoạt và thông tin plan.
- Có các mã demo:
  + MYSHOP-1MONTH-2026: kích hoạt 1 tháng.
  + MYSHOP-1YEAR-2026: kích hoạt 1 năm.
  + MYSHOP-LIFETIME-2026: kích hoạt lifetime.
  + MYSHOP-DEMO-2026: mã demo tương thích cũ.

13. Database
- Sử dụng PostgreSQL và Entity Framework Core migrations.
- Có seed demo 5 danh mục, 110 sản phẩm, nhiều đơn hàng, khách hàng, khuyến mãi, users và loyalty.
- Có dump PostgreSQL demo tại installer/database/myshop_demo.dump trong source chính nếu cần dùng để tạo installer.


CÁC CHỨC NĂNG CHƯA THỰC HIỆN
============================

- Chưa ký số code-signing nên Windows có thể cảnh báo app không rõ nguồn gốc nếu đóng gói installer.
- Không kèm setup.exe trong thư mục Final\Release lần này; nhóm sẽ tự tạo setup.exe riêng khi cần.
- Chưa có đồng bộ cloud/multi-branch real-time giữa nhiều máy.
- Chưa tích hợp thiết bị bán hàng thật như máy quét mã vạch, máy in hóa đơn nhiệt, ngăn kéo tiền.
- LLM assistant cần API key riêng của người dùng, không hardcode API key thật trong source code.
- Chưa có hệ thống phân quyền chi tiết đến từng nút/chức năng nhỏ, mới dùng theo vai trò chính.


CÁC CHỨC NĂNG ĐỀ NGHỊ GIẢNG VIÊN XEM XÉT CỘNG ĐIỂM
==================================================

- App có đầy đủ workflow POS: sản phẩm, danh mục, đơn hàng, khách hàng, loyalty, khuyến mãi, tồn kho, hóa đơn.
- Luồng tạo đơn hỗ trợ tự tạo khách hàng mới ngay trong màn hình order khi số điện thoại chưa tồn tại.
- Backup/restore database thân thiện hơn: tự dò công cụ PostgreSQL, cho phép browse thư mục PostgreSQL và file backup/restore.
- Database demo có thể restore từ PostgreSQL custom-format dump hoặc fallback seed bằng code.
- Reports có doanh thu, lợi nhuận, top products, sales commission và ML.Net insight.
- Có GraphQL demo và plugin loading demo, vượt ngoài yêu cầu POS cơ bản.
- Giao diện WinUI 3 có nhiều trang, MVVM, repository/service layer và data binding rõ ràng.
- License activation có nhiều plan demo: 1 tháng, 1 năm và lifetime.


BẢNG PHÂN CÔNG CÔNG VIỆC VÀ ĐIỂM TỰ ĐÁNH GIÁ
============================================

+----------------+----------+--------------------------------------------------------------+-------------+
| Thành viên     | MSSV     | Công việc chính                                               | Tự đánh giá |
+----------------+----------+--------------------------------------------------------------+-------------+
| Lê Minh        | 21127645 | Database PostgreSQL/EF Core, migrations, seed data,           | 9.5/10      |
|                |          | dump/restore database, scripts build, reports, dashboard,     |             |
|                |          | license activation, validation và đóng gói nộp bài.           |             |
+----------------+----------+--------------------------------------------------------------+-------------+
| Nguyễn Vũ Bách | 21127224 | UI WinUI, Products, Orders, Customers, Login, Settings,       | 9.5/10      |
|                |          | navigation, assets, testing các luồng nghiệp vụ, README và    |             |
|                |          | hoàn thiện demo flows theo từng phân quyền.                   |             |
+----------------+----------+--------------------------------------------------------------+-------------+

Nhận xét phân công:
- Công việc được chia gần đều giữa backend/database/đóng gói và UI/nghiệp vụ/testing.
- Cả hai thành viên đều tham gia hoàn thiện demo, sửa lỗi và kiểm tra ứng dụng.
- Điểm tự đánh giá bằng nhau vì khối lượng đóng góp được chia đều và đều ảnh hưởng trực tiếp đến bản nộp cuối.


HƯỚNG DẪN CHẠY BẢN RELEASE
==========================

1. Mở thư mục Final\Release\App.
2. Chạy ProjectTest.exe.
3. Máy chạy app cần có:
   - .NET 8 Desktop Runtime.
   - Windows App Runtime phù hợp với WinUI 3.
   - PostgreSQL đang chạy và connection string đã cấu hình hoặc database demo đã được seed/restore.
4. Đăng nhập bằng:
   - admin / MyShop123!
   - moderator / MyShop123!
   - sale / MyShop123!
5. Nếu app hỏi cấu hình database, nhập thông tin PostgreSQL local đang dùng.
6. Nếu cần tạo setup.exe, chạy lại quy trình build installer từ source chính bên ngoài thư mục Final.

Lưu ý:
- Final\Release hiện không chứa setup.exe.
- Nếu port PostgreSQL 5432 bị chiếm, kiểm tra service PostgreSQL đang chạy hoặc đổi port trong connection string.
- Nếu Windows báo thiếu Windows App Runtime, cài Windows App Runtime rồi mở lại ProjectTest.exe.
