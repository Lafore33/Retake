using System.Data;
using Microsoft.Data.SqlClient;
using Retake.DTOs;

namespace Retake.Service;


public class ProjectNotFoundException : Exception;
public class ConflictException(string message) : Exception(message);

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("Default")!;
    }

    public async Task<ProjectDTO> GetProject(int id)
    {
        var command =
            """
                SELECT PP.ProjectId, PP.StartDate, PP.EndDate, PP.Objective, A.Name, A.OriginDate, I.InstitutionId, I.Name, I.FoundedYear, S.FirstName, S.LastName, S.HireDate, SA.Role
                       FROM Preservation_Project PP JOIN dbo.Artifact A on A.ArtifactId = PP.ArtifactId 
                                                    JOIN Institution I ON A.InstitutionId = I.InstitutionId
                                                    JOIN dbo.Staff_Assignment SA on PP.ProjectId = SA.ProjectId
                                                    JOIN dbo.Staff S on S.StaffId = SA.StaffId
                                                                                WHERE PP.ProjectId = @Id;
            """;
        
        await using var connection = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand(command, connection);

        await connection.OpenAsync();

        cmd.Parameters.AddWithValue("@Id", id);
        
        await using var reader = await cmd.ExecuteReaderAsync();
        var flag = false;
        var project = new ProjectDTO();
        while (await reader.ReadAsync())
        {
            if (!flag)
            {
                project = new ProjectDTO()
                {
                    ProjectId = reader.GetInt32(0),
                    Objective = reader.GetString(3),
                    StartDate = reader.GetDateTime(1),

                    Artifact = new ArtifactDTO()
                    {
                        Name = reader.GetString(4),
                        OriginDate = reader.GetDateTime(5),
                        Institution = new InstitutionDTO()
                        {
                            InstitutionId = reader.GetInt32(6),
                            Name = reader.GetString(7),
                            FoundedYear = reader.GetInt32(8)
                        }
                    },
                    StaffAssignments = []
                };
                try
                {
                    project.EndDate = reader.GetDateTime(2);
                }
                catch (Exception e)
                {
                    project.EndDate = null;
                }

                flag = true;
                
            }
            
            project.StaffAssignments.Add(
                new StaffAssignmentDTO()
                {
                    FirstName = reader.GetString(9),
                    LastName = reader.GetString(10),
                    HireDate = reader.GetDateTime(11),
                    Role = reader.GetString(12)
                        
                }
            );
            
        }

        if (flag == false) throw new ProjectNotFoundException();
        return project;
    }

    private async Task<bool> DoesInstitutionExist(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand("SELECT 1 from Institution WHERE InstitutionId = @id", connection);

        await connection.OpenAsync();

        cmd.Parameters.AddWithValue("@id", id);

        var result = await cmd.ExecuteScalarAsync();
        
        return result != null;
    }
    
    private async Task<bool> DoesProjectExist(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand("SELECT 1 from Preservation_Project WHERE ProjectId = @id", connection);

        await connection.OpenAsync();

        cmd.Parameters.AddWithValue("@id", id);

        var result = await cmd.ExecuteScalarAsync();
        
        return result != null;
    }
    
    private async Task<bool> DoesArtifactExist(int id)
    {
        await using var connection = new SqlConnection(_connectionString);
        await using var cmd = new SqlCommand("SELECT 1 from Artifact WHERE ArtifactId = @id", connection);

        await connection.OpenAsync();

        cmd.Parameters.AddWithValue("@id", id);

        var result = await cmd.ExecuteScalarAsync();
        
        return result != null;
    }

    public async Task<bool> SaveArtifact(ArtifactRequestDTO artifactRequestDto)
    {
        var doesInstitutionExist = await DoesInstitutionExist(artifactRequestDto.Artifact.InstitutionId);
        if (!doesInstitutionExist) throw new ConflictException("The provided institution does not exist");
        
        var doesProjectExist = await DoesProjectExist(artifactRequestDto.Project.ProjectId);
        if (doesProjectExist) throw new ConflictException("The Project with the provided ID already exists");

        var doesArtifactExist = await DoesArtifactExist(artifactRequestDto.Artifact.ArtifactId);
        if (doesArtifactExist) throw new ConflictException("The Artifact with the provided ArtifactID already exists");
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            await using (var cmd = new SqlCommand(
                             "INSERT INTO Artifact VALUES (@ArtifactId, @Name, @OriginDate, @InstitutionId);",
                             connection, (SqlTransaction)transaction))
            {

                cmd.Parameters.AddWithValue("@ArtifactId", artifactRequestDto.Artifact.ArtifactId);
                cmd.Parameters.AddWithValue("@Name", artifactRequestDto.Artifact.Name);
                cmd.Parameters.AddWithValue("@OriginDate", artifactRequestDto.Artifact.OriginDate);
                cmd.Parameters.AddWithValue("@InstitutionId", artifactRequestDto.Artifact.InstitutionId);
                await cmd.ExecuteNonQueryAsync();
                
                await using (var cmd2 = new SqlCommand(
                                 "INSERT INTO Preservation_Project VALUES (@ProjectId, @ArtifactId, @StartDate, @EndDate, @Objective)",
                                 connection, (SqlTransaction)transaction))
                {
                    cmd2.Parameters.AddWithValue("@ProjectId", artifactRequestDto.Project.ProjectId);
                    cmd2.Parameters.AddWithValue("@ArtifactId", artifactRequestDto.Artifact.ArtifactId);
                    cmd2.Parameters.AddWithValue("@StartDate", artifactRequestDto.Project.StartDate);
                    cmd2.Parameters.AddWithValue("@EndDate", artifactRequestDto.Project.EndDate == null ? DBNull.Value : artifactRequestDto.Project.EndDate);
                    cmd2.Parameters.AddWithValue("@Objective", artifactRequestDto.Project.Objective);

                    await cmd2.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
        }
        catch 
        {
            await transaction.RollbackAsync();
            throw;
        }

        return true;

    }
}