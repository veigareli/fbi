using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Models;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRoundPointsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UserRoundPointsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/userroundpoints
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRoundPoints>>> GetUserRoundPoints()
    {
        return await _context.UserRoundPoints
            .Include(urp => urp.User)
            .ToListAsync();
    }

    // GET: api/userroundpoints/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserRoundPoints>> GetUserRoundPoint(int id)
    {
        var userRoundPoint = await _context.UserRoundPoints
            .Include(urp => urp.User)
            .FirstOrDefaultAsync(urp => urp.Id == id);

        if (userRoundPoint == null)
        {
            return NotFound();
        }

        return userRoundPoint;
    }

    // POST: api/userroundpoints
    [HttpPost]
    public async Task<ActionResult<UserRoundPoints>> PostUserRoundPoint(UserRoundPoints userRoundPoint)
    {
        _context.UserRoundPoints.Add(userRoundPoint);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUserRoundPoint", new { id = userRoundPoint.Id }, userRoundPoint);
    }

    // PUT: api/userroundpoints/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUserRoundPoint(int id, UserRoundPoints userRoundPoint)
    {
        if (id != userRoundPoint.Id)
        {
            return BadRequest();
        }

        _context.Entry(userRoundPoint).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserRoundPointExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/userroundpoints/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserRoundPoint(int id)
    {
        var userRoundPoint = await _context.UserRoundPoints.FindAsync(id);
        if (userRoundPoint == null)
        {
            return NotFound();
        }

        _context.UserRoundPoints.Remove(userRoundPoint);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // GET: api/userroundpoints/user/{userId}
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<UserRoundPoints>>> GetUserRoundPointsByUser(int userId)
    {
        var userRoundPoints = await _context.UserRoundPoints
            .Where(urp => urp.UserId == userId)
            .Include(urp => urp.User)
            .OrderBy(urp => urp.Round)
            .ToListAsync();

        return userRoundPoints;
    }

    // GET: api/userroundpoints/round/{round}
    [HttpGet("round/{round}")]
    public async Task<ActionResult<IEnumerable<UserRoundPoints>>> GetUserRoundPointsByRound(int round)
    {
        var userRoundPoints = await _context.UserRoundPoints
            .Where(urp => urp.Round == round)
            .Include(urp => urp.User)
            .OrderByDescending(urp => urp.Points)
            .ToListAsync();

        return userRoundPoints;
    }

    // POST: api/userroundpoints/bulk
    [HttpPost("bulk")]
    public async Task<ActionResult<IEnumerable<UserRoundPoints>>> PostUserRoundPointsBulk(IEnumerable<UserRoundPoints> userRoundPoints)
    {
        _context.UserRoundPoints.AddRange(userRoundPoints);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUserRoundPoints", userRoundPoints);
    }

    private bool UserRoundPointExists(int id)
    {
        return _context.UserRoundPoints.Any(e => e.Id == id);
    }
}
