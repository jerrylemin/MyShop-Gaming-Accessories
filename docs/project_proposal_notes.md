# Project Proposal Notes

## Các nhóm file đã quét

- Tài liệu nội bộ:
  - `docs/architecture.md`
  - `docs/database_schema.md`
  - `docs/data_flow.md`
  - `docs/developer_notes.md`
  - `docs/features.md`
  - `docs/final_cleanup.md`
  - `docs/navigation.md`
  - `docs/project_context.md`
  - `docs/sample_products.md`
  - `docs/services.md`
  - `docs/viewmodels.md`
  - `.ai_memory/PROJECT_IDENTITY.md`
  - `.ai_memory/DATABASE_MAP.md`
- Cấu hình và metadata:
  - `ProjectTest.csproj`
  - `App.xaml`
  - `App.xaml.cs`
  - `Properties/launchSettings.json`
  - `dotnet-tools.json`
- Mô hình dữ liệu và dữ liệu seed:
  - `Models/Category.cs`
  - `Models/Product.cs`
  - `Models/Order.cs`
  - `Models/OrderItem.cs`
  - `DataAccess/MyShopDbContext.cs`
  - `DataAccess/Seeding/GamingAccessorySeedGenerator.cs`
  - `DataAccess/Seeding/gaming_accessories_seed_data.json`
- Tầng dịch vụ và truy cập dữ liệu:
  - `Services/AppBootstrapper.cs`
  - `Services/NavigationService.cs`
  - `Services/DashboardService.cs`
  - `Services/ReportingService.cs`
  - `Services/ExcelProductImportService.cs`
  - `Services/SettingsService.cs`
  - `Services/DatabaseInitializer.cs`
  - `Repositories/CategoryRepository.cs`
  - `Repositories/ProductRepository.cs`
  - `Repositories/OrderRepository.cs`
- Giao diện và luồng người dùng:
  - `Views/LoginWindow.xaml`
  - `Views/DatabaseSetupWindow.xaml`
  - `Views/MainWindow.xaml`
  - `Views/MainWindow.xaml.cs`
  - `Views/Pages/DashboardPage.xaml`
  - `Views/Pages/ProductsPage.xaml`
  - `Views/Pages/ProductEditPage.xaml`
  - `Views/Pages/OrdersPage.xaml`
  - `Views/Pages/ReportsPage.xaml`
  - `Views/Pages/SettingsPage.xaml`
  - Các ViewModel tương ứng trong thư mục `ViewModels`
- Script và công cụ hỗ trợ:
  - `download_gaming_accessory_images.ps1`
  - `scripts/rebuild-dev-db.ps1`
  - `tools/DatabaseRebuilder/*`
  - `tools/VerificationRunner/*`

## Các tính năng hiện có được dùng để viết proposal

- Đăng nhập và lưu thông tin truy cập
- Cấu hình kết nối cơ sở dữ liệu PostgreSQL
- Dashboard tổng quan gồm tổng sản phẩm, sắp hết hàng, đơn hôm nay, doanh thu hôm nay
- Quản lí sản phẩm với tìm kiếm, lọc giá, lọc danh mục, sắp xếp, phân trang
- Xem chi tiết sản phẩm và bộ ảnh 3 hình
- Thêm, sửa, xóa sản phẩm
- Import dữ liệu sản phẩm từ Excel
- Quản lí đơn hàng và tự động đồng bộ tồn kho
- Báo cáo doanh thu theo ngày, tuần, tháng, năm
- Báo cáo sản phẩm bán chạy và tỉ trọng bán hàng
- Lưu màn hình làm việc cuối và số lượng sản phẩm mỗi trang
- Seed dữ liệu 5 danh mục, 50 sản phẩm, 180 đơn hàng

## Domain được chọn và lý do

Domain được chọn là **POS bán phụ kiện gaming**.

Lý do:

- Tài liệu hiện tại trong `docs` và `.ai_memory` đều xác nhận domain đã được chuyển sang gaming accessories POS.
- Seed data hiện tại gồm bàn phím gaming, chuột gaming, tai nghe gaming, mousepad, webcam và microphone.
- Giao diện `Products`, `Orders`, `Dashboard`, `Reports`, `Settings` đều đang phục vụ đúng bài toán POS cho cửa hàng phụ kiện gaming.
- Domain này phù hợp với ràng buộc đề bài và không thuộc các chủ đề bị loại bỏ.

## Các giả định khi viết proposal

- Proposal được viết theo trạng thái source code hiện tại của dự án, không dựa vào các domain cũ đã bị loại bỏ.
- Các tính năng trong mục 5.1 được xem là nhóm chức năng kế thừa phổ biến từ các phần mềm POS đã khảo sát.
- Các tính năng trong mục 5.2 là phần cải tiến hoặc điểm nhấn của chính dự án hiện tại, không phải tính năng ngoài phạm vi codebase.
- Phần khảo sát phần mềm tương tự dùng ba sản phẩm POS thực tế phù hợp với cửa hàng bán lẻ tại Việt Nam: KiotViet, Sapo POS và POS365.
- Các file trong `obj` và `bin` có được quét ở bước thống kê file, nhưng nội dung proposal chỉ dựa trên source files và docs có giá trị nghiệp vụ thực tế.
