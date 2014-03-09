using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NQuery.Authoring.Wpf
{
    public static class NQueryGlyphImageSource
    {
        private static readonly Dictionary<string, BitmapImage> Images = new Dictionary<string, BitmapImage>();

        private static string FromName(string iconName)
        {
            var assemblyName = typeof(NQueryGlyphImageSource).Assembly.GetName().Name;
            var uri = string.Format("pack://application:,,,/{0};component/Images/{1}.png", assemblyName, iconName);
            return uri;
        }

        private static string FromGlyph(NQueryGlyph glyph)
        {
            switch (glyph)
            {
                case NQueryGlyph.AmbiguousName:
                    return FromName("AmbiguousName");
                case NQueryGlyph.Keyword:
                    return FromName("Keyword");
                case NQueryGlyph.Variable:
                    return FromName("Variable");
                case NQueryGlyph.Relation:
                    return FromName("Relation");
                case NQueryGlyph.Table:
                    return FromName("Table");
                case NQueryGlyph.TableInstance:
                    return FromName("TableInstance");
                case NQueryGlyph.Aggregate:
                    return FromName("Aggregate");
                case NQueryGlyph.Column:
                    return FromName("Column");
                case NQueryGlyph.Function:
                    return FromName("Function");
                case NQueryGlyph.Method:
                    return FromName("Method");
                case NQueryGlyph.Property:
                    return FromName("Property");
                case NQueryGlyph.Type:
                    return FromName("Type");
                default:
                    throw new ArgumentOutOfRangeException("glyph");
            }
        }

        public static ImageSource Get(NQueryGlyph glyph)
        {
            var imageUri = FromGlyph(glyph);
            if (imageUri == null)
                return null;

            BitmapImage result;
            if (!Images.TryGetValue(imageUri, out result))
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