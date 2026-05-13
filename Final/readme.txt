THÔNG TIN ĐỒ ÁN
===============

Tên đồ án: MyShop Gaming Accessories POS
Môn: Lập trình Windows

Thành viên nhóm:
- Lê Minh - MSSV: 21127645
- Nguyễn Vũ Bách - MSSV: 21127224


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
- Database setup window khi app không kết nối được PostgreSQL.

12. Database
- Sử dụng PostgreSQL và Entity Framework Core migrations.
- Có seed demo 5 danh mục, 110 sản phẩm, nhiều đơn hàng, khách hàng, khuyến mãi, users và loyalty.
- Có dump PostgreSQL demo tại installer/database/myshop_demo.dump.

13. Installer
- Có file setup.exe một file duy nhất trong thư mục Release.
- Có thư mục Release\App chứa ProjectTest.exe và các file runtime publish trực tiếp từ mã nguồn.
- setup.exe cài app, .NET 8 Desktop Runtime, Windows App Runtime 1.8, PostgreSQL 18, database demo, shortcut Desktop/Start Menu.
- Installer ưu tiên restore database từ dump PostgreSQL, nếu lỗi thì fallback seed bằng code.
- Có script export/restore/test installer và log cài đặt.


CÁC CHỨC NĂNG CHƯA THỰC HIỆN
============================

- Chưa ký số code-signing cho setup.exe nên Windows SmartScreen có thể cảnh báo app không rõ nguồn gốc.
- Chưa có đồng bộ cloud/multi-branch real-time giữa nhiều máy.
- Chưa tích hợp thiết bị bán hàng thật như máy quét mã vạch, máy in hóa đơn nhiệt, ngăn kéo tiền.
- LLM assistant cần API key riêng của người dùng, không hardcode API key thật trong source code.
- Chưa có hệ thống phân quyền chi tiết đến từng nút/chức năng nhỏ, mới dùng theo vai trò chính.


CÁC CHỨC NĂNG ĐỀ NGHỊ GIẢNG VIÊN XEM XÉT CỘNG ĐIỂM
==================================================

- Installer setup.exe một file: tự cài runtime, PostgreSQL, restore database dump, fallback seed, tạo shortcut và ghi config kết nối.
- Database demo được export/restore bằng PostgreSQL custom-format dump thay vì chỉ seed lại bằng code.
- App có đầy đủ workflow POS: sản phẩm, danh mục, đơn hàng, khách hàng, loyalty, khuyến mãi, tồn kho, hóa đơn.
- Reports có doanh thu, lợi nhuận, top products, sales commission và ML.Net insight.
- Có GraphQL demo và plugin loading demo, vượt ngoài yêu cầu POS cơ bản.
- Có nhiều script tự động hóa build, export database, restore database, test installer và verification runner.
- Giao diện WinUI 3 có nhiều trang, MVVM, repository/service layer và data binding rõ ràng.


BẢNG PHÂN CÔNG CÔNG VIỆC VÀ ĐIỂM TỰ ĐÁNH GIÁ
============================================

+----------------+----------+--------------------------------------------------------------+-------------+
| Thành viên     | MSSV     | Công việc chính                                               | Tự đánh giá |
+----------------+----------+--------------------------------------------------------------+-------------+
| Lê Minh        | 21127645 | Database PostgreSQL/EF Core, migrations, seed data,           | 9.5/10      |
|                |          | installer setup.exe, dump/restore database, scripts build,    |             |
|                |          | reports, dashboard, validation và đóng gói nộp bài.           |             |
+----------------+----------+--------------------------------------------------------------+-------------+
| Nguyễn Vũ Bách | 21127224 | UI WinUI, Products, Orders, Customers, Login, Settings,       | 9.5/10      |
|                |          | navigation, assets, testing các luồng nghiệp vụ, README và    |             |
|                |          | hoàn thiện demo flows.                                       |             |
+----------------+----------+--------------------------------------------------------------+-------------+

Nhận xét phân công:
- Công việc được chia gần đều giữa backend/database/installer và UI/nghiệp vụ/testing.
- Cả hai thành viên đều tham gia hoàn thiện demo, sửa lỗi và kiểm tra ứng dụng.
- Điểm tự đánh giá bằng nhau vì khối lượng đóng góp được chia đều và đều ảnh hưởng trực tiếp đến bản nộp cuối.


HƯỚNG DẪN CHẠY BẢN RELEASE
==========================

1. Mở thư mục Release.
2. Nên cài bằng setup.exe: bấm chuột phải setup.exe, chọn Run as administrator.
3. Chờ installer cài runtime, PostgreSQL và database demo.
4. Mở shortcut MyShop Gaming Accessories POS trên Desktop hoặc Start Menu.
5. Thư mục Release\App có ProjectTest.exe được publish trực tiếp từ mã nguồn, dùng để đối chiếu file thực thi sau khi build. Nếu chạy trực tiếp file này thì máy vẫn cần có runtime và database đã cấu hình.
6. Đăng nhập bằng:
   - admin / MyShop123!
   - moderator / MyShop123!
   - sale / MyShop123!

Nếu cài đặt lỗi, xem log:
- C:\ProgramData\MyShop POS\Logs\setup-log.txt
- C:\ProgramData\MyShop POS\Logs\restore-demo-database.log
