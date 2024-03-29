﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CodeGenerated.Cryptoprimes;

namespace cryptoprime.VinKekFish
{
    /// <summary>Базовая однопоточная реализация VinKekFish для K = 1. Использование для тестирования. См. также descr.md</summary>
    public static unsafe class VinKekFishBase_etalonK1  // Была ошибка: отсутствовали комментарии в коде
    {                                                                                   /// <summary>Размер криптографического состояния в байтах (3200)</summary>
        public const int CryptoStateLen          = 3200;                                /// <summary>Размер криптографического состояния в блоках keccak (16)</summary>
        public const int CryptoStateLenKeccak    = CryptoStateLen / KeccakBlockLen;     /// <summary>Размер криптографического состояния в блоках ThreeFish (25)</summary>
        public const int CryptoStateLenThreeFish = CryptoStateLen / ThreeFishBlockLen;
                                                                                        /// <summary>Размер tweak (16 байтов, 2*ulong)</summary>
        public const int CryptoTweakLen          = 8*2;
                                                                                        /// <summary>Размер блока ввода-вывода (512 байтов = 4096 битов)</summary>
        public const int BLOCK_SIZE              = 512;                                 /// <summary>Размер максимального блока для ввода начала ключа: 2048 байтов (16384 бита)</summary>
        public const int MAX_SINGLE_KEY          = 2048;                                /// <summary>Максимально допустимая длина ОВИ (открытого вектора инициализации): 1148 байтов = 9184 битов</summary>
        public const int MAX_OIV                 = 1148;
                                                                                        /// <summary>Минимально допустимое количество раундов</summary>
        public const int MIN_ROUNDS              = 1;                                   /// <summary>Нормальное количество раундов</summary>
        public const int NORMAL_ROUNDS           = 64;                                  /// <summary>Уменьшенное количество раундов</summary>
        public const int REDUCED_ROUNDS          = 16;
                                                                                        /// <summary>Нормальная длина ключа в байтах (1024 байта = 8192 бита)</summary>
        public const int NORMAL_KEY              = 1024;                                /// <summary>Рекомендованная длина ключа в байтах (2048 байтов = 16384 бита)</summary>
        public const int RECOMMENDED_KEY         = 2048;                                /// <summary>Уменьшенная длина ключа в байтах (512 байтов = 4096 битов)</summary>
        public const int REDUCED_KEY             = 512;

        // Добавление: добавил дополнительные константы и изменил те, что были при проверке и чтении документации по губке https://keccak.team/files/SpongeFunctions.pdf

        /// <summary>Поглощение ключа губкой. Полное поглощение, включая криптографию. Пользователю не нужно, т.к. нужно использовать более специфические классы, например, VinKekFish_k1_base_20210419_keyGeneration</summary>
        /// <param name="key">Ключ</param>
        /// <param name="key_length">Длина ключа</param>
        /// <param name="OIV">Открытый вектор инициализации. Может быть null</param>
        /// <param name="OIV_length">Длина открытого вектора инициализации</param>
        /// <param name="state">Криптографическое состояние</param>
        /// <param name="state2">Вспомогательный массив криптографического состояния</param>
        /// <param name="b">Вспомогательный массив для функции keccak-f, размер b_size (25*8=200)</param>
        /// <param name="c">Вспомогательный массив для функции keccak-f, размер c_size (05*8=40)</param>
        /// <param name="tweak">Tweak, длина CryptoTweakLen (16)</param>
        /// <param name="tweakTmp">Вспомогательный массив для хранения tweak (длина CryptoTweakLen)</param>
        /// <param name="tweakTmp2">Второй вспомогательный массив для хранения tweak (длина CryptoTweakLen)</param>
        /// <param name="Initiated"> При пользовательском вызове всегда false. Если <see langword="false"/>, то state инициализированно, но никакие данные не вводились. Если true, то в state уже вводились данные: например, другой ключ. Если false - идёт перезапись. Если <see langword="true"/> - поглощение через xor</param>
        /// <param name="SecondKey">При пользовательском вызове всегда false. Вторичный отрезок ключа: при рекурсивном вызове этот параметр равен true, означая, что идёт поглощение следующих за первым отрезков ключей</param>
        /// <param name="R">Количество раундов для первого поглощения</param>
        /// <param name="RE">Количество раундов для отбоя после поглощения всего ключа (не рекомендуется делать низким)</param>
        /// <param name="RM">Количество раундов для поглощения дополнительных участков ключа (можно сделать низким, например, REDUCED_ROUNDS)</param>
        /// <param name="tablesForPermutations">Таблицы перестановок для всех раундов</param>
        /// <param name="transpose200_3200">Таблица перестановок transpose200_3200, см. GenTables()</param>
        public static void InputKey(byte * key, ulong key_length, byte * OIV, ulong OIV_length, byte * state, byte * state2, byte * b, byte *c, ulong * tweak, ulong * tweakTmp, ulong * tweakTmp2, bool Initiated, bool SecondKey, int R, int RE, int RM, ushort * tablesForPermutations, ushort * transpose200_3200, ushort * transpose200_3200_8)
        {
            if (SecondKey && OIV != null)
                throw new ArgumentException("VinKekFishBase_etalonK1.InputKey: SecondKey && OIV != null");

            if (SecondKey && RE != 0)
                throw new ArgumentOutOfRangeException("SecondKey && RE != 0");

            if (SecondKey != Initiated)
                throw new ArgumentOutOfRangeException("SecondKey != Initiated");

            if (OIV == null && OIV_length != 0)
                throw new ArgumentOutOfRangeException("VinKekFishBase_etalonK1.InputKey: OIV == null && OIV_length != 0");

            if (OIV != null && OIV_length > MAX_OIV)
                throw new ArgumentOutOfRangeException("VinKekFishBase_etalonK1.InputKey: OIV_length > MAX_OIV");

            if (key == null)
                throw new ArgumentNullException("VinKekFishBase_etalonK1.InputKey: key == null");

            if (key_length <= 0)
                throw new ArgumentNullException("VinKekFishBase_etalonK1.InputKey: key_length <= 0");

            if (R < MIN_ROUNDS)
                throw new ArgumentOutOfRangeException("R < MIN_ROUNDS");        // была ошибка: Добавлены локализующее сообщение, изменил MIN_INNER_ROUNDS
            if (RE < MIN_ROUNDS && !SecondKey)
                throw new ArgumentOutOfRangeException("RE < MIN_ROUNDS && !SecondKey"); // была ошибка: Исправлено неверное сообщение об ошибке

            var dataLen = key_length;
            var data    = key; // key:[0, key_length]
            if (SecondKey)
            {
                // dataLen всегда не более BLOCK_SIZE
                // dataLen никогда не увеличивается и равна key_length или менее
                if (dataLen > BLOCK_SIZE)
                    dataLen = BLOCK_SIZE;

                // data:[0, BLOCK_SIZE], data:[dataLen], data:[key_length]
                
                // i:[0, dataLen]
                for (ulong i = 0; i < dataLen; i++, data++)
                {
                    state[i+2] ^= *data;        // i+2 - это допустимо, т.к. речь идёт о вводе по алгоритму именно нужного размера
                } // state имеет размер CryptoStateLen (3200). Значит i всегда внутри state, т.к. BLOCK_SIZE = 512+2 < 3200

                // data = key + dataLen
                // data:[key_length - dataLen]
                // Если key_length <= BLOCK_SIZE, то data:[0]
            }
            else
            {
                if (dataLen > MAX_SINGLE_KEY)
                    dataLen = MAX_SINGLE_KEY;

                // data:[0, MAX_SINGLE_KEY], data:[dataLen], data:[key_length]

                for (ulong i = 0; i < dataLen; i++, data++)
                {
                    state[i+2] = *data;
                } // Аналогично, 2048+2 < 3200

                // data = key + dataLen
                // data:[key_length - dataLen]
                // Если key_length <= MAX_SINGLE_KEY, то data:[0]
                // метка :MEdMiY2Z44i6
            }

            byte len1 = (byte) dataLen;
            byte len2 = (byte) (dataLen >> 8);

            state[0] ^= len1;       // state нигде не изменялся и имеет индексы 0 и 1
            state[1] ^= len2;

            tweak[0] += 1253539379;

            if (!SecondKey)
            {
                tweak[1] += key_length; // tweak имеет 2 элемента ulong, 1 < 2

                if (OIV != null && OIV_length > 0)
                {
                    len1 = (byte) OIV_length;
                    len2 = (byte) (OIV_length >> 8);

                    state[2050] ^= len1;
                    state[2051] ^= len2;        // 2051 < 3200
                    // OIV_length:[1, MAX_OIV] = [1, 1148]
                    for (ulong i = 0; i < OIV_length; i++, OIV++)
                    {
                        state[i+2052] = *OIV;
                    }
                    // Оканчиваем на i = OIV_length - 1 = 1148 - 1 = 1147
                    // 1147 + 2052 = 3199 < 3200
                }
            }

            // TODO: указатели на таблицы перестановок
            step
            (
                countOfRounds: R, tablesForPermutations: tablesForPermutations,
                tweak: tweak, tweakTmp: tweakTmp, tweakTmp2: tweakTmp2, state: state, state2: state, b: b, c: c
            );

            if (key_length > dataLen)
            {
                InputKey
                (
                    key:        data,           // Это соответствует размеру выше data:[key_length - dataLen] (метка MEdMiY2Z44i6 )
                    key_length: key_length - dataLen,

                    SecondKey:  true,
                    Initiated:  true,

                    OIV:        null,
                    OIV_length: 0,

                    R:          RM,             // Повторный ввод ключа осуществляется под RM раундов
                    RM:         RM,
                    RE:         0,

                    state: state, state2: state2, tweak: tweak, tweakTmp: tweakTmp, tweakTmp2: tweakTmp2, b: b, c: c, tablesForPermutations: tablesForPermutations, transpose200_3200: transpose200_3200, transpose200_3200_8: transpose200_3200_8
                );
            }

            // Завершаем ввод ключа отбоем. Т.к. вызов функции рекурсивный, отбой происходит только в самой верхней функции - SecondKey = false
            if (!SecondKey)
            {
                InputData_Overwrite(data: null, state: state, dataLen: 0, tweak: tweak, regime: 255);
                step
                (
                    countOfRounds: RE, tablesForPermutations: tablesForPermutations,
                    tweak: tweak, tweakTmp: tweakTmp, tweakTmp2: tweakTmp2, state: state, state2: state, b: b, c: c
                );
            }
        }

        /// <summary>Сырой ввод данных. Вводит данные в состояние путём перезатирания (режим OVERWRITE), изменяет tweak. Не вызывает криптографические функции</summary>
        /// <param name="data">Указатель на вводимые данные, может быть null, если dataLen == 0</param>
        /// <param name="state">Указатель на криптографическое состояние</param>
        /// <param name="dataLen">Длина вводимых данных, не более BLOCK_SIZE (512 байтов)</param>
        /// <param name="tweak">Указатель на tweak (для соответствующего изменения tweak)</param>
        /// <param name="regime">Счётчик режима ввода</param>
        /// <param name="nullPaddding">Если <see langword="true"/>, то если данных меньше, чем BLOCK_SIZE, оставшиеся байты будут перезатёрты нулями, обеспечивая необратимость. Иначе остальные байты останутся неизменными</param>
        public static void InputData_Overwrite(byte * data, byte * state, ulong dataLen, ulong * tweak, byte regime, bool nullPaddding = true)
        {// nullPaddding был добавлен в ходе проверки
            if (dataLen > BLOCK_SIZE)
                throw new ArgumentOutOfRangeException();

            ulong i = 0;
            for (; i < dataLen; i++, data++)
            {
                state[i+3] = *data; // i + 3 = BLOCK_SIZE + 3 = 512 + 3 < 3200
            }

            if (nullPaddding)
            for (; i < BLOCK_SIZE; i++)
            {
                state[i+3] = 0;
            }

            byte len1 = (byte) dataLen;
            byte len2 = (byte) (dataLen >> 8);

            len2 &= 0x80;   // Старший бит количества вводимых байтов устанавливается в 1, если используется режим Overwrite

            // Ошибка с &= исправлена

            state[0] ^= len1;
            state[1] ^= len2;
            state[2] ^= regime;

            InputData_ChangeTweak(tweak: tweak, dataLen: (long) dataLen, Overwrite: true, regime: regime);
        }

        /// <summary>Сырой ввод данных. Вводит данные в состояние через xor (режим ввода sponge), изменяет tweak. Не вызывает криптографические функции</summary>
        public static void InputData_Xor(byte * data, byte * state, long dataLen, ulong * tweak, byte regime)
        {
            if (dataLen > BLOCK_SIZE)
                throw new ArgumentOutOfRangeException();

            for (long i = 0; i < dataLen; i++, data++)
            {
                state[i+3] ^= *data;    // BLOCK_SIZE = 512 < 3200; i < dataLen
            }

            byte len1 = (byte) dataLen;
            byte len2 = (byte) (dataLen >> 8);

            state[0] ^= len1;
            state[1] ^= len2;
            state[2] ^= regime;

            InputData_ChangeTweak(tweak: tweak, dataLen: dataLen, Overwrite: false, regime: regime);
        }

        /// <summary>Этот метод вызывать не надо, изменяет tweak. Он автоматически вызывается при вызове InputData_*</summary>
        public static void InputData_ChangeTweak(ulong * tweak, long dataLen, bool Overwrite, byte regime)
        {
            // Приращение tweak перед вводом данных
            tweak[0] += 1253539379;         // tweak - массив, длиной 2. Всё в порядке, индексы 0 и 1.

            tweak[1] += (ulong) dataLen;
            if (Overwrite)
                tweak[1] += 0x0100_0000_0000_0000;

            var reg = ((ulong) regime) << 40; // 8*5 - третий по старшинству байт, нумерация с 1
            tweak[1] += reg;
        }

        /// <summary>Если никаких данных не введено в режиме Sponge (xor), изменяет tweak</summary>
        public static void NoInputData_ChangeTweak(byte * state, ulong * tweak, byte regime)
        {
            // Приращение tweak перед вводом данных
            tweak[0] += 1253539379;

            // tweak[1] += dataLen;
            state[2] ^= regime;

            var reg = ((ulong) regime) << 40; // 8*5 - третий по старшинству байт, нумерация с 1
            tweak[1] += reg;
        }

        /// <summary>Шаг алгоритма ПОСЛЕ ввода данных. Перед step необходимо вызывать NoInputData_ChangeTweak или InputData_*</summary>
        /// <param name="countOfRounds">Количество раундов</param>
        /// <param name="tweak">Tweak после ввода данных, 16 байтов (все массивы могут быть в одном, если это удобно). Не изменяется в функции.</param>
        /// <param name="tweakTmp">Дополнительный массив для временного tweak, 16 байтов. Изменяется в функции.</param>
        /// <param name="tweakTmp2">Дополнительный массив для временного tweak, 16 байтов. Изменяется в функции.</param>
        /// <param name="state">Криптографическое состояние</param>
        /// <param name="state2">Вспомогательный массив для криптографического состояния</param>
        /// <param name="tablesForPermutations">Массив таблиц перестановок на каждый раунд. Длина должна быть countOfRounds*4 таблиц (CryptoStateLen*ushort на каждую таблицу)</param>
        /// <param name="b">Вспомогательный массив b для keccak.Keccackf</param>
        /// <param name="c">Вспомогательный массив c для keccak.Keccackf</param>
        public static void step(int countOfRounds, ulong * tweak, ulong * tweakTmp, ulong * tweakTmp2, byte * state, byte * state2, ushort * tablesForPermutations, byte* b, byte* c)
        {
            tweakTmp[0] = tweak[0];
            tweakTmp[1] = tweak[1];

            // Распределение впитывания
            DoPermutation(state, state2, CryptoStateLen, transpose128_3200);    // CryptoStateLen действительно является верным значением длины
            DoThreefishForAllBlocks(state2, state, tweakTmp, tweakTmp2);
            DoPermutation(state, state2, CryptoStateLen, transpose128_3200);
            BytesBuilder.CopyTo(CryptoStateLen, CryptoStateLen, state2, state);

            // tablesForPermutations имеет размер countOfRounds*4*CryptoStateLen*ushort
            // За один проход цикла идёт приращение 4 раза на CryptoStateLen
            // Так как tablesForPermutations имеет тип ushort * , то приращение на CryptoStateLen
            // является приращением в байтах на CryptoStateLen * 2
            // Таким образом, за один проход приращение идёт на 4 * CryptoStateLen * 2
            // Всего проходов countOfRounds.
            // Таким образом, всего идёт приращений на countOfRounds * 4 * CryptoStateLen * 2
            // Это как раз и есть размер таблицы. То есть всё хорошо.
            // tablesForPermutations после цикла не используется, т.к. полностью израсходована.

            // Основной шаг алгоритма: раунды
            for (int round = 0; round < countOfRounds; round++)
            {
                DoKeccakForAllBlocks(state, CryptoStateLenKeccak, b: (ulong*) b, c: (ulong*) c);
                DoPermutation(state, state2, CryptoStateLen, tablesForPermutations);
                tablesForPermutations += CryptoStateLen;

                DoThreefishForAllBlocks(state2, state, tweakTmp, tweakTmp2);
                DoPermutation(state, state2, CryptoStateLen, tablesForPermutations);
                tablesForPermutations += CryptoStateLen;

                // Довычисление tweakVal для второго преобразования VinKekFish
                tweakTmp[0] += 0x1_0000_0000U;

                DoKeccakForAllBlocks(state2, CryptoStateLenKeccak, b: (ulong*) b, c: (ulong*) c);
                DoPermutation(state2, state, CryptoStateLen, tablesForPermutations);
                tablesForPermutations += CryptoStateLen;

                DoThreefishForAllBlocks(state, state2, tweakTmp, tweakTmp2);
                DoPermutation(state2, state, CryptoStateLen, tablesForPermutations);
                tablesForPermutations += CryptoStateLen;

                // Вычисляем tweak для данного раунда (работаем со старшим 4-хбайтным словом младшего 8-мибайтного слова tweak)
                // Каждый раунд берёт +2 к старшему 4-хбайтовому слову; +1 - после первой половины, и +1 - после второй половины
                tweakTmp[0] += 0x1_0000_0000U;
            }

            // После последнего раунда производится заключительная рандомизация поблочной функцией keccak-f
            for (int i = 0; i < 2; i++)
            {
                DoKeccakForAllBlocks(state, CryptoStateLenKeccak, b: (ulong*) b, c: (ulong*) c);
                DoPermutation(state, state2, CryptoStateLen, transpose200_3200);
                DoKeccakForAllBlocks(state2, CryptoStateLenKeccak, b: (ulong*) b, c: (ulong*) c);
                DoPermutation(state2, state, CryptoStateLen, transpose200_3200_8);
            }
        }

        /// <summary>Выравнивает целое число i на интервал [0; ringModulo)</summary>
        /// <param name="i">Выравниваемое число</param>
        /// <param name="ringModulo">[0; ringModulo)</param>
        /// <returns>Выровненное число</returns>
        public static int getNumberFromRing(int i, int ringModulo)
        {//  При проверка сокрыто. Реализация не эффективно для больших i
            while (i < 0)
                i += ringModulo;

            while (i >= ringModulo)
                i -= ringModulo;

            return i;
        }

        const int ThreeFishBlockLen = 128;  // При проверке перенесено выше в константы
        const int    KeccakBlockLen = 200;

        /// <summary>Применяет ThreeFish поблочно ко всему состоянию алгоритма</summary>
        /// <param name="beginCryptoState">Начальное криптографическое состояние (инициализированное)</param>
        /// <param name="finalCryptoState">Финальное криптографическое состояние (для результата, будет перезатёрто)</param>
        /// <param name="tweak">Базовый tweak для раунда. Не изменяется</param>
        /// <param name="tweakTmp">Дополнительный массив для временного tweak</param>
        public static unsafe void DoThreefishForAllBlocks(byte* beginCryptoState, byte * finalCryptoState, ulong * tweak, ulong * tweakTmp)
        {
            int len = CryptoStateLenThreeFish;
            /*
            if ((len & 1) == 0)
                throw new ArgumentException("'len' must be odd", "len");
            */
            byte* cur = finalCryptoState;
            byte* key = beginCryptoState;

            BytesBuilder.CopyTo(CryptoStateLen, CryptoStateLen, beginCryptoState, finalCryptoState);    // CryptoStateLen - верно

            tweakTmp[0] = tweak[0];
            tweakTmp[1] = tweak[1];

            // getNumberFromRing не вызывается, вместо этого используется самостоятельный расчёт, он должен быть более быстрым
            int j   = len >> 1; // Начинаем с 12-ти, 25 / 2 = 12
            int add = 0;

            // cur - это финальное состояние, которое изменяется
            // key всегда вычисляется заново, т.к. он переходит через нуль - это массив ключевой информации для ThreeFish
            for (int i = 0; i < len; i++, j++, cur += ThreeFishBlockLen)
            {
                // cur - это byte * , так что все приращения - в байтах
                // cur += ThreeFishBlockLen - это приращение на 128 байтов.
                // Таких приращений всего len = CryptoStateLenThreeFish, то есть 25
                // Получается, что всего после конца цикла было приращений на 128*25=3200
                // То есть переполнения нет, cur точно смотрит на нулевой байт после массива

                if (j >= len)
                    j = 0;

                // j:[0, len - 1]

                // j * 128 - это как раз умножение на размер блока
                add = j << 7; // blockLen * j;

                // add всегда в диапазоне [0, 128*(len - 1)]
                // Таким образом, максимум будет 128*24=3072
                // Это должен быть указатель на последний блок. 3072+128=3200. Верно

                key = beginCryptoState + add;

                Threefish_Static_Generated.Threefish1024_step(key: (ulong *) key, tweak: (ulong *) tweakTmp, text: (ulong *) cur);

                tweakTmp[0] += 1;
            }
        }

        /// <summary>Применяет к криптографическому состоянию CryptoState поблочное преобразование keccak</summary>
        /// <param name="CryptoState">Криптографическое состояние</param>
        /// <param name="len">Длина криптографического состояния в блоках keccak (длина по 200 байтов; KeccakBlockLen)</param>
        public static unsafe void DoKeccakForAllBlocks(byte* CryptoState, int len, ulong * b, ulong * c)
        {
            byte* cur = CryptoState;
            // 16 * 200 = 3200
            for (int i = 0; i < len; i++, cur += KeccakBlockLen)
            {
                keccak.Keccackf(a: (ulong *) cur, c: c, b: b);
            }
        }

        /// <summary>Осуществляет перестановки байтов в массиве (для обеспечения диффузии)</summary>
        /// <param name="source">Исходный массив: из него берутся значения</param>
        /// <param name="target">Целевой массив: в него записываются значения</param>
        /// <param name="len">Длины обоих массивов в байтах</param>
        /// <param name="permutationTable">Таблица перестановок</param>
        public static void DoPermutation(byte* source, byte* target, int len, ushort* permutationTable)
        {
            /*
             * Перестановка:
             * Теперь байт с позиции source[permutationTable[i]] мы переставляем на позицию target[i]
             * 
             * Например, transpose200 должна быть [0, 200, 400, 600, 800 ...]
             * 
             * */


             // Таблица перестановок длиной CryptoStateLen*ushort байтов или CryptoStateLen элементов.
             // Len передаётся как раз CryptoStateLen
             // То есть переставляется CryptoStateLen байтов с использование CryptoStateLen элементов таблицы перестановок
             // Вроде бы, всё верно
             for (int i = 0; i < len; i++)
             {
                target[i] = source[permutationTable[i]];    // permutationTable должны содержать верные значения
             }
        }

        public static ushort* transpose128_3200    = null;
        public static ushort* transpose200_3200    = null;
        public static ushort* transpose200_3200_8  = null;
        // public static ushort* transpose400_3200_16 = null;

        public static readonly object sync = new object();

        /// <summary>Эту процедуру нужно вызвать для инициализации таблиц перестановок перед любым вызовом методов класса. Допускается многопоточный вызов без синхронизации. Вызов производится один раз на всю программу (на весь процесс)</summary>
        public static void GenTables()
        {
            lock (sync)
            {
                if (transpose128_3200 != null)
                    return;

                transpose128_3200    = GenTransposeTable(3200, 128);
                transpose200_3200    = GenTransposeTable(3200, 200);
                transpose200_3200_8  = GenTransposeTable(3200, 200,  stepInEndOfBlocks: 8);
                // transpose400_3200_16 = GenTransposeTable(3200, 400,  stepInEndOfBlocks: 16);
            }

            if (transpose128_3200[1] != 128)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose128_3200[1] != 128");
            if (transpose128_3200[8] != 1024)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose128_3200[8] != 1024");
            if (transpose200_3200[1] != 200)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose200_3200[1] != 200");
            if (transpose200_3200[8] != 1600)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose200_3200[8] != 1600");
            if (transpose200_3200[400] != 25)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose200_3200[400] != 25");
            if (transpose200_3200_8[2800] != 07)
                throw new Exception("VinKekFish: fatal algotirhmic error: GenTables - transpose200_3200_8[2800] != 07");
        }

        public static ushort* GenTransposeTable(ushort blockSize, ushort step, int numberOfRetries = 1, ushort stepInEndOfBlocks = 1, ushort stepInEndOfStep = 1)
        {
            var newTable = new ushort[blockSize];
            var buffer   = new ushort[blockSize];
            for (ushort i = 0; i < newTable.Length; i++)
            {
                newTable[i] = i;
                buffer  [i] = i;
            }

            for (int z = 0; z < numberOfRetries; z++)
            {
                int j = 0, k = 0;
                for (ushort i = 0; i < blockSize; i++)
                {
                    buffer[j++] = newTable[k];

                    k += step;
                    if (k >= blockSize)
                    {
                        k -= blockSize;
                        k += stepInEndOfBlocks;
                        if (k >= step)
                        {
                            k -= step;
                            k += stepInEndOfStep;
                        }
                    }
                }

                // Это просто копирование buffer в newTable
                // Так как оно происходит побайтово, то buffer.Length домножается на 2, т.к. элемент buffer типа ushort
                fixed (ushort* nt = newTable, buff = buffer)
                {
                    BytesBuilder.CopyTo(buffer.Length << 1, buffer.Length << 1, (byte*)buff, (byte*)nt);
                }
            }

            // Тестирование таблицы
            // Каждое значение должно быть представлено хотя бы один раз (и только один раз)
            for (ushort i = 0; i < newTable.Length; i++)
            {
                if (!newTable.Contains(i))
                {
                    throw new Exception("VinKekFish: fatal algotirhmic error 1: GenTransposeTable");
                }
            }
            /*
            var buffer1  = new byte[blockSize];
            var buffer2  = new byte[blockSize];
            // Двойное транспонирование: эта штука не работает. Здесь транспонирование не является операцией, которая обратна самой себе
            for (ushort i = 0; i < blockSize; i++)
            {
                buffer1[i] = (byte) i;
            }
            
            fixed (ushort* nt = newTable)
            fixed (byte*   b1 = buffer1, b2 = buffer2)
            {
                DoPermutation(b1, b2, blockSize, nt);
                DoPermutation(b2, b1, blockSize, nt);
            }

            for (ushort i = 0; i < blockSize; i++)
            {
                if (buffer1[i] != i)
                    throw new Exception("VinKekFish: fatal algotirhmic error 2: GenTransposeTable");
            }
            buffer1 = null;
            buffer2 = null;
            */

            // Вычисление размера newTable в байтах
            long rLen   = newTable.Length * sizeof(ushort);
            var result = (ushort *) Marshal.AllocHGlobal((int) rLen).ToPointer();

            // Выделили место и копируем
            fixed (ushort * newTablePointer = newTable)
            {// Вроде бы без переполнений
                byte * b = (byte *) newTablePointer;
                BytesBuilder.CopyTo(rLen, rLen, b, (byte *) result);
            }

            return result;
        }
    }
}
