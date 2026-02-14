using Microsoft.EntityFrameworkCore;
using Npgsql;
using Stayvelle.DB;
using Stayvelle.IRepository;
using Stayvelle.Models;
using Stayvelle.Query;

using Microsoft.Extensions.Configuration;
using Stayvelle.Services;
using Stayvelle.Models.DTOs;

namespace Stayvelle.RepositoryImpl
{
    public class UserRepository : IUsers
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Create
        public async Task<Response<UsersModel>> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            Response<UsersModel> response = new Response<UsersModel>();
            try
            {
                // Check if email already exists
                var existingEmail = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == createUserDTO.Email);
                if (existingEmail != null)
                {
                    response.Success = false;
                    response.Message = "User with this email already exists";
                    return response;
                }

                // Check if username already exists
                var existingUsername = await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == createUserDTO.Username);
                if (existingUsername != null)
                {
                    response.Success = false;
                    response.Message = "User with this username already exists";
                    return response;
                }

                var user = new UsersModel
                {
                    Name = createUserDTO.Name,
                    Email = createUserDTO.Email,
                    Password = createUserDTO.Password,
                    Username = createUserDTO.Username,
                    isactive = createUserDTO.isactive,
                    isstaff = createUserDTO.isstaff,
                    isadmin = createUserDTO.isadmin,
                    isdelete = false,
                    Phone = createUserDTO.Phone,
                    role_id = createUserDTO.role_id,
                    role_name = createUserDTO.role_name,
                    IsHousekeeping = createUserDTO.IsHousekeeping,
                    // ImageUrl removed
                    CreatedBy = createUserDTO.CreatedBy,
                    CreatedOn = DateTime.UtcNow
                };

                _context.UsersModel.Add(user);
                await _context.SaveChangesAsync();

                // Handle Documents
                if (createUserDTO.Documents != null && createUserDTO.Documents.Any())
                {
                    string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                    foreach (var doc in createUserDTO.Documents)
                    {
                        string filePath = "";
                        if (doc.File != null)
                        {
                            // Assuming Uploads.UploadImage returns the relative path or full path
                            filePath = await Uploads.UploadImage(doc.FileName, doc.File, "USER", baseUrl);
                        }

                        var newDoc = new DocumentModel
                        {
                            EntityType = "USER",
                            EntityId = user.Id,
                            DocumentType = doc.DocumentType ?? "USER_DOC",
                            FileName = doc.FileName,
                            Description = doc.Description,
                            FilePath = filePath,
                            IsPrimary = doc.IsPrimary,
                            CreatedBy = user.CreatedBy ?? "system",
                            CreatedOn = DateTime.UtcNow
                        };
                        _context.DocumentModel.Add(newDoc);
                    }
                    await _context.SaveChangesAsync();
                    
                    // Populate back into user object for response? 
                    // user.Documents = ... (Need to map back to DTO if needed, but response data is UsersModel which has Documents list)
                    // Let's populate it so the response checks out.
                    user.Documents = createUserDTO.Documents; // Note: DTOs mapped to DTOs in Model? Model has List<DocumentDto>
                }

                response.Success = true;
                response.Message = "User created successfully";
                response.Data = user;

                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var innerMessage = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                response.Message = $"Error while creating user: {ex.Message}{innerMessage}";
                response.Data = null;
                return response;
            }
        }

        // Read - Get All Users
        public async Task<Response<List<UsersModel>>> GetAllUsersAsync()
        {
            var response = new Response<List<UsersModel>>();
            try
            {
               var users = await _context.UsersModel.FromSqlRaw(UserQuery.GetAllUsers).ToListAsync();
               
               if (users != null && users.Any())
               {
                   var userIds = users.Select(u => u.Id).ToList();
                   var documents = await _context.DocumentModel
                       .Where(d => d.EntityType == "USER" && userIds.Contains(d.EntityId))
                       .ToListAsync();

                   foreach (var user in users)
                   {
                       user.Documents = documents
                           .Where(d => d.EntityId == user.Id)
                           .Select(d => new DocumentDto
                           {
                               DocumentId = d.DocumentId,
                               EntityType = d.EntityType,
                               EntityId = d.EntityId,
                               DocumentType = d.DocumentType,
                               FileName = d.FileName,
                               Description = d.Description,
                               FilePath = d.FilePath,
                               IsPrimary = d.IsPrimary
                           }).ToList();
                   }
               }

                response.Success = true;
                response.Message = "success";
                response.Data = users;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<Response<UsersModel>> GetUserByIdAsync(int id)
        {
            var response = new Response<UsersModel>();

            try
            {
                var user = await _context.UsersModel.FindAsync(id);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    response.Data = null;
                    return response;
                }

                var documents = await _context.DocumentModel
                    .Where(d => d.EntityType == "USER" && d.EntityId == id)
                    .ToListAsync();
                
                user.Documents = documents.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    EntityType = d.EntityType,
                    EntityId = d.EntityId,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    Description = d.Description,
                    FilePath = d.FilePath,
                    IsPrimary = d.IsPrimary
                }).ToList();

                response.Success = true;
                response.Message = "Success";
                response.Data = user;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<Response<UsersModel?>> GetUserByEmailAsync(string email)
        {
            var response = new Response<UsersModel>();
            try
            {
                var user = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == email);
                response.Data = user;
                
                if (user != null)
                {
                    var documents = await _context.DocumentModel
                        .Where(d => d.EntityType == "USER" && d.EntityId == user.Id)
                        .ToListAsync();
                    
                    user.Documents = documents.Select(d => new DocumentDto
                    {
                        DocumentId = d.DocumentId,
                        EntityType = d.EntityType,
                        EntityId = d.EntityId,
                        DocumentType = d.DocumentType,
                        FileName = d.FileName,
                        Description = d.Description,
                        FilePath = d.FilePath,
                        IsPrimary = d.IsPrimary
                    }).ToList();
                }

                response.Success = true;
                response.Message = "success";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        public async Task<UsersModel?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == username);
            
            if (user != null)
            {
                var documents = await _context.DocumentModel
                   .Where(d => d.EntityType == "USER" && d.EntityId == user.Id)
                   .ToListAsync();

                user.Documents = documents.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    EntityType = d.EntityType,
                    EntityId = d.EntityId,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    Description = d.Description,
                    FilePath = d.FilePath,
                    IsPrimary = d.IsPrimary
                }).ToList();
            }
            
            return user;
        }

        public async Task<Response<UsersModel>> UpdateUserAsync(int id, UpdateUserDTO updateUserDTO)
        {
            var response = new Response<UsersModel>();

            try
            {
                var existingUser = await _context.UsersModel.FindAsync(id);

                if (existingUser == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    response.Data = null;
                    return response;
                }

                // Check if email is being changed and already exists
                if (!string.IsNullOrEmpty(updateUserDTO.Email) && updateUserDTO.Email != existingUser.Email)
                {
                    var emailExists = await _context.UsersModel.FirstOrDefaultAsync(u => u.Email == updateUserDTO.Email);
                    if (emailExists != null)
                    {
                        response.Success = false;
                        response.Message = "User with this email already exists";
                        return response;
                    }
                }

                // Check if username is being changed and already exists
                if (!string.IsNullOrEmpty(updateUserDTO.Username) && updateUserDTO.Username != existingUser.Username)
                {
                    var usernameExists = await _context.UsersModel.FirstOrDefaultAsync(u => u.Username == updateUserDTO.Username);
                    if (usernameExists != null)
                    {
                        response.Success = false;
                        response.Message = "User with this username already exists";
                        return response;
                    }
                }

                // Update fields
                existingUser.Name = updateUserDTO.Name ?? existingUser.Name;
                existingUser.Email = updateUserDTO.Email ?? existingUser.Email;
                existingUser.Password = updateUserDTO.Password ?? existingUser.Password;
                existingUser.Username = updateUserDTO.Username ?? existingUser.Username;
                existingUser.Phone = updateUserDTO.Phone ?? existingUser.Phone;
                existingUser.role_id = updateUserDTO.role_id ?? existingUser.role_id;
                existingUser.role_name = updateUserDTO.role_name ?? existingUser.role_name;
                existingUser.isactive = updateUserDTO.isactive ?? existingUser.isactive;
                existingUser.isstaff = updateUserDTO.isstaff ?? existingUser.isstaff;
                existingUser.isadmin = updateUserDTO.isadmin ?? existingUser.isadmin;
                
                existingUser.isadmin = updateUserDTO.isadmin ?? existingUser.isadmin;
                
                if(updateUserDTO.IsHousekeeping.HasValue)
                {
                    existingUser.IsHousekeeping = updateUserDTO.IsHousekeeping.Value;
                }

                // Handle Documents
                if (updateUserDTO.Documents != null && updateUserDTO.Documents.Any())
                {
                    string baseUrl = _configuration["BaseUrl"] ?? "https://localhost:7252";
                    foreach (var doc in updateUserDTO.Documents)
                    {
                        if (doc.DocumentId > 0)
                        {
                            // Update existing document
                            var existingDoc = await _context.DocumentModel.FindAsync(doc.DocumentId);
                            if (existingDoc != null && existingDoc.EntityId == existingUser.Id && existingDoc.EntityType == "USER")
                            {
                                if (doc.File != null)
                                {
                                    string filePath = await Uploads.UploadImage(doc.FileName, doc.File, "USER", baseUrl);
                                    existingDoc.FilePath = filePath;
                                    existingDoc.FileName = doc.FileName;
                                }
                                existingDoc.Description = doc.Description;
                                existingDoc.DocumentType = doc.DocumentType ?? existingDoc.DocumentType;
                                existingDoc.IsPrimary = doc.IsPrimary;
                                // Maintain original metadata like CreatedBy/On
                                
                                existingDoc.IsPrimary = doc.IsPrimary;
                                
                                // Fix DateTime Kind for Postgres
                                if (existingDoc.CreatedOn.Kind == DateTimeKind.Unspecified)
                                {
                                    existingDoc.CreatedOn = DateTime.SpecifyKind(existingDoc.CreatedOn, DateTimeKind.Utc);
                                }
                                if (existingDoc.ModifiedOn.HasValue && existingDoc.ModifiedOn.Value.Kind == DateTimeKind.Unspecified)
                                {
                                    existingDoc.ModifiedOn = DateTime.SpecifyKind(existingDoc.ModifiedOn.Value, DateTimeKind.Utc);
                                }
                                
                                _context.DocumentModel.Update(existingDoc);
                            }
                        }
                        else
                        {
                            // Check for existing document of same type to update instead of creating new
                            var existingDocByType = await _context.DocumentModel
                                .FirstOrDefaultAsync(d => d.EntityType == "USER" && d.EntityId == existingUser.Id && d.DocumentType == (doc.DocumentType ?? "USER_DOC"));

                            if (existingDocByType != null)
                            {
                                if (doc.File != null)
                                {
                                    string filePath = await Uploads.UploadImage(doc.FileName, doc.File, "USER", baseUrl);
                                    existingDocByType.FilePath = filePath;
                                    existingDocByType.FileName = doc.FileName;
                                }
                                existingDocByType.Description = doc.Description;
                                // DocumentType is same
                                existingDocByType.IsPrimary = doc.IsPrimary;
                                
                                // DocumentType is same
                                existingDocByType.IsPrimary = doc.IsPrimary;

                                // Fix DateTime Kind for Postgres
                                if (existingDocByType.CreatedOn.Kind == DateTimeKind.Unspecified)
                                {
                                    existingDocByType.CreatedOn = DateTime.SpecifyKind(existingDocByType.CreatedOn, DateTimeKind.Utc);
                                }
                                if (existingDocByType.ModifiedOn.HasValue && existingDocByType.ModifiedOn.Value.Kind == DateTimeKind.Unspecified)
                                {
                                    existingDocByType.ModifiedOn = DateTime.SpecifyKind(existingDocByType.ModifiedOn.Value, DateTimeKind.Utc);
                                }
                                
                                _context.DocumentModel.Update(existingDocByType);
                            }
                            else
                            {
                                // Add new document
                                string filePath = "";
                                if (doc.File != null)
                                {
                                    filePath = await Uploads.UploadImage(doc.FileName, doc.File, "USER", baseUrl);
                                }

                                var newDoc = new DocumentModel
                                {
                                    EntityType = "USER",
                                    EntityId = existingUser.Id,
                                    DocumentType = doc.DocumentType ?? "USER_DOC",
                                    FileName = doc.FileName,
                                    Description = doc.Description,
                                    FilePath = filePath,
                                    IsPrimary = doc.IsPrimary,
                                    CreatedBy = "system", 
                                    CreatedOn = DateTime.UtcNow
                                };
                                _context.DocumentModel.Add(newDoc);
                            }
                        }
                    }
                }

                // Ensure DateTime Kind is UTC for compatibility with Postgres
                if (existingUser.CreatedOn.Kind == DateTimeKind.Unspecified)
                {
                    existingUser.CreatedOn = DateTime.SpecifyKind(existingUser.CreatedOn, DateTimeKind.Utc);
                }
                
                existingUser.ModifiedBy = "system"; // Or pass from DTO if available in common logic 
                existingUser.ModifiedOn = DateTime.UtcNow;

                _context.UsersModel.Update(existingUser);
                await _context.SaveChangesAsync();

                // Fetch updated documents for response
                var updatedDocuments = await _context.DocumentModel
                    .Where(d => d.EntityType == "USER" && d.EntityId == existingUser.Id)
                    .ToListAsync();
                
                existingUser.Documents = updatedDocuments.Select(d => new DocumentDto
                {
                    DocumentId = d.DocumentId,
                    EntityType = d.EntityType,
                    EntityId = d.EntityId,
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    Description = d.Description,
                    FilePath = d.FilePath,
                    IsPrimary = d.IsPrimary
                }).ToList();

                response.Success = true;
                response.Message = "User updated successfully";
                response.Data = existingUser;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var innerMessage = ex.InnerException != null ? $" Inner: {ex.InnerException.Message}" : "";
                response.Message = $"Error while updating user: {ex.Message}{innerMessage}";
                response.Data = null;
                return response;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var userIdParam = new NpgsqlParameter("@Id", id);
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(UserQuery.HardDeleteUser, userIdParam);
            return rowsAffected > 0;
        }

        public async Task<bool> SoftDeleteUserAsync(int id)
        {
            var userIdParam = new NpgsqlParameter("@Id", id);
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(UserQuery.SoftDeleteUser, userIdParam);
            return rowsAffected > 0;
        }
        public async Task<Response<List<UsersModel>>> GetHousekeepingUsersAsync()
        {
            var response = new Response<List<UsersModel>>();
            try
            {
                var users = await _context.UsersModel
                    .Where(u => u.IsHousekeeping && !u.isdelete)
                    .OrderBy(u => u.Name)
                    .ToListAsync();
                
                // Explicitly NOT fetching documents as per requirement

                response.Success = true;
                response.Message = "success";
                response.Data = users;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }
    }
}
