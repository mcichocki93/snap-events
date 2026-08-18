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
    // Longest edge for gallery grid tiles, sized for high-DPI screens.
    private const int GridThumbnailPixels = 400;

    private readonly IGalleryService _galleryService;
    private readonly ILogger<PhotoController> _logger;

    public PhotoController(
        IGalleryService galleryService,
        ILogger<PhotoController> logger)
    {
        _galleryService = galleryService;
        _logger = logger;
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

    /// <summary>
    /// Opens a resumable upload session. The browser writes chunks straight to
    /// Drive from here, so a transfer cut short by the screen locking picks up
    /// where it stopped, and photo bytes never travel through this server.
    /// </summary>
    [HttpPost("upload-session/{guid}")]
    [ProducesResponseType(typeof(CreateUploadSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateUploadSessionResponse>> CreateUploadSession(
        string guid,
        [FromBody] CreateUploadSessionRequest request)
    {
        if (!InputValidator.IsValidGuid(guid) || InputValidator.ContainsSuspiciousPatterns(guid))
        {
            _logger.LogWarning(
                "Upload session attempt with invalid GUID from {IP}: {Guid}",
                HttpContext.Connection.RemoteIpAddress, guid);

            return BadRequest(new CreateUploadSessionResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.InvalidGuidFormat
            });
        }

        var (success, response, _) = await _galleryService.CreateUploadSessionAsync(
            guid,
            request.FileName,
            request.MimeType,
            request.Size,
            Request.Headers.Origin.ToString());

        return success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Confirms the browser finished writing to a session, so the gallery cache
    /// is dropped and the reserved quota slot stands.
    /// </summary>
    [HttpPost("upload-session/{guid}/complete")]
    [ProducesResponseType(typeof(UploadPhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadPhotoResponse>> CompleteUploadSession(
        string guid,
        [FromBody] CompleteUploadRequest request)
    {
        if (!InputValidator.IsValidGuid(guid) || InputValidator.ContainsSuspiciousPatterns(guid))
        {
            return BadRequest(new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.InvalidGuidFormat
            });
        }

        if (!InputValidator.IsValidGuid(request.PhotoId))
        {
            return BadRequest(new UploadPhotoResponse
            {
                Success = false,
                Message = ApplicationConstants.ErrorMessages.InvalidIdentifier
            });
        }

        var (success, response, _) = await _galleryService.CompleteUploadSessionAsync(guid, request.PhotoId);

        return success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// Gives back the quota slot reserved for an upload the guest abandoned.
    /// </summary>
    [HttpPost("upload-session/{guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> CancelUploadSession(string guid)
    {
        if (!InputValidator.IsValidGuid(guid) || InputValidator.ContainsSuspiciousPatterns(guid))
            return BadRequest();

        await _galleryService.CancelUploadSessionAsync(guid);

        return NoContent();
    }

    /// <summary>
    /// Serves a photo for a given gallery. The gallery is part of the route
    /// because a photo ID on its own used to be enough to fetch any photo from
    /// any gallery.
    /// </summary>
    [HttpGet("proxy/{guid}/{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ResponseCache(Duration = ApplicationConstants.Cache.PhotoProxyDurationSeconds)]
    public async Task<ActionResult> ProxyPhoto(string guid, string photoId, [FromQuery] string? size = null)
    {
        try
        {
            if (!InputValidator.IsValidGuid(guid) || !InputValidator.IsValidGuid(photoId))
            {
                _logger.LogWarning(
                    "Proxy request with invalid identifiers from {IP}: {Guid}/{PhotoId}",
                    HttpContext.Connection.RemoteIpAddress, guid, photoId);
                return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidIdentifier });
            }

            // The gallery grid asks for "?size=thumb" on every tile. This used to
            // be ignored, so a 50-photo page pulled 50 full-resolution originals.
            int? thumbnailSize = string.Equals(size, "thumb", StringComparison.OrdinalIgnoreCase)
                ? GridThumbnailPixels
                : null;

            var (success, photo, errorMessage) =
                await _galleryService.GetPhotoStreamAsync(guid, photoId, thumbnailSize);

            if (!success || photo == null)
                return NotFound(new { message = errorMessage });

            Response.Headers.Append("Cache-Control", $"public, max-age={ApplicationConstants.Cache.PhotoProxyDurationSeconds}");

            // The stream is not seekable, so nothing downstream can work the
            // length out on its own - without this the response goes out chunked.
            if (photo.Length.HasValue)
                Response.ContentLength = photo.Length.Value;

            return File(photo.Stream, photo.MimeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying photo: {PhotoId}", photoId);
            return StatusCode(500, new { message = ApplicationConstants.ErrorMessages.GeneralError });
        }
    }

    [HttpGet("proxy/{guid}/{photoId}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ProxyPhotoDownload(string guid, string photoId)
    {
        try
        {
            if (!InputValidator.IsValidGuid(guid) || !InputValidator.IsValidGuid(photoId))
                return BadRequest(new { message = ApplicationConstants.ErrorMessages.InvalidIdentifier });

            // Forward the browser's Range header so its download manager can
            // resume a transfer that the screen locking cut short, instead of
            // starting the photo over.
            var rangeHeader = Request.Headers.Range.ToString();

            var (success, photo, errorMessage) =
                await _galleryService.GetPhotoStreamAsync(guid, photoId, rangeHeader: rangeHeader);

            if (!success || photo == null)
                return NotFound(new { message = errorMessage });

            var safeFileName = InputValidator.SanitizeFileName(photo.FileName);

            _logger.LogInformation("Photo download: {PhotoId}, IP: {IP}",
                photoId, HttpContext.Connection.RemoteIpAddress);

            // Advertised even on a full response, so the browser knows it may
            // ask for a range if this one gets interrupted.
            Response.Headers.AcceptRanges = "bytes";

            if (photo.Length.HasValue)
                Response.ContentLength = photo.Length.Value;

            if (photo.IsPartial)
            {
                Response.Headers.ContentRange = photo.ContentRange;
                Response.StatusCode = StatusCodes.Status206PartialContent;
            }

            return File(photo.Stream, photo.MimeType, safeFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error proxying photo download: {PhotoId}", photoId);
            return StatusCode(500, new { message = ApplicationConstants.ErrorMessages.GeneralError });
        }
    }
}