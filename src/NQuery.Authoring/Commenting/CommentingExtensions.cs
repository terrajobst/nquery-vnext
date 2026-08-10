namespace NQuery.Authoring.Commenting;

public static class CommentingExtensions
{
    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddCommentingService()
        {
            ThrowIfNull(builder);

            return builder.AddService(_ => new CommentingService());
        }
    }
}
