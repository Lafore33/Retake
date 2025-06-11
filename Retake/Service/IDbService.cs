using Retake.DTOs;

namespace Retake.Service;

public interface IDbService
{

    Task<ProjectDTO> GetProject(int id);

    Task<bool> SaveArtifact(ArtifactRequestDTO artifactRequestDto);
}