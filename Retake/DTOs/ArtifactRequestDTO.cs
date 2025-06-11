namespace Retake.DTOs;

public class ArtifactRequestDTO
{
    public ArtifactInsertDTO Artifact { get; set; }
    public ProjectRequetDTO Project { get; set; }
}

public class ArtifactInsertDTO
{
    public int ArtifactId { get; set; }
    public string Name { get; set; }
    public DateTime OriginDate { get; set; }
    public int InstitutionId { get; set; }
}

public class ProjectRequetDTO
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}