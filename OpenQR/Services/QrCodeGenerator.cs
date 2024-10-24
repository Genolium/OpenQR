using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Text.RegularExpressions;
using OpenQR.Models;

namespace OpenQR.Services
{
    // Класс для генерации изображений QR-кода.
    internal class QrCodeGenerator
    {
        // Регулярное выражение для проверки URL.
        private static readonly Regex UrlRegex = new Regex(@"^[a-zA-Z]+://", RegexOptions.Compiled);

        // Генерирует изображение QR-кода на основе данных qr.
        public static Bitmap GenerateQRCodeImage(IQR_CodeData qr, int moduleSize = 10, float cornerRadiusPercent = 25, bool unifiedFinders = true)
        {
            // Преобразование цветов из строк в объекты Color.
            Color startColor = ColorTranslator.FromHtml(qr.ForegroundColor_Top);
            Color endColor = ColorTranslator.FromHtml(qr.ForegroundColor_Bottom);
            Color backgroundColor = ColorTranslator.FromHtml(qr.BackgroundColor);

            // Вычисление размера границы.
            int borderSize = 2 * moduleSize;

            // Получение содержимого QR-кода.
            string content = "";
            // Если тип данных - ссылка, то добавляем протокол (если он не указан).
            if (typeof(QR_Link).Name == qr.GetType().Name)
            {
                if (!UrlRegex.IsMatch(qr.Content))
                {
                    content = ((QR_Link)qr).Protocol + qr.Content;
                }
                else
                    content = qr.Content;
            }
            else
                content = qr.Content;

            // Вычисление размера данных в байтах.
            int dataSize = Encoding.UTF8.GetByteCount(content);

            // Выбор уровня коррекции ошибок в зависимости от размера данных.
            QrCode.Ecc eccLevel;

            if (qr.Logo == null)
            {
                eccLevel = dataSize < 100 ? QrCode.Ecc.LOW :
                                  dataSize < 500 ? QrCode.Ecc.MEDIUM :
                                  dataSize < 1000 ? QrCode.Ecc.QUARTILE :
                                  QrCode.Ecc.HIGH;
            }
            else
            {
                eccLevel = dataSize < 100 ? QrCode.Ecc.MEDIUM :
                                  dataSize < 500 ? QrCode.Ecc.QUARTILE : QrCode.Ecc.HIGH;
            }

            // Кодирование текста в QR-код.
            QrCode qrCode = QrCode.EncodeText(content, eccLevel);

            // Вычисление размера QR-кода и изображения.
            int qrCodeSize = qrCode.Size * moduleSize;
            int imageSize = qrCodeSize + 2 * borderSize;

            // Создание изображения.
            Bitmap qrCodeImage = new Bitmap(imageSize, imageSize);

            // Инициализация графического объекта.
            using (Graphics graphics = Graphics.FromImage(qrCodeImage))
            {
                // Заливка фона.
                graphics.Clear(backgroundColor);

                // Создание градиентной кисти.
                LinearGradientBrush gradientBrush;
                if (qr.FromLeftToRightCorner)
                {
                    gradientBrush = new LinearGradientBrush(
                        new Point(borderSize, borderSize),
                        new Point(qrCodeSize + borderSize, qrCodeSize + borderSize),
                        startColor, endColor);
                }
                else
                {
                    gradientBrush = new LinearGradientBrush(
                        new Point(qrCodeSize + borderSize, borderSize),
                        new Point(borderSize, qrCodeSize + borderSize),
                        startColor, endColor);
                }

                // Вычисление радиуса скругления углов.
                float cornerRadius = moduleSize * (cornerRadiusPercent / 100f);

                // Матрица для отслеживания уже отрисованных искателей.
                bool[,] drawnFinders = new bool[qrCode.Size, qrCode.Size];

                // Отрисовка объединенных искателей (если включено).
                if (unifiedFinders)
                {
                    DrawFinder(graphics, gradientBrush, borderSize, borderSize, moduleSize, qr.ModuleShape, backgroundColor);
                    DrawFinder(graphics, gradientBrush, borderSize + (qrCode.Size - 7) * moduleSize, borderSize, moduleSize, qr.ModuleShape, backgroundColor);
                    DrawFinder(graphics, gradientBrush, borderSize, borderSize + (qrCode.Size - 7) * moduleSize, moduleSize, qr.ModuleShape, backgroundColor);

                    // Помечаем искатели как отрисованные.
                    for (int i = 0; i < 7; i++)
                    {
                        for (int j = 0; j < 7; j++)
                        {
                            drawnFinders[i, j] = true;
                            drawnFinders[qrCode.Size - 7 + i, j] = true;
                            drawnFinders[i, qrCode.Size - 7 + j] = true;
                        }
                    }
                }

                // Отрисовка модулей QR-кода.
                for (int y = 0; y < qrCode.Size; y++)
                {
                    for (int x = 0; x < qrCode.Size; x++)
                    {
                        // Пропускаем уже отрисованные искатели и пустые модули.
                        if (!drawnFinders[y, x] && qrCode.Modules[y][x])
                        {
                            // Вычисление позиции модуля.
                            float xPos = x * moduleSize + borderSize;
                            float yPos = y * moduleSize + borderSize;

                            // Отрисовка модуля в зависимости от выбранной формы.
                            switch (qr.ModuleShape)
                            {
                                case ShapeType.Square:
                                    DrawSquare(graphics, gradientBrush, xPos, yPos, moduleSize);
                                    break;
                                case ShapeType.Circle:
                                    DrawCircle(graphics, gradientBrush, xPos, yPos, moduleSize);
                                    break;
                                case ShapeType.RoundedSquare:
                                    DrawRoundedSquare(graphics, gradientBrush, xPos, yPos, moduleSize, cornerRadius);
                                    break;
                                default:
                                    throw new ArgumentException($"Unsupported shape type: {qr.ModuleShape}");
                            }
                        }
                    }
                }

                // Отрисовка логотипа (если он задан).
                if (qr.Logo != null && qr.Logo.Width > 0 && qr.Logo.Height > 0)
                {
                    // Вычисление размера и позиции логотипа.
                    int logoSize = qrCodeSize / 4;
                    int logoPosition = (imageSize - logoSize) / 2;

                    // Создание пути для скругленных углов логотипа.
                    GraphicsPath path = new GraphicsPath();
                    path.AddArc(logoPosition, logoPosition, logoSize, logoSize, 180, 90);
                    path.AddArc(logoPosition + logoSize - 10, logoPosition, 10, 10, 270, 90);
                    path.AddArc(logoPosition + logoSize - 10, logoPosition + logoSize - 10, 10, 10, 0, 90);
                    path.AddArc(logoPosition, logoPosition + logoSize - 10, 10, 10, 90, 90);
                    path.CloseFigure();

                    // Отрисовка логотипа.
                    graphics.DrawImage(qr.Logo, logoPosition + 5, logoPosition + 5, logoSize - 10, logoSize - 10);
                }
            }

            // Возвращение готового изображения.
            return qrCodeImage;
        }

        // Отрисовывает квадратный модуль.
        private static void DrawSquare(Graphics graphics, Brush brush, float x, float y, float size)
        {
            graphics.FillRectangle(brush, x, y, size, size);
        }

        // Отрисовывает круглый модуль.
        private static void DrawCircle(Graphics graphics, Brush brush, float x, float y, float size)
        {
            graphics.FillEllipse(brush, x, y, size, size);
        }

        // Отрисовывает квадратный модуль со скругленными углами.
        private static void DrawRoundedSquare(Graphics graphics, Brush brush, float x, float y, float size, float cornerRadius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(x, y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(x + size - cornerRadius * 2, y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(x + size - cornerRadius * 2, y + size - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(x, y + size - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }

        // Отрисовывает искатель QR-кода.
        private static void DrawFinder(Graphics graphics, Brush brush, float x, float y, float size, ShapeType shapeType, Color backgroundColor)
        {
            // Вычисление размеров искателя.
            float outerSize = size * 7;
            float middleSize = size * 5;
            float innerSize = size * 3;

            // Отрисовка искателя в зависимости от выбранной формы модулей.
            switch (shapeType)
            {
                case ShapeType.Square:
                    graphics.FillRectangle(brush, x, y, outerSize, outerSize);
                    graphics.FillRectangle(new SolidBrush(backgroundColor), x + size, y + size, middleSize, middleSize);
                    graphics.FillRectangle(brush, x + 2 * size, y + 2 * size, innerSize, innerSize);
                    break;
                case ShapeType.Circle:
                    graphics.FillEllipse(brush, x, y, outerSize, outerSize);
                    graphics.FillEllipse(new SolidBrush(backgroundColor), x + size, y + size, middleSize, middleSize);
                    graphics.FillEllipse(brush, x + 2 * size, y + 2 * size, innerSize, innerSize);
                    break;
                case ShapeType.RoundedSquare:
                    float cornerRadius = size;
                    DrawRoundedRectangle(graphics, brush, x, y, outerSize, outerSize, cornerRadius);
                    DrawRoundedRectangle(graphics, new SolidBrush(backgroundColor), x + size, y + size, middleSize, middleSize, cornerRadius);
                    DrawRoundedRectangle(graphics, brush, x + 2 * size, y + 2 * size, innerSize, innerSize, cornerRadius);
                    break;
            }
        }

        // Отрисовывает прямоугольник со скругленными углами.
        private static void DrawRoundedRectangle(Graphics graphics, Brush brush, float x, float y, float width, float height, float cornerRadius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddArc(x, y, cornerRadius * 2, cornerRadius * 2, 180, 90);
                path.AddArc(x + width - cornerRadius * 2, y, cornerRadius * 2, cornerRadius * 2, 270, 90);
                path.AddArc(x + width - cornerRadius * 2, y + height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
                path.AddArc(x, y + height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }

        // Вычисляет размер модуля на основе размера изображения и данных QR-кода.
        public static int CalculateModuleSize(IQR_CodeData qr, int imageSize)
        {
            // Получение содержимого QR-кода.
            string content = "";
            // Если тип данных - ссылка, то добавляем протокол (если он не указан).
            if (typeof(QR_Link).Name == qr.GetType().Name)
            {
                if (!UrlRegex.IsMatch(qr.Content))
                {
                    content = ((QR_Link)qr).Protocol + qr.Content;
                }
                else
                    content = qr.Content;
            }
            else
                content = qr.Content;

            // Вычисление размера данных в байтах.
            int dataSize = Encoding.UTF8.GetByteCount(content);

            // Выбор уровня коррекции ошибок в зависимости от размера данных.
            QrCode.Ecc eccLevel;

            if (qr.Logo == null)
            {
                eccLevel = dataSize < 100 ? QrCode.Ecc.LOW :
                                  dataSize < 500 ? QrCode.Ecc.MEDIUM :
                                  dataSize < 1000 ? QrCode.Ecc.QUARTILE :
                                  QrCode.Ecc.HIGH;
            }
            else
            {
                eccLevel = dataSize < 100 ? QrCode.Ecc.MEDIUM :
                                  dataSize < 500 ? QrCode.Ecc.QUARTILE : QrCode.Ecc.HIGH;
            }

            // Кодирование текста в QR-код.
            QrCode qrCode = QrCode.EncodeText(content, eccLevel);

            // Вычисление размера модуля.
            int moduleSize = imageSize / (qrCode.Size + 4);

            return moduleSize;
        }
    }
}