// ФАЙЛ ДЛЯ АНАЛИЗА

using cryptoprime.VinKekFish;
using cryptoprime;
using vinkekfish.keccak.keccak_20200918;

using static cryptoprime.VinKekFish.VinKekFishBase_etalonK1;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static cryptoprime.BytesBuilderForPointers;

namespace vinkekfish
{
    // Описание состояний в файле ./Documentation/VinKekFish_k1_base_20210419_состояния.md
    // Файл не осуществляет ввода-вывода
    // Не имеет примитивов синхронизации
    // Не читает/записывает данные во внешние глобальные переменные

    /// <include file='Documentation/VinKekFish_k1_base_20210419.xml' path='docs/members[@name="VinKekFish_k1_base_20210419"]/VinKekFish_k1_base_20210419/*' />
    public unsafe class VinKekFish_k1_base_20210419: IDisposable
    {
		// 4. Начать со входа в программу (или статического конструктора проверяемого класса).
		
        protected       int    _RTables = 0;
        public          int    RTables => _RTables;
		
        protected       Record _state = null, _state2 = null, t0 = null, t1 = null, t2 = null, _transpose200_3200 = null, _b = null, _c = null;
        protected       Record stateHandle   = null;
        protected       Record pTablesHandle = null;

        protected bool isInited1 = false;
        protected bool isInited2 = false;
        public bool IsInited1 => isInited1;		// Всегда работает
        public bool IsInited2 => isInited2;		// Всегда работает
		
		// Всегда проинициализированно, невозможно перезаписать
		// Сам по себе аллокатор не имеет никаких состояний и уже проинициализирован при создании
		// Запись и чтение в/из объекта невозможны
		// Объект переходит в состояние "Уничтожен" при автоматическом вызове деструктора
		// Конфиденциальных данных не содержит, не нуждается в вызове очистки
		// Неверное использование невозможно
		// Объект не индексируемый, выход за его пределы невозможен
		// Запись в удалённый объект невозможна
        public static readonly AllocatorForUnsafeMemoryInterface AllocHGlobal_allocator = new AllocHGlobal_AllocatorForUnsafeMemory();

	// Это всегда работает
        protected volatile bool isHaveOutputData = false;
        public             bool IsHaveOutputData => isHaveOutputData;

        public VinKekFish_k1_base_20210419()
        {
            GC.Collect();
			// Сейчас ничего не проинициализированно.

			// Это можно вызывать когда угодно. Хотя в целом, нужно дополнительно проверять
			// Запись и чтение в сам объект не производится, см. ниже
            GenTables();
            GC.Collect();
			
			
			// 8l6xxHvrscnC
			// Проинициализированы статические таблицы перестановок в VinKekFishBase_etalonK1 transpose128_3200 и transpose200_3200 
			// В объекте ничего не изменилось: ничего не проинициализированно
        }

        public virtual void Init1(int RoundsForTables, byte * additionalKeyForTables, long additionalKeyForTables_length, byte[] OpenInitVectorForTables = null, int PreRoundsForTranspose = 8)
        {
			// Возможно, ничего ещё не проинициализированно
			
            Clear();
            GC.Collect();

            // Место на
            // Криптографическое состояние
            // Копию криптографического состояния
            // 4 tweak (основной и запасные)
            // new byte[CryptoStateLen * 2 + CryptoTweakLen * 4];
            // место для вспомогательных матриц c и b
			
			// 8. Все ли созданные объекты проходят полный допустимый или рекомендуемый цикл состояний?
			// stateHandle удаляется в Clear, очищается в ClearState
            stateHandle = AllocHGlobal_allocator.AllocMemory(CryptoStateLen * 2 + CryptoTweakLen * 4 + cryptoprime.keccak.b_size + cryptoprime.keccak.c_size);
            stateHandle.Clear();

			// 8. Все ли созданные объекты проходят полный допустимый или рекомендуемый цикл состояний?
			// В методе ClearState рассматривается уничтожение данных объектов
			// TODO: Уничтожение "NoCopyClone" и "+" записей
            _state  = stateHandle.NoCopyClone(CryptoStateLen);
            _state2 = _state  + CryptoStateLen; // Это перегруженная операция сложения, _state2 идёт за массивом _state и имеет длину CryptoStateLen
            t0      = _state2 + CryptoTweakLen;
            t1      = t0      + CryptoTweakLen;
            t2      = t1      + CryptoTweakLen;
            _b      = t2      + cryptoprime.keccak.b_size;
            _c      = _b      + cryptoprime.keccak.c_size;

            _transpose200_3200 = AllocHGlobal_allocator.AllocMemory(transpose200_3200.Length * sizeof(ushort));
            fixed (ushort * t = transpose200_3200)
            {
                byte * tt = (byte *) t;
                BytesBuilder.CopyTo(_transpose200_3200.len, _transpose200_3200.len, tt, _transpose200_3200);
            }

            _RTables       = RoundsForTables;
            pTablesHandle = GenStandardPermutationTables(Rounds: _RTables, key: additionalKeyForTables, key_length: additionalKeyForTables_length, OpenInitVector: OpenInitVectorForTables, PreRoundsForTranspose: PreRoundsForTranspose);


            GC.Collect();
            GC.WaitForPendingFinalizers();  // Это чтобы сразу получить все проблемные вызовы, связанные с утечками памяти
            isInited1 = true;
        }

        /// <summary>Вторая инициализация: ввод ключа и ОВИ, обнуление состояния и т.п.</summary>
        /// <param name="key">Основной ключ</param>
        /// <param name="OpenInitVector">Основной вектор инициализации, может быть null</param>
        /// <param name="Rounds">Количество раундов при шифровании первого блока ключа (рекомендуется 16-64)</param>
        /// <param name="RoundsForEnd">Количество раундов при широфвании последующих блоков ключа (допустимо 4)</param>
        /// <param name="RoundsForExtendedKey">Количество раундов отбоя ключа (рекомендуется NORMAL_ROUNDS = 64)</param>
        public virtual void Init2(byte * key, ulong key_length, byte[] OpenInitVector, int Rounds = NORMAL_ROUNDS, int RoundsForEnd = NORMAL_ROUNDS, int RoundsForExtendedKey = REDUCED_ROUNDS)
        {
            if (!isInited1)
                throw new ArgumentOutOfRangeException("VinKekFish_k1_base_20210419: Init1 must be executed before Init2");

            // В этой и вызываемых функциях требуется проверка на наличие ошибок в неверных параметрах
            ClearState();
            if (pTablesHandle == null)
                throw new ArgumentOutOfRangeException("VinKekFish_k1_base_20210419: Init1 must be executed before Init2 (pTables == null)");

            fixed (byte * oiv = OpenInitVector)
            {
                InputKey
                (
                    key: key, key_length: key_length, OIV: oiv, OpenInitVector == null ? 0 : (ulong) OpenInitVector.LongLength,
                    state: _state, state2: _state2, b: _b, c: _c,
                    tweak: t0, tweakTmp: t1, tweakTmp2: t2,
                    Initiated: false, SecondKey: false,
                    R: Rounds, RE: RoundsForEnd, RM: RoundsForExtendedKey, tablesForPermutations: pTablesHandle, transpose200_3200: _transpose200_3200
                );
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();  // Это чтобы сразу получить все проблемные вызовы, связанные с утечками памяти
            isInited2 = true;
        }

        public void Clear()
        {
			// Здесь может быть вообще ничего не проинциализированно
			// Может же быть такое, что в таблицах перестановок и криптографическом состоянии - конфиденциальные данные
			// Запись разрешена и требуется семантикой метода
            isInited1 = false;
			
			// Это тоже может быть вызвано
            ClearState();

			// Обнуление с проверкой на null
			// Обнуление разрешено и требуется семантикой метода
			// Чтение не производится (если не считать получение полей объекта)
			// Высвобождение выделенной памяти: требуется семантикой метода
			// Производится ли полная очистка? Очистка производится автоматически. Должна быть полной
			
            pTablesHandle?.Dispose();
            _transpose200_3200?.Dispose();
            stateHandle?.Dispose();			// pnXFNkUrR6Pw

			// Возможно ли после очистки использовать объекты pTablesHandle, _transpose200_3200 и stateHandle ?
			// Нет, невозможно. Все они обнулены ниже. Повторный вход в этот же метод не даст ошибки, т.к. везде идёт проверка на null

			// 7.0. Все ли созданные объекты переходят в состояние "очищен" и "уничтожен"?
/* Это декларации объектов
Record _state = null, _state2 = null, t0 = null, t1 = null, t2 = null, _transpose200_3200 = null, _b = null, _c = null;
Record stateHandle   = null;
Record pTablesHandle = null;

t2 обнуляется трижды
Указатели обнуляются все.

Содержимое: stateHandle отвечает за другие указатели

_state  = stateHandle.NoCopyClone(CryptoStateLen);
_state2 = _state  + CryptoStateLen; // Это перегруженная операция сложения, _state2 идёт за массивом _state и имеет длину CryptoStateLen
t0      = _state2 + CryptoTweakLen;
t1      = t0      + CryptoTweakLen;
t2      = t1      + CryptoTweakLen;
_b      = t2      + cryptoprime.keccak.b_size;
_c      = _b      + cryptoprime.keccak.c_size;

Таким образом, при обнулении stateHandle уже не требуют обнуления перечисленные указатели
Остаются только _transpose200_3200, stateHandle и pTablesHandle
Именно они обнуляются выше
*/
		
// 7.1. Можно ли понять, что объекты находятся в непригодном состоянии (непроинициализирован, очищен, уничтожен)?
// Указатели на все объекты обнулены - можно понять по этому
// Деструкторов нет
// Как уже указывалось, повторный вызов не произведёт каких-либо проблем

            stateHandle = null;			// pnXFNkUrR6Pw
            _state      = null;
            _state2     = null;
            t0          = null;
            t1          = null;
            t2          = null;
            t2          = null;
            t2          = null;
            _b          = null;
            _c          = null;

            _RTables            = 0;
            pTablesHandle      = null;
            _transpose200_3200 = null;

            GC.Collect();
        }

        public void ClearState()
        {
			// Здесь может быть ничего не проинициализированно. Может же и наоборот.
            isInited2        = false;
            isHaveOutputData = false;

            // Здесь обнуление состояния
			// Как видно, обнуление происходит, только если объект не является null
            stateHandle?.Clear();
        }

        /// <summary>Генерирует стандартную таблицу перестановок</summary>
        /// <param name="Rounds">Количество раундов, для которых идёт генерация. Для каждого раунда по 4-ре таблицы</param>
        /// <param name="key">Это вспомогательный ключ для генерации таблиц перестановок. Основной ключ вводить нельзя! Этот ключ не может быть ключом, вводимым в VinKekFish, см. описание VinKekFish.md</param>
        /// <param name="PreRoundsForTranspose">Количество раундов, где таблицы перестановок не генерируются от ключа, а идут стандартно transpose128_3200 и transpose200_3200</param>
        public static Record GenStandardPermutationTables(int Rounds, AllocatorForUnsafeMemoryInterface allocator = null, byte * key = null, long key_length = 0, byte[] OpenInitVector = null, int PreRoundsForTranspose = 8)
        {
            if (PreRoundsForTranspose < 1 || PreRoundsForTranspose > Rounds)
                throw new ArgumentOutOfRangeException("VinKekFish_base_20210419.GenStandardPermutationTables: PreRoundsForTranspose < 1 || PreRoundsForTranspose > Rounds");

            if (allocator == null)
                allocator = AllocHGlobal_allocator;

            using var prng = new Keccak_PRNG_20201128();

            if (key != null && key_length > 0)
                prng.InputKeyAndStep(key, key_length);

            if (OpenInitVector != null && OpenInitVector.Length > 0)
            {
                prng.InputBytes(OpenInitVector);
                prng.calcStep();
            }

            long len1  = VinKekFishBase_etalonK1.CryptoStateLen;
            long len2  = VinKekFishBase_etalonK1.CryptoStateLen << 1;

            var result = allocator.AllocMemory(len1 * Rounds * 4 * sizeof(ushort));
            var table1 = new ushort[len1];
            var table2 = new ushort[len1];

            for (ushort i = 0; i < table1.Length; i++)
            {
                table1[i] = i;
                table2[i] = (ushort) (table1.Length - i - 1);
            }

            fixed (ushort * transpose200_3200_u = VinKekFishBase_etalonK1.transpose200_3200, transpose128_3200_u = VinKekFishBase_etalonK1.transpose128_3200)
            fixed (ushort * Table1 = table1, Table2 = table2)
            {
                ushort * R = result;
                ushort * r = R;
                byte * transpose200_3200 = (byte *) transpose200_3200_u;
                byte * transpose128_3200 = (byte *) transpose128_3200_u;

                for (; PreRoundsForTranspose > 0 && Rounds > 0; Rounds--, PreRoundsForTranspose--)
                {
                    BytesBuilder.CopyTo(len2, len2, transpose200_3200, (byte *) r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, transpose128_3200, (byte *) r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, transpose200_3200, (byte *) r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, transpose128_3200, (byte *) r); r += len1;
                }

                for (; Rounds > 0; Rounds--)
                {
                    prng.doRandomPermutationForUShorts(table1);
                    prng.doRandomPermutationForUShorts(table2);
/*  // Если необходимо, раскомментировать отладочный код: здесь проверяется, что перестановки были корректны (что они перестановки, а не какие-то ошибки)
#if DEBUG
                    CheckPermutationTable(table1);
                    CheckPermutationTable(table2);
#endif
*/
                    BytesBuilder.CopyTo(len2, len2, (byte*)Table1, (byte*)r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, (byte*)Table2, (byte*)r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, transpose200_3200, (byte*)r); r += len1;
                    BytesBuilder.CopyTo(len2, len2, transpose128_3200, (byte*)r); r += len1;
                }

                BytesBuilder.ToNull(table1.Length * sizeof(ushort), (byte *) Table1);
                BytesBuilder.ToNull(table1.Length * sizeof(ushort), (byte *) Table2);
            }

            return result;
        }

#if DEBUG
        private static void CheckPermutationTable(ushort[] table1)
        {
            bool found;
            for (int i = 0; i < table1.Length; i++)
            {
                found = false;
                for (int j = 0; j < table1.Length; j++)
                {
                    if (table1[j] == i)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    throw new Exception($"DEBUG: doRandomPermutationForUShorts incorrect: value {i} not found");
            }
        }
#endif

        /// <summary>Выполняет один шаг криптографического преобразования. Это сокращённый вызов step без подготовки tweak. Не использовать напрямую</summary>
        /// <param name="CountOfRounds">Количество раундов</param>
        protected void DoStep(int CountOfRounds)
        {
            step
            (
                countOfRounds: CountOfRounds, tablesForPermutations: pTablesHandle, transpose200_3200: _transpose200_3200,
                tweak: t0, tweakTmp: t1, tweakTmp2: t2, state: _state, state2: _state, b: _b, c: _c
            );

            isHaveOutputData = true;
        }

        /// <summary>Получает из криптографического состояния вывод</summary>
        /// <param name="output">Массив для получения вывода</param>
        /// <param name="start">Индекс в массиве output, с которого надо начинать запись</param>
        /// <param name="outputLen">Длина массива output</param>
        /// <param name="countToOutput">Количество байтов, которое нужно изъять из массива</param>
        public virtual void outputData(byte * output, long start, long outputLen, long countToOutput)
        {
            if (!isHaveOutputData)
                throw new ArgumentOutOfRangeException("VinKekFish_k1_base_20210419.outputData: !isHaveOutputData");

            if (countToOutput > BLOCK_SIZE)
                throw new ArgumentOutOfRangeException("VinKekFish_k1_base_20210419.outputData: lenToOutput > BLOCK_SIZE");

            if (start + countToOutput > outputLen)
                throw new ArgumentOutOfRangeException("VinKekFish_k1_base_20210419.outputData: start + lenToOutput > len");

            BytesBuilder.CopyTo(countToOutput, outputLen, _state, output, start);
            isHaveOutputData = false;
        }

        /// <summary>Уничтожает объект: реализация IDisposable</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Очищает объект</summary>
        /// <param name="disposing"><see langword="true"/> при всех вызовах, исключая деструктор</param>
        public virtual void Dispose(bool disposing)
        {
            Clear();

            if (!disposing)
                throw new Exception("VinKekFish_k1_base_20210419.Dispose: ~VinKekFish_k1_base_20210419 executed");
        }

        ~VinKekFish_k1_base_20210419()
        {
            Dispose(false);
        }
    }
}
