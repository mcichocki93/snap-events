using Microsoft.AspNetCore.Mvc;
using WeddingPhotos.Domain.Constants;
using WeddingPhotos.Domain.DTOs;
using WeddingPhotos.Domain.Interfaces;
using WeddingPhotos.Domain.Models;
using WeddingPhotos.Domain.Validation;

namespace WeddingPhotos.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class PhotoController : ControllerBase
{
    private readonly IGalleryService _galleryService;
    private readonly ILogger<PhotoController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public PhotoController(
        IGalleryService galleryService,
        ILogger<PhotoController> logger,
        IHttpClientFactory httpClientFactory)
    {
        _galleryService = galleryService;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("gallery/{guid}")]
    [ProducesResponseType(typeof(GalleryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GalleryResponse>> GetGallery(
        string guid,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = ApplicationConstants.Pagination.DefaultPageSize)
    {
        // SECURITY: Validate GUID format
        if (!InputValidator.IsValidGuid(guid))
        {
            _logger.LogWarning(
                "Invalid GUID format attempted from {IP}: {Guid}",
                HttpContext.Connection.RemoteIpAddress,
                guid
            );
            return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidGuidFormat });
        }

        // SECURITY: Check for injection attempts
        if (InputValidator.ContainsSuspiciousPatterns(guid))
        {
            _logger.LogWarning(
                "Possible MongoDB injection attempt from {IP}: {Guid}",
                HttpContext.Connection.RemoteIpAddress,
                guid
            );
            return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidInputData });
        }

        // Delegate business logic to service
        var (success, response, errorMessage) = await _galleryService.GetGalleryAsync(guid, page, pageSize);

        if (!success)
        {
            if (errorMessage == ApplicationConstants.ErrorMessages.GalleryNotFound)
            {
                return NotFound(new { message = errorMessage });
            }

            return BadRequest(new { message = errorMessage });
        }

        return Ok(response);
    }

    [HttpPost("upload/{guid}")]
    [ProducesResponseType(typeof(UploadPhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(ApplicationConstants.FileUpload.MaxFileSizeBytes)]
    public async Task<ActionResult<UploadPhotoResponse>> UploadPhoto(
        string guid,
        IFormFile file)
    {
        // SECURITY: Validate GUID
        if (!InputValidator.IsValidGuid(guid))
        {
            _logger.LogWarning(
                "Upload attempt with invalid GUID from {IP}: {Guid}",
                HttpContext.Connection.RemoteIpAddress,
                guid
            );
            return BadRequest(new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.InvalidGuidFormat
            });
        }

        // SECURITY: Check for injection
        if (InputValidator.ContainsSuspiciousPatterns(guid))
        {
            _logger.LogWarning(
                "Upload injection attempt from {IP}: {Guid}",
                HttpContext.Connection.RemoteIpAddress,
                guid
            );
            return BadRequest(new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.InvalidInputData
            });
        }

        // Check if file is provided
        if (file == null || file.Length == 0)
        {
            return BadRequest(new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.NoFileSelected
            });
        }

        _logger.LogInformation(
            "Upload started: GUID={Guid}, File={FileName}, Size={Size}MB, IP={IP}",
            guid,
            file.FileName,
            file.Length / (1024.0 * 1024.0),
            HttpContext.Connection.RemoteIpAddress
        );

        // Delegate business logic to service
        using var stream = file.OpenReadStream();
        var (success, response, errorMessage) = await _galleryService.UploadPhotoAsync(
            guid,
            stream,
            file.FileName,
            file.ContentType,
            file.Length);

        if (!success)
        {
            if (errorMessage == ApplicationConstants.ErrorMessages.GalleryNotFound)
            {
                return NotFound(response);
            }

            if (errorMessage == ApplicationConstants.ErrorMessages.UploadError)
            {
                return StatusCode(500, response);
            }

            if (errorMessage == ApplicationConstants.ErrorMessages.GeneralUploadError)
            {
                return StatusCode(500, response);
            }

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("proxy/{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = ApplicationConstants.Cache.PhotoProxyDurationSeconds)]
    public async Task<ActionResult> ProxyPhoto(string photoId, [FromQuery] string? size = null)
    {
        try
        {
            // SECURITY: Validate photoId
            if (!InputValidator.IsValidGuid(photoId))
            {
                _logger.LogWarning(
                    "Proxy request with invalid photoId from {IP}: {PhotoId}",
                    HttpContext.Connection.RemoteIpAddress,
                    photoId
                );
                return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidIdentifier });
            }

            // Get download URL from service
            var (success, downloadUrl, errorMessage) = await _galleryService.GetPhotoDownloadUrlAsync(photoId);

            if (!success || string.IsNullOrEmpty(downloadUrl))
            {
                return NotFound(new { message = errorMessage });
            }

            var httpClient = _httpClientFactory.CreateClient("PhotoProxy");

            var response = await httpClient.GetAsync(downloadUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Failed to fetch photo from Google Drive: {PhotoId}, Status: {Status}",
                    photoId, response.StatusCode
                );
                return NotFound(new { message = ApplicationConstants.ErrorMessages.PhotoNotFound });
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            var stream = await response.Content.ReadAsStreamAsync();

            Response.Headers.Append("Cache-Control", $"public, max-age={ApplicationConstants.Cache.PhotoProxyDurationSeconds}");

            return File(stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying photo: {PhotoId}", photoId);
            return StatusCode(500, new { message = ApplicationConstants.ErrorMessages.GeneralError });
        }
    }

    [HttpGet("proxy/{photoId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ProxyPhotoDownload(string photoId)
    {
        try
        {
            // SECURITY: Validate photoId
            if (!InputValidator.IsValidGuid(photoId))
            {
                return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidIdentifier });
            }

            // Get download URL from service
            var (success, downloadUrl, errorMessage) = await _galleryService.GetPhotoDownloadUrlAsync(photoId);

            if (!success || string.IsNullOrEmpty(downloadUrl))
            {
                return NotFound(new { message = errorMessage });
            }

            var httpClient = _httpClientFactory.CreateClient("PhotoProxy");
            var response = await httpClient.GetAsync(downloadUrl);

            if (!response.IsSuccessStatusCode)
            {
                return NotFound(new { message = ApplicationConstants.ErrorMessages.PhotoNotFound });
            }

            var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            var fileName = $"photo_{photoId}.jpg";

            if (response.Content.Headers.ContentDisposition?.FileName != null)
            {
                fileName = InputValidator.SanitizeFileName(
                    response.Content.Headers.ContentDisposition.FileName.Trim('"')
                );
            }

            var stream = await response.Content.ReadAsStreamAsync();

            _logger.LogInformation(
                "Photo download: {PhotoId}, IP: {IP}",
                photoId,
                HttpContext.Connection.RemoteIpAddress
            );

            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying photo download: {PhotoId}", photoId);
            return StatusCode(500, new { message = ApplicationConstants.ErrorMessages.GeneralError });
        }
    }
}