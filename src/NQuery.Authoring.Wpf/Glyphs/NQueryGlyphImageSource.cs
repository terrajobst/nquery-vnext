using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NQuery.Authoring.Wpf
{
    public static class NQueryGlyphImageSource
    {
        private static readonly Dictionary<string, BitmapImage> Images = new();

        private static string FromName(string iconName)
        {
            var assemblyName = typeof(NQueryGlyphImageSource).Assembly.GetName().Name;
            var uri = $"pack://application:,,,/{assemblyName};component/Images/{iconName}.png";
            return uri;
        }

        private static string FromGlyph(Glyph glyph)
        {
            switch (glyph)
            {
                case Glyph.AmbiguousName:
                    return FromName(@"AmbiguousName");
                case Glyph.Keyword:
                    return FromName(@"Keyword");
                case Glyph.Variable:
                    return FromName(@"Variable");
                case Glyph.Relation:
                    return FromName(@"Relation");
                case Glyph.Table:
                    return FromName(@"Table");
                case Glyph.TableInstance:
                    return FromName(@"TableInstance");
                case Glyph.Aggregate:
                    return FromName(@"Aggregate");
                case Glyph.Column:
                    return FromName(@"Column");
                case Glyph.Function:
                    return FromName(@"Function");
                case Glyph.Method:
                    return FromName(@"Method");
                case Glyph.Property:
                    return FromName(@"Property");
                case Glyph.Type:
                    return FromName(@"Type");
                default:
                    throw ExceptionBuilder.UnexpectedValue(glyph);
            }
        }

        public static ImageSource Get(Glyph glyph)
        {
            var imageUri = FromGlyph(glyph);
            if (imageUri is null)
                return null;

            if (!Images.TryGetValue(imageUri, out var result))
            {
                result = new BitmapImage(new Uri(imageUri));
                Images.Add(imageUri, result);
            }

            return result;
        }

        public static ImageSource Get(Symbol symbol)
        {
            var glyph = symbol.GetGlyph();
            return Get(glyph);
        }
    }
}