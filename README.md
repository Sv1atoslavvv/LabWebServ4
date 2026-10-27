# EmailSenderWeb — лабораторна робота № 4

ASP.NET Core MVC застосунок із окремою DLL-бібліотекою сервісів `EmailSenderWeb.Services`. Проєкт демонструє надсилання email через Gmail SMTP, DI lifetimes, логування HTTP-запитів і завантаження файлів.

## Склад solution

- `EmailSenderWeb` — вебзастосунок із MVC-сторінками та контролерами.
- `EmailSenderWeb.Services` — `IEmailSender`, конфігурація та MailKit реалізація.
- `EmailSenderLab4.sln` — solution для лабораторної роботи № 4.

## Запуск

Потрібен .NET 10 SDK. Із кореня репозиторію:

```powershell
dotnet restore .\EmailSenderLab4.sln -m:1
dotnet build .\EmailSenderLab4.sln --no-restore -m:1
dotnet run --project .\EmailSenderWeb\EmailSenderWeb.csproj --no-build
```

## Налаштування Gmail

1. Увімкни двоетапну перевірку Google та створи пароль застосунку для пошти, що надсилатиме листи.
2. У `EmailSenderWeb` задай секрет локально через .NET User Secrets:

```powershell
cd .\EmailSenderWeb
dotnet user-secrets set "EmailSender:UserName" "your-address@gmail.com"
dotnet user-secrets set "EmailSender:Password" "APP_PASSWORD"
dotnet user-secrets set "EmailSender:FromEmail" "your-address@gmail.com"
```

Секрет пароля зберігається локально поза Git. SMTP налаштований на `smtp.gmail.com`, порт `587`, TLS — STARTTLS.

## Лабораторні роботи № 2–3

Застосунок містить форму надсилання email і демонстрацію DI lifetimes на `/Lifetimes`. Файловий logger записує HTTP-запити у щоденні файли в `EmailSenderWeb/Logs`; ця папка ігнорується Git.

## Лабораторна робота № 4 — завантаження файлів

`FilesController` приймає файл із Razor-форми, перевіряє розмір до 20 МБ і блокує файли, що можуть виконувати активний код. Файли з унікальними іменами зберігаються у `EmailSenderWeb/wwwroot/Files`. Сторінка `/Files` показує список файлів, а сторінка деталей відкриває вибраний файл. Вміст папки `Files` ігнорується Git; `.gitkeep` зберігає саму директорію у проєкті.
