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

    // Step 5: getAllPOI()
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PointOfInterest>>> GetPOIs()
    {
        return await _context.PointsOfInterest.ToListAsync(); // Returns Step 7: All POIs
    }

    // Step 21: getSpecificPOI()
    [HttpGet("{id}")]
    public async Task<ActionResult<PointOfInterest>> GetPOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id);
        if (poi == null) return NotFound();
        return poi; // Returns Step 23: POI data
    }

    // Step 12: createNewPOI()
    [HttpPost]
    public async Task<ActionResult<PointOfInterest>> CreatePOI(PointOfInterest poi)
    {
        // Step 13: checkPOI() - Basic Validation
        if (string.IsNullOrWhiteSpace(poi.Name))
            return BadRequest("Name is required.");

        _context.PointsOfInterest.Add(poi); // Step 14: savePOI()
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPOI), new { id = poi.Id }, poi); // Step 16: POI save status
    }

    // Step 29: editPOIData()
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePOI(int id, PointOfInterest poi)
    {
        if (id != poi.Id) return BadRequest();

        // Step 30: checkPOIEdit()
        _context.Entry(poi).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(); // Step 31: savePOI()
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.PointsOfInterest.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return Ok(new { message = "Edit successful" }); // Step 33: save POI message
    }

    // Step 38: deleteSelectedPOI()
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePOI(int id)
    {
        var poi = await _context.PointsOfInterest.FindAsync(id); // Step 39: deletePOI()
        if (poi == null) return NotFound();

        _context.PointsOfInterest.Remove(poi);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Successful deletion" }); // Step 41: deletion message
    }
}