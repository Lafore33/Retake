using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Retake.Service;

namespace Retake.Controller;

[Route("api/[controller]")]
[ApiController]
public class ProjectsController : ControllerBase
{
    private readonly IDbService _dbService;

    public ProjectsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProject(int id)
    {
        try
        {
            var project = await _dbService.GetProject(id);
            return Ok(project);
        }
        catch (ProjectNotFoundException e)
        {
            return NotFound();
        }
    }
}