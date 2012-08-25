using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NQuery.Language.VSEditor
{
    [Export(typeof(INQueryGlyphService))]
    internal sealed class NQueryGlyphService : INQueryGlyphService
    {
        private readonly Dictionary<string, BitmapImage> _images = new Dictionary<string, BitmapImage>();

        private static string FromName(string iconName)
        {
            var assemblyName = typeof(NQueryGlyphService).Assembly.GetName().Name;
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
                case NQueryGlyph.Parameter:
                    return FromName("Parameter");
                case NQueryGlyph.Relation:
                    return FromName("Relation");
                case NQueryGlyph.ScalarValueFunction:
                    return FromName("ScalarValueFunction");
                case NQueryGlyph.StoredProcedure:
                    return FromName("StoredProcedure");
                case NQueryGlyph.Table:
                    return FromName("Table");
                case NQueryGlyph.TableRef:
                    return FromName("TableRef");
                case NQueryGlyph.TableValueFunction:
                    return FromName("TableValueFunction");
                case NQueryGlyph.Aggregate:
                    return FromName("Aggregate");
                case NQueryGlyph.Column:
                    return FromName("Column");
                case NQueryGlyph.Function:
                    return FromName("Function");
                default:
                    throw new ArgumentOutOfRangeException("glyph");
            }
        }

        public ImageSource GetGlyph(NQueryGlyph glyph)
        {
            var imageUri = FromGlyph(glyph);
            if (imageUri == null)
                return null;

            BitmapImage result;
            if (!_images.TryGetValue(imageUri, out result))
            {
                result = new BitmapImage(new Uri(imageUri));
                _images.Add(imageUri, result);
            }

            return result;
        }
    }
}