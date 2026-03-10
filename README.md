Hướng dẫn chạy project

1\. Chạy Backend



Mở thư mục backend:



UserManagement.API



Cập nhật chuỗi kết nối database trong file:



appsettings.json



Sau đó chạy lệnh:



dotnet ef database update



Tiếp theo chạy API:



dotnet run



API sẽ chạy tại:



https://localhost:7226/swagger

2\. Chạy Frontend



Di chuyển đến thư mục frontend:



cd user-management-ui



Cài đặt thư viện:



npm install



Chạy project:



npm run dev



Mở trình duyệt tại:



http://localhost:3000

API Endpoints

Method	Endpoint	Mô tả

GET	/api/users	Lấy danh sách người dùng

POST	/api/users	Thêm người dùng

PUT	/api/users/{id}	Cập nhật người dùng

DELETE	/api/users/{id}	Xóa người dùng

