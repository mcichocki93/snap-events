namespace WeddingPhotos.Domain.Constants;

public static class ApplicationConstants
{
    public static class FileUpload
    {
        public const long MaxFileSizeBytes = 104857600; // 100MB
        public const long DefaultMaxFileSizeBytes = 10485760; // 10MB
        public const int MaxFileNameLength = 255;
    }

    public static class Pagination
    {
        public const int MinPage = 1;
        public const int MaxPage = 1000;
        public const int MinPageSize = 1;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 50;
    }

    public static class EventTypes
    {
        public const string Wedding = "Wedding";
        public const string Birthday = "Birthday";
        public const string Baptism = "Baptism";
        public const string Communion = "Communion";
        public const string Corporate = "Corporate";
        public const string Conference = "Conference";
        public const string Other = "Other";

        public static readonly string[] All = new[]
        {
            Wedding,
            Birthday,
            Baptism,
            Communion,
            Corporate,
            Conference,
            Other
        };
    }

    public static class EventTypeDisplayNames
    {
        public const string Wedding = "Wesele";
        public const string Birthday = "Urodziny";
        public const string Baptism = "Chrzciny";
        public const string Communion = "Komunia";
        public const string Corporate = "Impreza firmowa";
        public const string Conference = "Konferencja";
        public const string Other = "Inna uroczystość";
        public const string Default = "Uroczystość";
    }

    public static class EventTypeEmojis
    {
        public const string Wedding = "💍";
        public const string Birthday = "🎂";
        public const string Baptism = "👶";
        public const string Communion = "🎓";
        public const string Corporate = "🎉";
        public const string Conference = "📊";
        public const string Other = "🎊";
        public const string Default = "📸";
    }

    public static class AllowedMimeTypes
    {
        public static readonly HashSet<string> Images = new()
        {
            "image/jpeg",
            "image/jpg",
            "image/png",
            "image/gif",
            "image/webp",
            "image/heic",
            "image/heif"
        };
    }

    public static class AllowedFileExtensions
    {
        public static readonly HashSet<string> Images = new()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".webp",
            ".heic",
            ".heif"
        };
    }

    public static class Cache
    {
        public const int PhotoProxyDurationSeconds = 3600; // 1 hour
    }

    public static class Validation
    {
        public const int GuidMinLength = 6;
        public const int GuidMaxLength = 50;
    }

    public static class ErrorMessages
    {
        // Polish error messages
        public const string InvalidGuidFormat = "Format nieprawidłowy";
        public const string InvalidInputData = "Nieprawidłowe dane wejściowe";
        public const string GalleryNotFound = "Galeria nie została znaleziona";
        public const string GalleryDeactivated = "Galeria została dezaktywowana";
        public const string GalleryExpired = "Link do galerii wygasł - nie można już dodawać zdjęć";
        public const string NoFileSelected = "Nie wybrano pliku";
        public const string FileTooBig = "Plik jest za duży. Maksymalny rozmiar: {0}MB";
        public const string InvalidFileType = "Nieprawidłowy typ pliku. Dozwolone: JPG, PNG, GIF, WEBP";
        public const string UploadError = "Nie udało się przesłać pliku";
        public const string GeneralUploadError = "Wystąpił błąd podczas przesyłania pliku";
        public const string LoadingGalleryError = "Wystąpił błąd podczas ładowania galerii";
        public const string PhotoNotFound = "Nie znaleziono zdjęcia";
        public const string GeneralError = "Wystąpił błąd";
        public const string InvalidIdentifier = "Nieprawidłowy identyfikator";
        public const string InvalidPageNumber = "Nieprawidłowy numer strony";
        public const string InvalidPageSize = "Nieprawidłowy rozmiar strony";
    }

    public static class SuccessMessages
    {
        public const string PhotoUploadedSuccessfully = "Zdjęcie zostało pomyślnie przesłane";
    }
}
