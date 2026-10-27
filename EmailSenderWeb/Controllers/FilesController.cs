using EmailSenderWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderWeb.Controllers;

public sealed class FilesController : Controller
{
    private const long MaxFileSize = 20 * 1024 * 1024;
    private static readonly HashSet<string> PreviewExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp", ".pdf", ".txt", ".csv"
    };
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".htm", ".xhtml", ".svg", ".js", ".mjs", ".css", ".php", ".aspx",
        ".cshtml", ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh", ".com", ".scr"
    };
    private readonly string _filesDirectory;

    public FilesController(IWebHostEnvironment environment)
    {
        _filesDirectory = Path.Combine(environment.WebRootPath, "Files");
        Directory.CreateDirectory(_filesDirectory);
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Notice = TempData["Notice"];
        ViewBag.Error = TempData["Error"];
        return View(ReadFiles());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxFileSize + 1024 * 1024)]
    public async Task<IActionResult> Upload(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            TempData["Error"] = "Оберіть файл для завантаження.";
            return RedirectToAction(nameof(Index));
        }

        if (file.Length > MaxFileSize)
        {
            TempData["Error"] = "Максимальний розмір файлу — 20 МБ.";
            return RedirectToAction(nameof(Index));
        }

        var originalName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalName);
        if (string.IsNullOrWhiteSpace(originalName) || BlockedExtensions.Contains(extension))
        {
            TempData["Error"] = "Цей тип файлу не дозволено завантажувати.";
            return RedirectToAction(nameof(Index));
        }

        var storedName = $"{Guid.NewGuid():N}_{originalName}";
        var destination = Path.Combine(_filesDirectory, storedName);
        await using (var stream = System.IO.File.Create(destination))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        TempData["Notice"] = $"Файл «{originalName}» завантажено.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(string name)
    {
        var safeName = Path.GetFileName(name);
        var file = ReadFiles().FirstOrDefault(item =>
            string.Equals(item.StoredName, safeName, StringComparison.Ordinal));
        return file is null ? NotFound() : View(file);
    }

    [HttpGet]
    public IActionResult Preview(string name)
    {
        var file = FindFile(name);
        if (file is null)
        {
            return NotFound();
        }

        var extension = Path.GetExtension(file);
        if (!PreviewExtensions.Contains(extension))
        {
            return PhysicalFile(file, "application/octet-stream", Path.GetFileName(file));
        }

        Response.Headers.ContentDisposition = "inline";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(file, GetContentType(extension), enableRangeProcessing: true);
    }

    private string? FindFile(string name)
    {
        var safeName = Path.GetFileName(name);
        if (!string.Equals(name, safeName, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(safeName))
        {
            return null;
        }

        var path = Path.Combine(_filesDirectory, safeName);
        return System.IO.File.Exists(path) ? path : null;
    }

    private IReadOnlyList<StoredFileItem> ReadFiles()
    {
        return Directory.EnumerateFiles(_filesDirectory)
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.CreationTimeUtc)
            .Select(info =>
            {
                var separator = info.Name.IndexOf('_');
                var displayName = separator >= 0 ? info.Name[(separator + 1)..] : info.Name;
                return new StoredFileItem
                {
                    StoredName = info.Name,
                    DisplayName = displayName,
                    Length = info.Length,
                    UploadedAt = info.CreationTime,
                    CanPreview = PreviewExtensions.Contains(info.Extension)
                };
            })
            .ToList();
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".pdf" => "application/pdf",
            ".csv" => "text/csv; charset=utf-8",
            ".txt" => "text/plain; charset=utf-8",
            _ => "application/octet-stream"
        };
    }
}
