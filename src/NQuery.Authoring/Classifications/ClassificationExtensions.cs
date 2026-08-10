namespace NQuery.Authoring.Classifications;

public static class ClassificationExtensions
{
    extension(AuthoringServicesBuilder builder)
    {
        public AuthoringServicesBuilder AddClassificationService()
        {
            ThrowIfNull(builder);

            return builder.AddService(_ => new ClassificationService());
        }
    }
}
