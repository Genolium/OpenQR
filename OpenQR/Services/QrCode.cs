using System.Text;

namespace OpenQR.Services
{
    // Перечисление для определения формы модуля QR-кода.
    public enum ShapeType
    {
        // Квадрат
        Square,
        // Круг
        Circle,
        // Квадрат со скругленными углами
        RoundedSquare
    }

    // Класс для представления QR-кода.
    public class QrCode
    {
        // Свойства для доступа к основным параметрам QR-кода
        // Уровень коррекции ошибок QR-кода.
        public Ecc ErrorCorrectionLevel { get { return errorCorrectionLevel; } }
        // Размер QR-кода в модулях (пикселях).
        public int Size { get { return size; } }
        // Версия QR-кода (от 1 до 40).
        public int Version { get { return version; } }
        // Номер маски, примененной к QR-коду.
        public int Mask { get { return mask; } }
        // Двумерный массив логических значений, представляющий модули QR-кода.
        public List<List<bool>> Modules { get { return modules; } }

        // ---- Режимы кодирования QR-кода ----
        public enum Mode
        {
            // Числовой режим
            NUMERIC = 0x1,
            // Буквенно-цифровой режим
            ALPHANUMERIC = 0x2,
            // Байтовый режим
            BYTE = 0x4,
            // Режим кандзи
            KANJI = 0x8,
            // Режим расширенной интерпретации канала
            ECI = 0x7
        }

        // ---- Уровни коррекции ошибок QR-кода ----
        public enum Ecc
        {
            // Низкий
            LOW,
            // Средний
            MEDIUM,
            // Четвертной
            QUARTILE,
            // Высокий
            HIGH
        }

        // ---- Сегмент QR-кода ----
        public class QrSegment
        {
            // Режим кодирования
            public Mode mode;
            // Количество символов
            public int numChars;
            // Закодированные данные
            public List<bool> data;

            // Конструктор сегмента
            public QrSegment(Mode md, int numCh, List<bool> dt)
            {
                mode = md;
                numChars = numCh;
                data = dt;
            }

            // Статические методы для создания сегментов

            // Создание байтового сегмента
            public static QrSegment MakeBytes(List<byte> data)
            {
                if (data.Count > int.MaxValue)
                    throw new ArgumentException("Данные слишком длинные");

                BitBuffer bb = new BitBuffer();
                foreach (byte b in data)
                    bb.AppendBits(b, 8);

                return new QrSegment(Mode.BYTE, data.Count, bb.GetData());
            }

            // Создание числового сегмента
            public static QrSegment MakeNumeric(string digits)
            {
                BitBuffer bb = new BitBuffer();
                int accumData = 0;
                int accumCount = 0;
                int charCount = 0;

                foreach (char c in digits)
                {
                    if (c < '0' || c > '9')
                        throw new ArgumentException("Строка содержит нечисловые символы");

                    accumData = accumData * 10 + (c - '0');
                    accumCount++;

                    if (accumCount == 3)
                    {
                        bb.AppendBits(accumData, 10);
                        accumData = 0;
                        accumCount = 0;
                    }
                }

                if (accumCount > 0)
                    bb.AppendBits(accumData, accumCount * 3 + 1);

                return new QrSegment(Mode.NUMERIC, charCount, bb.GetData());
            }

            // Создание буквенно-цифрового сегмента
            public static QrSegment MakeAlphanumeric(string text)
            {
                BitBuffer bb = new BitBuffer();
                int accumData = 0;
                int accumCount = 0;
                int charCount = 0;

                foreach (char c in text)
                {
                    int index = AlphanumericCharset.IndexOf(c);
                    if (index == -1)
                        throw new ArgumentException("Строка содержит символы, не кодируемые в буквенно-цифровом режиме");

                    accumData = accumData * 45 + index;
                    accumCount++;

                    if (accumCount == 2)
                    {
                        bb.AppendBits(accumData, 11);
                        accumData = 0;
                        accumCount = 0;
                    }
                }

                if (accumCount > 0)
                    bb.AppendBits(accumData, 6);

                return new QrSegment(Mode.ALPHANUMERIC, charCount, bb.GetData());
            }

            // Создание сегментов из текста
            public static List<QrSegment> MakeSegments(string text)
            {
                List<QrSegment> result = new List<QrSegment>();

                if (string.IsNullOrEmpty(text))
                    return result;

                if (IsNumeric(text))
                    result.Add(MakeNumeric(text));
                else if (IsAlphanumeric(text))
                    result.Add(MakeAlphanumeric(text));
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(text);
                    result.Add(MakeBytes(bytes.ToList()));
                }

                return result;
            }

            // Создание ECI сегмента
            public static QrSegment MakeEci(int assignVal)
            {
                BitBuffer bb = new BitBuffer();

                if (assignVal < 0)
                    throw new ArgumentException("Значение назначения ECI вне диапазона");
                else if (assignVal < 1 << 7)
                    bb.AppendBits(assignVal, 8);
                else if (assignVal < 1 << 14)
                {
                    bb.AppendBits(2, 2);
                    bb.AppendBits(assignVal, 14);
                }
                else if (assignVal < 1000000)
                {
                    bb.AppendBits(6, 3);
                    bb.AppendBits(assignVal, 21);
                }
                else
                    throw new ArgumentException("Значение назначения ECI вне диапазона");

                return new QrSegment(Mode.ECI, 0, bb.GetData());
            }

            // Получение общего количества битов для списка сегментов
            public static int GetTotalBits(List<QrSegment> segs, int version)
            {
                int result = 0;
                foreach (QrSegment seg in segs)
                {
                    int ccbits = NumCharCountBits(seg.mode, version);
                    if (seg.numChars >= 1L << ccbits)
                        return -1; // Длина сегмента не помещается в битовую ширину поля

                    if (4 + ccbits > int.MaxValue - result)
                        return -1; // Сумма переполнится

                    result += 4 + ccbits;

                    if (seg.data.Count > int.MaxValue - result)
                        return -1; // Сумма переполнится

                    result += seg.data.Count;
                }
                return result;
            }

            // Проверка, является ли текст числовым
            public static bool IsNumeric(string text)
            {
                foreach (char c in text)
                {
                    if (c < '0' || c > '9')
                        return false;
                }
                return true;
            }

            // Проверка, является ли текст буквенно-цифровым
            public static bool IsAlphanumeric(string text)
            {
                foreach (char c in text)
                {
                    if (AlphanumericCharset.IndexOf(c) == -1)
                        return false;
                }
                return true;
            }

            // Helper methods
            // Возвращает количество бит, необходимых для кодирования количества символов в сегменте.
            public static int NumCharCountBits(Mode mode, int version)
            {
                int[] numBitsCharCount;
                switch (mode)
                {
                    case Mode.NUMERIC:
                        numBitsCharCount = new int[] { 10, 12, 14 };
                        break;
                    case Mode.ALPHANUMERIC:
                        numBitsCharCount = new int[] { 9, 11, 13 };
                        break;
                    case Mode.BYTE:
                        numBitsCharCount = new int[] { 8, 16, 16 };
                        break;
                    case Mode.KANJI:
                        numBitsCharCount = new int[] { 8, 10, 12 };
                        break;
                    case Mode.ECI:
                        // ECI mode doesn't encode character count
                        numBitsCharCount = new int[] { 0, 0, 0 };
                        break;
                    default:
                        throw new ArgumentException("Invalid mode");
                }

                return numBitsCharCount[(version + 7) / 17];
            }


            // Строка допустимых символов для буквенно-цифрового режима.
            private static readonly string AlphanumericCharset = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ $%*+-./:";

            public Mode GetMode() => mode;
            public int GetNumChars() => numChars;
            public List<bool> GetData() => data;
        }

        // ---- BitBuffer ----
        // Класс для работы с буфером битов.
        public class BitBuffer
        {
            // Список битов.
            private List<bool> data;

            // Конструктор
            public BitBuffer()
            {
                data = new List<bool>();
            }

            // Добавляет указанное количество бит из значения в буфер.
            public void AppendBits(int val, int len)
            {
                if (len < 0 || len > 31 || val >> len != 0)
                    throw new ArgumentException("Value out of range");

                for (int i = len - 1; i >= 0; i--)
                    data.Add((val >> i & 1) != 0);
            }

            // Возвращает список битов.
            public List<bool> GetData() => data;

            // Возвращает размер буфера в битах.
            public int Size => data.Count;

            // Индексатор для доступа к битам по индексу.
            public bool this[int index] => data[index];

            // Вставляет коллекцию битов в указанную позицию.
            public void InsertRange(int index, IEnumerable<bool> collection)
            {
                data.InsertRange(index, collection);
            }
        }

        // ---- QR Code Constants ----
        // Минимальная версия QR-кода.
        public const int MIN_VERSION = 1;
        // Максимальная версия QR-кода.
        public const int MAX_VERSION = 40;

        // Константы штрафов за различные паттерны в QR-коде, 
        // используемые для выбора маски с наименьшим штрафом.
        private const int PENALTY_N1 = 3;
        private const int PENALTY_N2 = 3;
        private const int PENALTY_N3 = 40;
        private const int PENALTY_N4 = 10;

        // Количество кодовых слов ECC на блок для каждой версии и уровня коррекции ошибок.
        private static readonly int[][] ECC_CODEWORDS_PER_BLOCK = new int[][] {
            new int[] {-1,  7, 10, 15, 20, 26, 18, 20, 24, 30, 18, 20, 24, 26, 30, 22, 24, 28, 30, 28, 28, 28, 28, 30, 30, 26, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30},  // Low
            new int[] {-1, 10, 16, 26, 18, 24, 16, 18, 22, 22, 26, 30, 22, 22, 24, 24, 28, 28, 26, 26, 26, 26, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28, 28},  // Medium
            new int[] {-1, 13, 22, 18, 26, 18, 24, 18, 22, 20, 24, 28, 26, 24, 20, 30, 24, 28, 28, 26, 30, 28, 30, 30, 30, 30, 28, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30},  // Quartile
            new int[] {-1, 17, 28, 22, 16, 22, 28, 26, 26, 24, 28, 24, 28, 22, 24, 24, 30, 28, 28, 26, 28, 30, 24, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30}   // High
        };

        // Количество блоков коррекции ошибок для каждой версии и уровня коррекции ошибок.
        private static readonly int[][] NUM_ERROR_CORRECTION_BLOCKS = new int[][] {
            new int[] {-1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 4,  4,  4,  4,  4,  6,  6,  6,  6,  7,  8,  8,  9,  9, 10, 12, 12, 12, 13, 14, 15, 16, 17, 18, 19, 19, 20, 21, 22, 24, 25},  // Low
            new int[] {-1, 1, 1, 1, 2, 2, 4, 4, 4, 5, 5,  5,  8,  9,  9, 10, 10, 11, 13, 14, 16, 17, 17, 18, 20, 21, 23, 25, 26, 28, 29, 31, 33, 35, 37, 38, 40, 43, 45, 47, 49},  // Medium
            new int[] {-1, 1, 1, 2, 2, 4, 4, 6, 6, 8, 8,  8, 10, 12, 16, 12, 17, 16, 18, 21, 20, 23, 23, 25, 27, 29, 34, 34, 35, 38, 40, 43, 45, 48, 51, 53, 56, 59, 62, 65, 68},  // Quartile
            new int[] {-1, 1, 1, 2, 4, 4, 4, 5, 6, 8, 8, 11, 11, 16, 16, 18, 16, 19, 21, 25, 25, 25, 34, 30, 32, 35, 37, 40, 42, 45, 48, 51, 54, 57, 60, 63, 66, 70, 74, 77, 81}   // High
        };

        // Уровень коррекции ошибок.
        private Ecc errorCorrectionLevel;
        // Размер QR-кода в модулях.
        private int size;
        // Матрица модулей QR-кода.
        private List<List<bool>> modules;
        // Версия QR-кода.
        private int version;
        // Номер маски.
        private int mask;

        // ---- Constructor ----
        // Конструктор, создающий QR-код из заданных параметров.
        public QrCode(int ver, Ecc ecl, List<byte> dataCodewords, int msk)
        {
            // Инициализация полей
            version = ver;
            errorCorrectionLevel = ecl;

            // Проверка корректности версии
            if (ver < MIN_VERSION || ver > MAX_VERSION)
                throw new ArgumentException("Version value out of range");

            // Проверка корректности маски
            if (msk < -1 || msk > 7)
                throw new ArgumentException("Mask value out of range");

            // Вычисление размера QR-кода
            size = ver * 4 + 17;
            // Инициализация матрицы модулей
            modules = new List<List<bool>>(size);
            for (int i = 0; i < size; i++)
            {
                modules.Add(new List<bool>(size));
                for (int j = 0; j < size; j++)
                    // Изначально все модули светлые (false)
                    modules[i].Add(false);
            }

            // Создание матрицы для отслеживания функциональных модулей
            List<List<bool>> isFunction = new List<List<bool>>(size);
            for (int i = 0; i < size; i++)
            {
                isFunction.Add(new List<bool>(size));
                for (int j = 0; j < size; j++)
                    isFunction[i].Add(false);
            }

            // Отрисовка функциональных паттернов
            DrawFunctionPatterns(modules, isFunction);
            // Добавление кодов коррекции ошибок и перемежение данных
            List<byte> allCodewords = AddEccAndInterleave(dataCodewords);
            // Отрисовка кодовых слов
            DrawCodewords(modules, isFunction, allCodewords);

            // Применение маски
            if (msk == -1)
            {
                long minPenalty = long.MaxValue;
                // Поиск маски с наименьшим штрафом
                for (int i = 0; i < 8; i++)
                {
                    ApplyMask(modules, isFunction, i);
                    DrawFormatBits(modules, isFunction, i);
                    long penalty = GetPenaltyScore(modules);

                    if (penalty < minPenalty)
                    {
                        msk = i;
                        minPenalty = penalty;
                    }

                    // Сброс маски
                    ApplyMask(modules, isFunction, i);
                }
            }

            // Применение выбранной маски
            mask = msk;
            ApplyMask(modules, isFunction, msk);
            // Отрисовка финальных битов формата
            DrawFormatBits(modules, isFunction, msk);
        }


        // ---- Public Static Factory Methods ----
        // Создает QR-код из текста, используя заданный уровень коррекции ошибок.
        public static QrCode EncodeText(string text, Ecc ecl)
        {
            List<QrSegment> segs = QrSegment.MakeSegments(text);
            return EncodeSegments(segs, ecl);
        }

        // Создает QR-код из бинарных данных, используя заданный уровень коррекции ошибок.
        public static QrCode EncodeBinary(List<byte> data, Ecc ecl)
        {
            List<QrSegment> segs = new List<QrSegment> { QrSegment.MakeBytes(data) };
            return EncodeSegments(segs, ecl);
        }

        // Создает QR-код из списка сегментов, используя заданный уровень коррекции ошибок.
        public static QrCode EncodeSegments(List<QrSegment> segs, Ecc ecl,
            int minVersion = MIN_VERSION, int maxVersion = MAX_VERSION, int mask = -1, bool boostEcl = true)
        {
            // Проверка корректности параметров
            if (!(MIN_VERSION <= minVersion && minVersion <= maxVersion && maxVersion <= MAX_VERSION) || mask < -1 || mask > 7)
                throw new ArgumentException("Invalid value");

            // Поиск минимальной версии, подходящей для кодирования данных
            int version, dataUsedBits;
            for (version = minVersion; ; version++)
            {
                // Вычисление емкости данных для текущей версии и уровня коррекции ошибок
                int dataCapacityBits = GetNumDataCodewords(version, ecl) * 8;
                // Вычисление количества бит, необходимых для кодирования данных
                dataUsedBits = QrSegment.GetTotalBits(segs, version);

                // Если данные помещаются в текущую версию, то выходим из цикла
                if (dataUsedBits != -1 && dataUsedBits <= dataCapacityBits)
                    break;

                // Если достигнута максимальная версия, то выбрасываем исключение
                if (version >= maxVersion)
                {
                    string errorMessage;
                    // Формирование сообщения об ошибке
                    if (dataUsedBits == -1)
                        errorMessage = "Segment too long";
                    else
                        errorMessage = $"Data length = {dataUsedBits} bits, Max capacity = {dataCapacityBits} bits";
                    // Выброс исключения
                    throw new ArgumentException(errorMessage);
                }
            }

            // Попытка увеличить уровень коррекции ошибок, если данные все еще помещаются
            if (boostEcl)
            {
                foreach (Ecc newEcl in new[] { Ecc.MEDIUM, Ecc.QUARTILE, Ecc.HIGH })
                {
                    if (dataUsedBits <= GetNumDataCodewords(version, newEcl) * 8)
                        ecl = newEcl;
                }
            }

            // Создание битового буфера и добавление в него всех сегментов
            BitBuffer bb = new BitBuffer();
            foreach (QrSegment seg in segs)
            {
                bb.AppendBits((int)seg.GetMode(), 4);
                bb.AppendBits(seg.GetNumChars(), QrSegment.NumCharCountBits(seg.mode, version));
                bb.InsertRange(bb.Size, seg.GetData());
            }

            // Добавление завершающих битов и выравнивание по байтам
            int dataCapacityBits2 = GetNumDataCodewords(version, ecl) * 8;
            bb.AppendBits(0, Math.Min(4, dataCapacityBits2 - bb.Size));
            bb.AppendBits(0, (8 - bb.Size % 8) % 8);

            // Заполнение буфера чередующимися байтами до достижения емкости
            for (byte padByte = 0xEC; bb.Size < dataCapacityBits2; padByte ^= 0xEC ^ 0x11)
                bb.AppendBits(padByte, 8);

            // Упаковка битов в байты в формате big-endian
            List<byte> dataCodewords = new List<byte>(new byte[bb.Size / 8]);
            for (int i = 0; i < bb.Size; i++)
            {
                int byteIndex = i >> 3;
                int bitPosition = 7 - (i & 7);
                dataCodewords[byteIndex] |= (byte)((bb[i] ? 1 : 0) << bitPosition);
            }

            // Создание QR-кода
            return new QrCode(version, ecl, dataCodewords, mask);
        }

        // ---- Private Helper Methods ----
        // Возвращает биты формата для заданного уровня коррекции ошибок.
        private static int GetFormatBits(Ecc ecl)
        {
            switch (ecl)
            {
                case Ecc.LOW: return 1;
                case Ecc.MEDIUM: return 0;
                case Ecc.QUARTILE: return 3;
                case Ecc.HIGH: return 2;
                default: throw new ArgumentException("Unreachable");
            }
        }

        // Отрисовывает функциональные паттерны QR-кода: искатели, разделители,
        // тайминговые паттерны и паттерны выравнивания.
        private void DrawFunctionPatterns(List<List<bool>> modules, List<List<bool>> isFunction)
        {
            // Отрисовка горизонтальных и вертикальных тайминговых паттернов
            for (int i = 0; i < size; i++)
            {
                SetFunctionModule(modules, isFunction, 6, i, i % 2 == 0);
                SetFunctionModule(modules, isFunction, i, 6, i % 2 == 0);
            }

            // Отрисовка 3 искателей (все углы, кроме правого нижнего)
            DrawFinderPattern(modules, isFunction, 3, 3);
            DrawFinderPattern(modules, isFunction, size - 4, 3);
            DrawFinderPattern(modules, isFunction, 3, size - 4);

            // Отрисовка паттернов выравнивания
            List<int> alignPatPos = GetAlignmentPatternPositions();
            int numAlign = alignPatPos.Count;
            for (int i = 0; i < numAlign; i++)
            {
                for (int j = 0; j < numAlign; j++)
                {
                    // Не рисовать на углах искателей
                    if (!(i == 0 && j == 0 || i == 0 && j == numAlign - 1 || i == numAlign - 1 && j == 0))
                        DrawAlignmentPattern(modules, isFunction, alignPatPos[i], alignPatPos[j]);
                }
            }

            // Отрисовка данных конфигурации
            // (фиктивное значение маски; будет перезаписано позже)
            DrawFormatBits(modules, isFunction, 0);
            // Отрисовка версии (если необходимо)
            DrawVersion(modules, isFunction);
        }

        // Отрисовывает биты формата (уровень коррекции ошибок и маска) в QR-коде.
        private void DrawFormatBits(List<List<bool>> modules, List<List<bool>> isFunction, int msk)
        {
            // Вычисление кода коррекции ошибок и упаковка битов
            int data = GetFormatBits(errorCorrectionLevel) << 3 | msk;
            int rem = data;
            for (int i = 0; i < 10; i++)
                rem = rem << 1 ^ (rem >> 9) * 0x537;
            int bits = (data << 10 | rem) ^ 0x5412;

            // Отрисовка первой копии
            for (int i = 0; i <= 5; i++)
                SetFunctionModule(modules, isFunction, 8, i, GetBit(bits, i));
            SetFunctionModule(modules, isFunction, 8, 7, GetBit(bits, 6));
            SetFunctionModule(modules, isFunction, 8, 8, GetBit(bits, 7));
            SetFunctionModule(modules, isFunction, 7, 8, GetBit(bits, 8));
            for (int i = 9; i < 15; i++)
                SetFunctionModule(modules, isFunction, 14 - i, 8, GetBit(bits, i));

            // Отрисовка второй копии
            for (int i = 0; i < 8; i++)
                SetFunctionModule(modules, isFunction, size - 1 - i, 8, GetBit(bits, i));
            for (int i = 8; i < 15; i++)
                SetFunctionModule(modules, isFunction, 8, size - 15 + i, GetBit(bits, i));
            // Всегда темный модуль
            SetFunctionModule(modules, isFunction, 8, size - 8, true);
        }


        // Отрисовывает номер версии QR-кода (для версий 7 и выше).
        private void DrawVersion(List<List<bool>> modules, List<List<bool>> isFunction)
        {
            if (version < 7)
                return;

            // Вычисление кода коррекции ошибок и упаковка битов
            int rem = version;
            for (int i = 0; i < 12; i++)
                rem = rem << 1 ^ (rem >> 11) * 0x1F25;
            long bits = version << 12 | rem;

            // Отрисовка двух копий
            for (int i = 0; i < 18; i++)
            {
                bool bit = GetBit(bits, i);
                int a = size - 11 + i % 3;
                int b = i / 3;
                SetFunctionModule(modules, isFunction, a, b, bit);
                SetFunctionModule(modules, isFunction, b, a, bit);
            }
        }

        // Отрисовывает искатель (паттерн в углу QR-кода).
        private void DrawFinderPattern(List<List<bool>> modules, List<List<bool>> isFunction, int x, int y)
        {
            for (int dy = -4; dy <= 4; dy++)
            {
                for (int dx = -4; dx <= 4; dx++)
                {
                    int dist = Math.Max(Math.Abs(dx), Math.Abs(dy));
                    int xx = x + dx, yy = y + dy;
                    if (0 <= xx && xx < size && 0 <= yy && yy < size)
                        SetFunctionModule(modules, isFunction, xx, yy, dist != 2 && dist != 4);
                }
            }
        }

        // Отрисовывает паттерн выравнивания.
        private void DrawAlignmentPattern(List<List<bool>> modules, List<List<bool>> isFunction, int x, int y)
        {
            for (int dy = -2; dy <= 2; dy++)
            {
                for (int dx = -2; dx <= 2; dx++)
                    SetFunctionModule(modules, isFunction, x + dx, y + dy, Math.Max(Math.Abs(dx), Math.Abs(dy)) != 1);
            }
        }

        // Устанавливает значение функционального модуля.
        private void SetFunctionModule(List<List<bool>> modules, List<List<bool>> isFunction, int x, int y, bool isDark)
        {
            modules[y][x] = isDark;
            isFunction[y][x] = true;
        }

        // Возвращает значение модуля по координатам.
        private bool Module(List<List<bool>> modules, int x, int y)
        {
            return modules[y][x];
        }

        // Добавляет коды коррекции ошибок и перемежает данные.
        private List<byte> AddEccAndInterleave(List<byte> data)
        {
            // Проверка длины данных
            if (data.Count != GetNumDataCodewords(version, errorCorrectionLevel))
                throw new ArgumentException("Invalid argument");

            // Вычисление параметров
            int numBlocks = NUM_ERROR_CORRECTION_BLOCKS[(int)errorCorrectionLevel][version];
            int blockEccLen = ECC_CODEWORDS_PER_BLOCK[(int)errorCorrectionLevel][version];
            int rawCodewords = GetNumRawDataModules(version) / 8;
            int numShortBlocks = numBlocks - rawCodewords % numBlocks;
            int shortBlockLen = rawCodewords / numBlocks;

            // Разделение данных на блоки и добавление ECC к каждому блоку
            List<List<byte>> blocks = new List<List<byte>>();
            List<byte> rsDiv = ReedSolomonComputeDivisor(blockEccLen);

            for (int i = 0, k = 0; i < numBlocks; i++)
            {
                List<byte> dat = data.GetRange(k, shortBlockLen - blockEccLen + (i < numShortBlocks ? 0 : 1));
                k += dat.Count;
                List<byte> ecc = ReedSolomonComputeRemainder(dat, rsDiv);

                if (i < numShortBlocks)
                    dat.Add(0);
                dat.AddRange(ecc);
                blocks.Add(dat);
            }

            // Перемежение байтов из каждого блока в единую последовательность
            List<byte> result = new List<byte>();
            for (int i = 0; i < blocks[0].Count; i++)
            {
                for (int j = 0; j < blocks.Count; j++)
                {
                    // Пропуск байта заполнения в коротких блоках
                    if (i != shortBlockLen - blockEccLen || j >= numShortBlocks)
                        result.Add(blocks[j][i]);
                }
            }

            return result;
        }

        // Записывает кодовые слова в матрицу модулей, используя зигзагообразное сканирование.
        private void DrawCodewords(List<List<bool>> modules, List<List<bool>> isFunction, List<byte> data)
        {
            // Проверка длины данных
            if (data.Count != GetNumRawDataModules(version) / 8)
                throw new ArgumentException("Invalid argument");

            int i = 0; // Индекс бита в данных
            // Зигзагообразное сканирование
            for (int right = size - 1; right >= 1; right -= 2)
            {
                // Индекс правого столбца в каждой паре столбцов
                if (right == 6)
                    right = 5;

                for (int vert = 0; vert < size; vert++)
                {
                    // Вертикальный счетчик
                    for (int j = 0; j < 2; j++)
                    {
                        int x = right - j; // Действительная координата x
                        bool upward = (right + 1 & 2) == 0; // Направление сканирования (вверх или вниз)
                        int y = upward ? size - 1 - vert : vert; // Действительная координата y

                        // Если модуль не является функциональным и есть еще данные для записи
                        if (!isFunction[y][x] && i < data.Count * 8)
                        {
                            // Запись бита в модуль
                            modules[y][x] = GetBit(data[i >> 3], 7 - (i & 7));
                            i++;
                        }
                        // Оставшиеся биты (0-7), если таковые имеются, были установлены как 
                        // светлые (false) конструктором и не изменяются этим методом.
                    }
                }
            }
        }

        // Применяет выбранную маску к матрице модулей.
        private void ApplyMask(List<List<bool>> modules, List<List<bool>> isFunction, int msk)
        {
            // Проверка номера маски
            if (msk < 0 || msk > 7)
                throw new ArgumentException("Mask value out of range");

            // Проход по всем модулям QR-кода
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool invert;
                    // Выбор маски в зависимости от номера
                    switch (msk)
                    {
                        case 0: invert = (x + y) % 2 == 0; break;
                        case 1: invert = y % 2 == 0; break;
                        case 2: invert = x % 3 == 0; break;
                        case 3: invert = (x + y) % 3 == 0; break;
                        case 4: invert = (x / 3 + y / 2) % 2 == 0; break;
                        case 5: invert = x * y % 2 + x * y % 3 == 0; break;
                        case 6: invert = (x * y % 2 + x * y % 3) % 2 == 0; break;
                        case 7: invert = ((x + y) % 2 + x * y % 3) % 2 == 0; break;
                        default: throw new ArgumentException("Unreachable");
                    }

                    // Инвертирование модуля, если это не функциональный модуль 
                    // и условие маски выполняется
                    modules[y][x] ^= invert && !isFunction[y][x];
                }
            }
        }

        // Вычисляет штрафную оценку для матрицы модулей на основе различных паттернов.
        private long GetPenaltyScore(List<List<bool>> modules)
        {
            long result = 0;

            // Штраф за соседние модули в строке/столбце одинакового цвета
            // и паттерны, похожие на искатели
            for (int y = 0; y < size; y++)
            {
                bool runColor = false;
                int runX = 0;
                int[] runHistory = new int[7];
                for (int x = 0; x < size; x++)
                {
                    // Если цвет модуля совпадает с цветом предыдущего
                    if (Module(modules, x, y) == runColor)
                    {
                        runX++;
                        if (runX == 5)
                            result += PENALTY_N1; // Штраф за 5 подряд идущих модулей
                        else if (runX > 5)
                            result++; // Штраф за каждый последующий модуль
                    }
                    else
                    {
                        // Обновление истории паттернов
                        FinderPenaltyAddHistory(runX, runHistory);
                        if (!runColor)
                            // Штраф за паттерны, похожие на искатели
                            result += FinderPenaltyCountPatterns(runHistory) * PENALTY_N3;
                        runColor = Module(modules, x, y);
                        runX = 1;
                    }
                }
                // Обновление истории и подсчет штрафов в конце строки
                result += FinderPenaltyTerminateAndCount(runColor, runX, runHistory) * PENALTY_N3;
            }

            // Аналогично для столбцов
            for (int x = 0; x < size; x++)
            {
                bool runColor = false;
                int runY = 0;
                int[] runHistory = new int[7];
                for (int y = 0; y < size; y++)
                {
                    if (Module(modules, x, y) == runColor)
                    {
                        runY++;
                        if (runY == 5)
                            result += PENALTY_N1;
                        else if (runY > 5)
                            result++;
                    }
                    else
                    {
                        FinderPenaltyAddHistory(runY, runHistory);
                        if (!runColor)
                            result += FinderPenaltyCountPatterns(runHistory) * PENALTY_N3;
                        runColor = Module(modules, x, y);
                        runY = 1;
                    }
                }
                result += FinderPenaltyTerminateAndCount(runColor, runY, runHistory) * PENALTY_N3;
            }

            // Штраф за блоки 2x2 модулей одинакового цвета
            for (int y = 0; y < size - 1; y++)
            {
                for (int x = 0; x < size - 1; x++)
                {
                    bool color = Module(modules, x, y);
                    if (color == Module(modules, x + 1, y) &&
                        color == Module(modules, x, y + 1) &&
                        color == Module(modules, x + 1, y + 1))
                        result += PENALTY_N2;
                }
            }

            // Штраф за дисбаланс темных и светлых модулей
            int dark = 0;
            foreach (List<bool> row in modules)
            {
                foreach (bool color in row)
                {
                    if (color)
                        dark++;
                }
            }

            int total = size * size;
            // Вычисление минимального k, удовлетворяющего условию баланса
            int k = (int)((Math.Abs(dark * 20L - total * 10L) + total - 1) / total) - 1;
            result += k * PENALTY_N4;
            return result;
        }

        // Возвращает список позиций паттернов выравнивания для текущей версии QR-кода.
        private List<int> GetAlignmentPatternPositions()
        {
            // Для версии 1 паттерны выравнивания отсутствуют
            if (version == 1)
                return new List<int>();
            else
            {
                // Вычисление количества и шага паттернов выравнивания
                int numAlign = version / 7 + 2;
                int step = (version * 8 + numAlign * 3 + 5) / (numAlign * 4 - 4) * 2;
                // Формирование списка позиций
                List<int> result = new List<int>();
                for (int i = 0, pos = size - 7; i < numAlign - 1; i++, pos -= step)
                    result.Insert(0, pos);
                result.Insert(0, 6);
                return result;
            }
        }

        // Возвращает количество модулей данных для заданной версии QR-кода.
        public static int GetNumRawDataModules(int ver)
        {
            // Проверка версии
            if (ver < MIN_VERSION || ver > MAX_VERSION)
                throw new ArgumentException("Version number out of range");

            int result = (16 * ver + 128) * ver + 64;
            // Вычет модулей, занятых функциональными паттернами
            if (ver >= 2)
            {
                int numAlign = ver / 7 + 2;
                result -= (25 * numAlign - 10) * numAlign - 55;
                if (ver >= 7)
                    result -= 36;
            }

            return result;
        }

        // Возвращает количество кодовых слов данных для заданной версии и уровня коррекции ошибок.
        public static int GetNumDataCodewords(int ver, Ecc ecl)
        {
            int numRawDataModules = GetNumRawDataModules(ver);
            int eccCodewordsPerBlock = ECC_CODEWORDS_PER_BLOCK[(int)ecl][ver];
            int numErrorCorrectionBlocks = NUM_ERROR_CORRECTION_BLOCKS[(int)ecl][ver];

            return numRawDataModules / 8 - eccCodewordsPerBlock * numErrorCorrectionBlocks;
        }

        // Вычисляет порождающий полином для алгоритма Рида-Соломона.
        private List<byte> ReedSolomonComputeDivisor(int degree)
        {
            // Проверка степени полинома
            if (degree < 1 || degree > 255)
                throw new ArgumentOutOfRangeException(nameof(degree), "Degree out of range");

            // Коэффициенты полинома хранятся от старшей степени к младшей, 
            // исключая старший член, который всегда равен 1.
            // Например, полином x^3 + 255x^2 + 8x + 93 хранится как массив байтов {255, 8, 93}.
            List<byte> result = new List<byte>(new byte[degree]);
            result[result.Count - 1] = 1;  // Начинаем с монома x^0

            // Вычисление полинома-произведения (x - r^0) * (x - r^1) * (x - r^2) * ... * (x - r^{degree-1}),
            // отбрасывая старший моном, который всегда равен 1x^degree.
            // r = 0x02 - порождающий элемент поля GF(2^8/0x11D).
            byte root = 1;
            for (int i = 0; i < degree; i++)
            {
                // Умножение текущего произведения на (x - r^i)
                for (int j = 0; j < result.Count; j++)
                {
                    result[j] = ReedSolomonMultiply(result[j], root);
                    if (j + 1 < result.Count)
                        result[j] ^= result[j + 1];
                }
                root = ReedSolomonMultiply(root, 0x02);
            }
            return result;
        }

        // Вычисляет остаток от деления данных на порождающий полином по алгоритму Рида-Соломона.
        private List<byte> ReedSolomonComputeRemainder(List<byte> data, List<byte> divisor)
        {
            List<byte> result = new List<byte>(new byte[divisor.Count]);

            foreach (byte b in data)
            {
                byte factor = (byte)(b ^ result[0]);
                result.RemoveAt(0);
                result.Add(0);

                for (int i = 0; i < result.Count; i++)
                {
                    result[i] ^= ReedSolomonMultiply(divisor[i], factor);
                }
            }

            return result;
        }

        // Умножает два элемента поля GF(2^8/0x11D) по алгоритму Рида-Соломона.
        private byte ReedSolomonMultiply(byte x, byte y)
        {
            // Умножение "русским крестьянским методом"
            int z = 0;
            for (int i = 7; i >= 0; i--)
            {
                z = z << 1 ^ (z >> 7) * 0x11D;
                z ^= (y >> i & 1) * x;
            }
            return (byte)z;
        }

        // Подсчитывает количество паттернов, похожих на искатели, в истории запусков.
        private int FinderPenaltyCountPatterns(int[] runHistory)
        {
            int n = runHistory[1];
            // Проверка на соответствие паттерну искателя
            bool core = n > 0 && runHistory[2] == n && runHistory[3] == n * 3 && runHistory[4] == n && runHistory[5] == n;
            // Подсчет паттернов
            return (core && runHistory[0] >= n * 4 && runHistory[6] >= n ? 1 : 0)
                 + (core && runHistory[6] >= n * 4 && runHistory[0] >= n ? 1 : 0);
        }

        // Завершает текущий запуск, обновляет историю запусков и подсчитывает штрафные очки.
        private int FinderPenaltyTerminateAndCount(bool currentRunColor, int currentRunLength, int[] runHistory)
        {
            // Завершение темного запуска
            if (currentRunColor)
            {
                FinderPenaltyAddHistory(currentRunLength, runHistory);
                currentRunLength = 0;
            }
            // Добавление светлой границы к последнему запуску
            currentRunLength += size;
            // Обновление истории запусков
            FinderPenaltyAddHistory(currentRunLength, runHistory);
            // Подсчет штрафных очков
            return FinderPenaltyCountPatterns(runHistory);
        }

        // Добавляет текущую длину запуска в историю запусков.
        private void FinderPenaltyAddHistory(int currentRunLength, int[] runHistory)
        {
            if (runHistory[0] == 0)
                // Добавление светлой границы к начальному запуску
                currentRunLength += size;
            // Сдвиг истории запусков
            Array.Copy(runHistory, 0, runHistory, 1, runHistory.Length - 1);
            // Добавление текущей длины запуска
            runHistory[0] = currentRunLength;
        }

        // Возвращает значение бита по индексу.
        private bool GetBit(long x, int i) => (x >> i & 1) != 0;
        // Возвращает значение бита по индексу.
        private bool GetBit(int x, int i) => (x >> i & 1) != 0;
    }
}