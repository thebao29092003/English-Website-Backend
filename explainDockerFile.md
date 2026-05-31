# Đây là một Dockerfile được viết theo mô hình Multi-stage build (xây dựng đa giai đoạn) 
# tiêu chuẩn dành cho ứng dụng .NET 8.0 [1].
# Mục đích chính của Multi-stage build là tách biệt môi trường build 
# (cần SDK nặng) và môi trường chạy (chỉ cần Runtime nhẹ). 
# Điều này giúp file image thành phẩm cuối cùng của bạn cực kỳ gọn nhẹ 
# (chỉ khoảng hơn 200MB thay vì hơn 800MB nếu gộp cả SDK).


# GIAI ĐOẠN 1: Thiết lập môi trường chạy (Runtime)

# Tải về image Runtime của ASP.NET Core 8.0. Image này chỉ chứa những thư viện tối thiểu cần thiết để chạy ứng dụng, 
# không có trình biên dịch
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

#Chạy container dưới quyền một user không có quyền quản trị (non-root) để bảo mật. $APP_UID là một biến môi trường có sẵn trong .NET 8 (thường có ID là 1654, tên là app). 
#Đây là một cải tiến bảo mật tiêu chuẩn của .NET 8.
USER $APP_UID

# Tạo và di chuyển vào thư mục /app bên trong container.
WORKDIR /app

# Khai báo cổng mà container sẽ lắng nghe (8080 cho HTTP và 8081 cho HTTPS). Lưu ý: Trong .NET 8, khi chạy dưới quyền non-root, 
# cổng mặc định đã được đổi từ 80 thành 8080.
EXPOSE 8080
EXPOSE 8081


# GIAI ĐOẠN 2: Biên dịch ứng dụng (Build)

# Khởi tạo một giai đoạn mới, sử dụng image SDK 8.0 (chứa đầy đủ công cụ biên dịch, CLI, compiler...). 
# Giai đoạn này được đặt tên là build.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Khai báo tham số cấu hình build, mặc định là bản Release (tối ưu hóa hiệu năng).
ARG BUILD_CONFIGURATION=Release

# Đổi thư mục làm việc thành /src.
WORKDIR /src

# Sao chép chỉ riêng file dự án .csproj từ máy của bạn vào container.
COPY ["English.Website.csproj","."]

# Tại sao chỉ copy file .csproj trước rồi mới restore? Đây là kỹ thuật tận dụng cơ chế Cache của Docker. 
# Nếu bạn không thay đổi file .csproj (không thêm thư viện mới), 
# Docker sẽ bỏ qua bước tải này ở các lần build sau, giúp tăng tốc độ build lên rất nhiều.
# Cái lệnh này giống npm install trong Node.js, nó sẽ đọc file .csproj để biết cần tải về những thư viện nào và sau đó tải chúng về từ NuGet.
RUN dotnet restore "./English.Website.csproj"

# Sao chép toàn bộ mã nguồn còn lại từ máy của bạn vào container.
COPY . .
WORKDIR "/src/."

# Biên dịch dự án ra thư mục /app/build.
RUN dotnet build "./English.Website.csproj" -c $BUILD_CONFIGURATION -o /app/build


# GIAI ĐOẠN 3: Xuất bản ứng dụng (Publish)

# Kế thừa trực tiếp từ giai đoạn build ở trên.
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

# Xuất bản ứng dụng ra thư mục /app/publish. Lệnh này sẽ dọn dẹp, tối ưu hóa các file code và gom tất cả file .dll cần thiết để chạy ứng dụng vào một chỗ
# /p:UseAppHost=false: Chỉ định không tạo ra file thực thi (.exe) riêng cho hệ điều hành cụ thể, vì chúng ta sẽ chạy ứng dụng thông qua lệnh dotnet English.Website.dll.
RUN dotnet publish "./English.Website.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


GIAI ĐOẠN 4: Tạo Image thành phẩm cuối cùng (Final)

# Quay trở lại sử dụng image base siêu nhẹ (chỉ chứa Runtime) đã thiết lập ở Giai đoạn 1.
FROM base AS final

WORKDIR /app

# Sao chép toàn bộ các file đã được xuất bản từ thư mục /app/publish của giai đoạn publish 
# sang thư mục /app của giai đoạn hiện tại.
# Tại sao làm vậy? Toàn bộ mã nguồn gốc, các file rác sinh ra trong quá trình compile và cả bộ SDK nặng nề ở các giai đoạn trước sẽ bị bỏ lại hoàn toàn. 
# Image cuối cùng cực kỳ sạch sẽ, chỉ chứa file chạy và môi trường chạy.
COPY --from=publish /app/publish .

# Định nghĩa lệnh sẽ chạy khi container khởi động: Chạy ứng dụng web của bạn bằng cách thực thi file English.Website.dll.
ENTRYPOINT [ "dotnet", "English.Website.dll" ]