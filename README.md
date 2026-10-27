# EmailSenderWeb — лабораторна робота № 2

ASP.NET Core MVC застосунок із окремою DLL-бібліотекою сервісів `EmailSenderWeb.Services`. Email надсилається через Gmail SMTP за допомогою MailKit.

## Склад solution

- `EmailSenderWeb` — вебзастосунок, MVC форма та action.
- `EmailSenderWeb.Services` — `IEmailSender`, конфігурація та MailKit реалізація.
- `Startup.ConfigureServices` — DI-реєстрація `IEmailSender` як Transient і демонстрація Transient/Scoped/Singleton.

## Запуск

Потрібен .NET 10 SDK. Із кореня solution:

```powershell
dotnet restore .\EmailSenderLab2.sln -m:1
dotnet build .\EmailSenderLab2.sln --no-restore -m:1
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

3. Запусти сайт, введи одержувача та текст у формі й натисни **Submit**.

`Host`, `Port`, `UseStartTls`, `FromName` і `Subject` задані в `EmailSenderWeb/appsettings.json`. Секрет пароля зберігається локально поза Git, хоча конфігурація застосунку читається через стандартний `IConfiguration`.

Для лабораторної адреса SMTP задана як `smtp.gmail.com`, порт `587`, TLS — STARTTLS. Без локально заданих облікових даних програма покаже помилку надсилання, але вебсайт і форма запускаються.

## Демонстрація DI lifetimes

Перейди на `/Lifetimes`: два ID для Transient відрізняються, а Scoped і Singleton у межах одного запиту збігаються. При повторному запиті сторінки Scoped ID змінюється, Singleton зберігається до перезапуску застосунку.

## Лабораторна робота № 3 — логування HTTP-запитів

`Startup.ConfigureServices` реєструє файловий `ILoggerProvider`, який створює каталог `EmailSenderWeb/Logs` і щоденні файли `requests-yyyy-MM-dd.log`. `RequestLoggingMiddleware` стоїть на початку конвеєра та записує повний URL, час отримання запиту й IP клієнта. Каталог `Logs` ігнорується Git через `.gitignore`.

Приклад адреси з query string: `http://localhost:6100/?lab=3&message=hello%20world`. Компонент query починається після `?`; параметри розділяються `&`, а значення — через `=`. Символи, що є даними та можуть бути сприйняті як роздільники, пробіли й інші символи поза допустимим набором потрібно percent-encode, наприклад пробіл як `%20`.
