using CloudinaryDotNet.Actions;

namespace API.Interface;

public interface IPhotoServices
{
    Task<ImageUploadResult> AddPhotoAsync(IFormFile file);
    Task<DeletionResult> DeletionPhotoAsync(string publicId);
}