using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;


[ApiController]
[Route("api/[controller]")]
public class POIController : ControllerBase
{
    private readonly AppDbContext _context;

    public POIController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> GetPOIs()
    {
        return await _context.PointsOfInterest.ToListAsync(); 
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PointOfInterest>> GetPOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id);
        if (poi == null) return NotFound();
        return poi; 
    }


    [HttpPost]
    public async Task<ActionResult<PointOfInterest>> CreatePOI(PointOfInterest poi)
    {
        if (string.IsNullOrWhiteSpace(poi.Name))
            return BadRequest("Name is required.");

        _context.PointsOfInterest.Add(poi); 
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPOI), new { id = poi.Id }, poi); 
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePOI(int id, PointOfInterest poi)
    {
        if (id != poi.Id) return BadRequest();


        _context.Entry(poi).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(); 
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.PointsOfInterest.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return Ok(new { message = "Edit successful" }); 
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id); 
        if (poi == null) return NotFound();

        _context.PointsOfInterest.Remove(poi);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successful deletion" }); 
    }
}