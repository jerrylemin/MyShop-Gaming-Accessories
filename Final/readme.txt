THONG TIN DO AN
===============

Ten do an: MyShop Gaming Accessories POS
Mon: Lap trinh Windows

Thanh vien nhom:
- Le Minh - MSSV: 21127645
- Nguyen Vu Bach - MSSV: 21127224


CAC CHUC NANG DA THUC HIEN
==========================

1. Dang nhap va phan quyen
- Dang nhap bang tai khoan demo admin / MyShop123!, moderator / MyShop123!, sale / MyShop123!.
- Phan quyen theo vai tro Admin, Moderator, Sale.
- Luu thong tin dang nhap da ma hoa o local storage.

2. Quan ly san pham gaming accessories
- Danh sach san pham co hinh anh, SKU, hang san xuat, gia nhap, gia ban, ton kho, danh muc.
- Them, sua, xoa, xem chi tiet san pham.
- Tim kiem, loc theo danh muc/gia, sap xep va phan trang.
- Import san pham tu Excel.
- Dong goi san hinh anh gaming products trong Assets/GamingProducts.

3. Quan ly danh muc
- Xem danh sach danh muc.
- Them, sua, xoa danh muc.
- Lien ket danh muc voi san pham.

4. Quan ly don hang POS
- Tao don hang tu nhieu san pham.
- Cap nhat so luong, gia, trang thai don hang.
- Tu dong tru/hoan ton kho khi tao, sua, huy, xoa don.
- Tim kiem, loc ngay, sap xep, phan trang don hang.
- Xuat hoa don PDF.

5. Quan ly khach hang va loyalty
- Them, sua, xoa khach hang.
- Tim kiem theo ten, so dien thoai, email.
- Luu diem tich luy, tong chi tieu, lich su mua hang.
- Lien ket khach hang voi don hang.
- Ghi nhan giao dich diem loyalty.

6. Khuyen mai
- Co bang khuyen mai va ma khuyen mai demo.
- Ho tro tinh giam gia theo luong logic trong don hang.

7. Dashboard
- Tong san pham, san pham sap het hang.
- Don hang hom nay, doanh thu hom nay.
- Don hang gan day, top san pham ban chay.
- Bieu do thong ke doanh thu.

8. Reports
- Bao cao doanh thu va loi nhuan theo ngay, tuan, thang, nam.
- Top san pham ban chay.
- Ty trong doanh so san pham.
- Hoa hong nhan vien ban hang.
- ML.Net insight/forecast doanh thu va goi y restock.

9. GraphQL
- Trang GraphQL demo query san pham, don hang, bao cao.
- Co mutation demo luu san pham va don hang.

10. Plugins
- Trang Plugins de doc plugin local.
- Co sample plugin project.

11. Cai dat va cau hinh
- Settings cho so dong moi trang, login saved credentials, LLM config, backup/restore, license activation.
- Database setup window khi app khong ket noi duoc PostgreSQL.

12. Database
- Su dung PostgreSQL va Entity Framework Core migrations.
- Co seed demo 5 danh muc, 110 san pham, nhieu don hang, khach hang, khuyen mai, users va loyalty.
- Co dump PostgreSQL demo tai installer/database/myshop_demo.dump.

13. Installer
- Co file setup.exe mot file duy nhat trong thu muc Release.
- setup.exe cai app, .NET 8 Desktop Runtime, Windows App Runtime 1.8, PostgreSQL 18, database demo, shortcut Desktop/Start Menu.
- Installer uu tien restore database tu dump PostgreSQL, neu loi thi fallback seed bang code.
- Co script export/restore/test installer va log cai dat.


CAC CHUC NANG CHUA THUC HIEN
============================

- Chua ky so code-signing cho setup.exe nen Windows SmartScreen co the canh bao app khong ro nguon goc.
- Chua co dong bo cloud/multi-branch real-time giua nhieu may.
- Chua tich hop thiet bi ban hang that nhu may quet ma vach, may in hoa don nhiet, ngan keo tien.
- LLM assistant can API key rieng cua nguoi dung, khong hardcode API key that trong source code.
- Chua co he thong phan quyen chi tiet den tung nut/chuc nang nho, moi dung theo vai tro chinh.


CAC CHUC NANG DE NGHI GIANG VIEN XEM XET CONG DIEM
==================================================

- Installer setup.exe mot file: tu cai runtime, PostgreSQL, restore database dump, fallback seed, tao shortcut va ghi config ket noi.
- Database demo duoc export/restore bang PostgreSQL custom-format dump thay vi chi seed lai bang code.
- App co day du workflow POS: san pham, danh muc, don hang, khach hang, loyalty, khuyen mai, ton kho, hoa don.
- Reports co doanh thu, loi nhuan, top products, sales commission va ML.Net insight.
- Co GraphQL demo va plugin loading demo, vuot ngoai yeu cau POS co ban.
- Co nhieu script tu dong hoa build, export database, restore database, test installer va verification runner.
- Giao dien WinUI 3 co nhieu trang, MVVM, repository/service layer va data binding ro rang.


BANG PHAN CONG CONG VIEC VA DIEM TU DANH GIA
============================================

+----------------+----------+--------------------------------------------------------------+-------------+
| Thanh vien     | MSSV     | Cong viec chinh                                               | Tu danh gia |
+----------------+----------+--------------------------------------------------------------+-------------+
| Le Minh        | 21127645 | Database PostgreSQL/EF Core, migrations, seed data,           | 9.5/10      |
|                |          | installer setup.exe, dump/restore database, scripts build,    |             |
|                |          | reports, dashboard, validation va dong goi nop bai.           |             |
+----------------+----------+--------------------------------------------------------------+-------------+
| Nguyen Vu Bach | 21127224 | UI WinUI, Products, Orders, Customers, Login, Settings,       | 9.5/10      |
|                |          | navigation, assets, testing cac luong nghiep vu, README va    |             |
|                |          | hoan thien demo flows.                                       |             |
+----------------+----------+--------------------------------------------------------------+-------------+

Nhan xet phan cong:
- Cong viec duoc chia gan deu giua backend/database/installer va UI/nghiep vu/testing.
- Ca hai thanh vien deu tham gia hoan thien demo, sua loi va kiem tra ung dung.
- Diem tu danh gia bang nhau vi khoi luong dong gop duoc chia deu va deu anh huong truc tiep den ban nop cuoi.


HUONG DAN CHAY BAN RELEASE
==========================

1. Mo thu muc Release.
2. Bam chuot phai setup.exe, chon Run as administrator.
3. Cho installer cai runtime, PostgreSQL va database demo.
4. Mo shortcut MyShop Gaming Accessories POS tren Desktop hoac Start Menu.
5. Dang nhap bang:
   - admin / MyShop123!
   - moderator / MyShop123!
   - sale / MyShop123!

Neu cai dat loi, xem log:
- C:\ProgramData\MyShop POS\Logs\setup-log.txt
- C:\ProgramData\MyShop POS\Logs\restore-demo-database.log
