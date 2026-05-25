using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public PaymentController(AppDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpPost("openTripPayment")]
    public ActionResult<object> openTripPayment()
    {
        return new { message = "TripPayments opened" };
    }

    [HttpPost("openTripPayments")]
    public ActionResult<object> openTripPayments()
    {
        return new { message = "TripPayments opened" };
    }

    [HttpGet("trip/{tripId}/reservations")]
    public async Task<ActionResult<IEnumerable<Reservation>>> getTripReservations(int tripId)
    {
        return await _context.Reservations.Where(item => item.TripId == tripId).ToListAsync();
    }

    [HttpPost("reservation")]
    public async Task<ActionResult<Reservation>> saveReservationData(Reservation reservation)
    {
        reservation.ReservationStatus = ReservationStatus.WaitingForPayment;
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(getSpecificReservations), new { id = reservation.Id }, reservation);
    }

    [HttpGet("reservation/{id}")]
    public async Task<ActionResult<Reservation>> getSpecificReservations(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        return reservation;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Payment>>> getPaymentData()
    {
        return await getPayments();
    }

    [HttpGet("getPayments")]
    public async Task<ActionResult<IEnumerable<Payment>>> getPayments()
    {
        return await _context.Payments.Include(item => item.Reservation).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> savePayment(Payment payment)
    {
        savePaymentData(payment);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(getPayment), new { id = payment.Id }, payment);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Payment>> getPayment(int id)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        return payment;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> updatePayment(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }

        _context.Entry(payment).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return Ok(new { message = "payment updated" });
    }

    [HttpPost("{id}/requestPayment")]
    public async Task<IActionResult> requestPayment(int id, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.FindAsync(new object?[] { id }, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        var result = await requestPayment(payment, cancellationToken);
        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status502BadGateway, result.Message);
        }

        return await processPayment(id, cancellationToken);
    }

    [HttpPost("{id}/requestPaymentPage")]
    public async Task<ActionResult<PaymentPageResponse>> requestPaymentPage(int id, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.Include(item => item.Reservation).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        return await requestPaymentPage(payment, cancellationToken);
    }

    [HttpPost("{id}/processPayment")]
    public async Task<IActionResult> processPayment(int id, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.Include(item => item.Reservation).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        var result = await processPayment(payment, "payment", cancellationToken);
        payment.updatePaymentData(PaymentStatus.Paid);
        if (payment.Reservation != null)
        {
            payment.Reservation.updateReservationData(ReservationStatus.Paid);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { message = result.Message, payment.PaymentStatus });
    }

    [HttpPost("{id}/requestRefund")]
    public async Task<IActionResult> requestRefund(int id, CancellationToken cancellationToken)
    {
        var payment = await _context.Payments.Include(item => item.Reservation).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (payment == null)
        {
            return NotFound();
        }

        var result = await requestRefund(payment, cancellationToken);
        if (!result.Success)
        {
            return StatusCode(StatusCodes.Status502BadGateway, result.Message);
        }

        payment.updatePaymentData(PaymentStatus.Refunded);
        if (payment.Reservation != null)
        {
            payment.Reservation.updateReservationData(ReservationStatus.Cancelled);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { message = result.Message, payment.PaymentStatus });
    }

    private void savePaymentData(Payment payment)
    {
        payment.savePaymentData();
    }

    private Task<PaymentPageResponse> requestPaymentPage(Payment payment, CancellationToken cancellationToken = default)
    {
        return getPaymentPage(payment, cancellationToken);
    }

    private Task<PaymentPageResponse> getPaymentPage(Payment payment, CancellationToken cancellationToken = default)
    {
        var bankUrl = _configuration["ExternalApis:BankingUrl"];
        var paymentPage = string.IsNullOrWhiteSpace(bankUrl)
            ? "local-banking-adapter://payment"
            : $"{bankUrl.TrimEnd('/')}/payment-page/{payment.Id}";

        return Task.FromResult(new PaymentPageResponse(
            payment.Id,
            payment.Amount,
            payment.ReservationId,
            paymentPage,
            "Payment page data prepared"));
    }

    private async Task<PaymentProcessResult> requestPayment(Payment payment, CancellationToken cancellationToken = default)
    {
        return await processPayment(payment, "payment", cancellationToken);
    }

    private async Task<PaymentProcessResult> requestRefund(Payment payment, CancellationToken cancellationToken = default)
    {
        return await processPayment(payment, "refund", cancellationToken);
    }

    private async Task<PaymentProcessResult> processPayment(Payment payment, string operation, CancellationToken cancellationToken = default)
    {
        var bankUrl = _configuration["ExternalApis:BankingUrl"];
        if (string.IsNullOrWhiteSpace(bankUrl))
        {
            return new PaymentProcessResult(true, $"Local banking adapter accepted {operation}");
        }

        var payload = JsonSerializer.Serialize(new
        {
            payment.Id,
            payment.Amount,
            payment.Date,
            payment.TripId,
            payment.ReservationId,
            operation
        });

        using var content = new StringContent(payload, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync($"{bankUrl.TrimEnd('/')}/{operation}", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return new PaymentProcessResult(true, $"Banking service accepted {operation}");
    }
}

public record PaymentProcessResult(bool Success, string Message);

public record PaymentPageResponse(int PaymentId, decimal Amount, int? ReservationId, string PaymentPage, string Message);
