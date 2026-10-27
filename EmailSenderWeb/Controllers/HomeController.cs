using EmailSenderWeb.Models;
using EmailSenderWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailSenderWeb.Controllers;

public sealed class HomeController : Controller
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IEmailSender emailSender, ILogger<HomeController> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewBag.Notice = TempData["Notice"];
        return View(new EmailFormModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EmailFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _emailSender.SendEmailAsync(model.Recipient, model.Message, cancellationToken);
            TempData["Notice"] = "Повідомлення успішно надіслано.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Не вдалося надіслати лист одержувачу {Recipient}.", model.Recipient);
            ModelState.AddModelError(string.Empty,
                "Не вдалося надіслати лист. Перевірте налаштування EmailSender та доступ до SMTP-сервера.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}
