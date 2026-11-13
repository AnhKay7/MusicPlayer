# 🎵 MusicPlayer

> 🚧 **Đang phát triển.** App chưa hoàn thiện, tính năng và giao diện còn thay đổi.

Ứng dụng nghe nhạc offline trên Windows kiểu Spotify, viết bằng C# / WPF (.NET 10). Đồ án môn IT008.

## Chạy thử

Yêu cầu: Windows 10/11, Visual Studio 2026 (workload *.NET desktop development*) hoặc .NET 10 SDK.

```bash
git clone https://github.com/AnhKay7/MusicPlayer.git
cd MusicPlayer
dotnet run
```

Dữ liệu thư viện lưu ở `%LocalAppData%\MusicPlayer\library.db` (xoá file này để app quét lại từ đầu).

## Tiến độ

- [x] Phát nhạc, tua, âm lượng, chuyển bài, shuffle / repeat
- [x] Thư viện nhạc lưu bằng SQLite, quản lý thư mục quét, refresh
- [ ] Thêm bài lẻ, tìm kiếm, yêu thích, playlist

Chi tiết xem tab [Issues](https://github.com/AnhKay7/MusicPlayer/issues).
