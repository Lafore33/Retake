using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Retake.DTOs;
using Retake.Service;

namespace Retake.Controller;

[Route("api/[controller]")]
[ApiController]
public class ArtifactsController : ControllerBase
{
    
    private readonly IDbService _dbService;

    public ArtifactsController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpPost]
    public async Task<IActionResult> SaveArtifact(ArtifactRequestDTO artifactRequestDto)
    {
        try
        {
            await _dbService.SaveArtifact(artifactRequestDto);
            return Created();
        }
        catch (ConflictException e)
        {
            return Conflict(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return StatusCode(500);
        }
    }
}