using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ROB
{
    internal class Painter
    {
        public static void PaintTextImage(string text, MemoryStream ms)
        {
            using (var paint = new SKPaint())
            {
                paint.Typeface = SKTypeface.FromFamilyName("Consolas", SKFontStyle.Normal);
                paint.TextSize = 16f;
                paint.TextAlign = SKTextAlign.Left;
                paint.Color = SKColors.White;
                paint.IsAntialias = false;

                var lines = text.Split(Environment.NewLine).Select(l => l.Trim());

                text = String.Join(Environment.NewLine, lines);

                var bounds = SKRect.Empty;

                var width = lines.Max(l => paint.MeasureText(l.Trim(), ref bounds));
                var textHeight = paint.TextSize + 2;
                var height = paint.BreakText(lines.First(), width);

                var skBounds = new SKRect(0, 0, width, textHeight * lines.Count());

                var bitmap = new SKBitmap((int)width, (int)(textHeight * lines.Count()));

                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColor.Parse("#36393E"));

                    float y = textHeight;

                    foreach (var line in lines)
                    {
                        canvas.DrawText(line, 0, y, paint);
                        y += paint.TextSize + 2;
                    }

                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                        {
                            data.SaveTo(ms);
                        }
                    }
                }
            }
        }
    }
}
