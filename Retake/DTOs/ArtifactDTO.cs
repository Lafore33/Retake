namespace Retake.DTOs;

public class ArtifactDTO
{
    public string Name { get; set; }
    public DateTime OriginDate { get; set; }
    public InstitutionDTO Institution { get; set; }
}
