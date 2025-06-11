namespace Retake.DTOs;

public class ProjectDTO
{
    public int ProjectId { get; set; }
    public string Objective { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ArtifactDTO Artifact { get; set; }
    
    public List<StaffAssignmentDTO> StaffAssignments { get; set; }
}
