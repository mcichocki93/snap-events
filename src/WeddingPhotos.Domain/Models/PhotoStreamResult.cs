namespace WeddingPhotos.Domain.Models;

/// <summary>
/// A photo being streamed out of storage.
///
/// This is a record rather than a tuple because serving a photo now carries
/// enough detail - size, partial-content bounds - that positional tuples had
/// stopped being readable at the call sites.
/// </summary>
public sealed record PhotoStreamResult
{
    public required Stream Stream { get; init; }

    public required string MimeType { get; init; }

    public required string FileName { get; init; }

    /// <summary>
    /// Bytes in this response. Null when storage did not report a length, in
    /// which case the response goes out chunked.
    /// </summary>
    public long? Length { get; init; }

    /// <summary>
    /// Set when the caller asked for a byte range and storage honoured it. The
    /// response must then be 206 with this as its Content-Range.
    /// </summary>
    public string? ContentRange { get; init; }

    public bool IsPartial => ContentRange != null;
}
