using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using cryptoprime;
using static cryptoprime.keccak;
using static cryptoprime.BytesBuilderForPointers;

namespace vinkekfish.keccak.keccak_20200918
{
    public unsafe class Keccak_PRNG_20201128 : Keccak_base_20200918
    {
		// allocator никогда не null cRdzaP6t0ZtB
        public readonly AllocatorForUnsafeMemoryInterface allocator             = new BytesBuilderForPointers.AllocHGlobal_AllocatorForUnsafeMemory();
        public          AllocatorForUnsafeMemoryInterface allocatorForSaveBytes = new BytesBuilderForPointers.AllocHGlobal_AllocatorForUnsafeMemory(); // new BytesBuilderForPointers.Fixed_AllocatorForUnsafeMemory();
        // Fixed работает раза в 3 медленнее почему-то

        public Keccak_PRNG_20201128(AllocHGlobal_AllocatorForUnsafeMemory allocator = null)
        {
			// allocator либо не создан, либо готов к использованию
            if (allocator != null)
				// allocator готов к использованию. В таком случае, он приравнивается к this.allocator
				// Состояния не меняются, this.allocator перезаписывается
                this.allocator = allocator;

			// Либо this.allocator был создан при конструировании объекта, либо, если не null, перезаписан в конструкторе
			// Следовательно, this.allocator никогда не null :cRdzaP6t0ZtB

			// this.allocator
			// MCaZG5ctJub4.Готов к использованию
			// Аллокатор никогда не null и, при этом, создан. Это значит, что готов к использованию

			// InputSize - константа, её использование всегда верно
			// AllocMemory всегда верно 12ZY8Gap4PXu
			// Память может не выделится по одной причине: не хватило памяти, однако это - OutOfMemoryException, в т.ч. для Marshal.AllocHGlobal
			// inputTo может быть null только если в конструкторе произошло исключение и оно было проигнорировано, а ссылка на объект не была потеряна. Кажется, такого быть не может.
			// Следовательно, inputTo никогда не null :02amwFLcmkbN
            inputTo = AllocMemory(InputSize);
			
			// MCaZG5ctJub4.Не инициализирован
			// Состояние верно: массив создан, но не инициализирован
			// 1:Пустой - верно. Массив пустой (данные не вводились), см. AllocMemory
			// 2:Связанный - связанный с аллокатором this.allocator (см. AllocMemory)
			// 3:С аллокатором - см. AllocMemory
			// Указанные ниже состояния на выходе - верны
        }
		/*
		Используемые объекты и их состояния:
		inputTo
			MCaZG5ctJub4.Не инициализирован
				MCaZG5ctJub4.Ссылка на объект очищена
			1:Пустой
			2:Связанный
			3:С аллокатором
		allocator
			MCaZG5ctJub4.Готов к использованию
			MCaZG5ctJub4.Ссылка на объект очищена
		this.allocator
			MCaZG5ctJub4.Готов к использованию
		*/

		// Использование AllocMemory всегда верно (:12ZY8Gap4PXu), т.к. this.allocator не может быть null cRdzaP6t0ZtB
		// Все возвращаемые значения соответствуют
		// 1:Пустой - данные ещё не вводились
		// 2:Связанный - связан с this.allocator и соответствующим выделенным массивом
		// 3:С аллокатором - связан с this.allocator
        public Record AllocMemory(long len)
        {
			// allocator никогда не null, см. выше
            return allocator.AllocMemory(len);
        }

		/*
		Используемые объекты и их состояния:
		this.allocator
			MCaZG5ctJub4.Готов к использованию
		*/

        // TODO: сделать тесты на Clone
        public override Keccak_abstract Clone()
        {
			// Использование этого конструктора допустимо всегда.
			// Могут возникнуть исключения
            var result = new Keccak_PRNG_20201128();

            // Очищаем C и B, чтобы не копировать какие-то значения, которые не стоит копировать, да и хранить тоже
            clearOnly_C_and_B();

            // Копировать всё состояние не обязательно. Но здесь, для надёжности, копируется всё (в т.ч. ранее очищенные нули)
			// State всегда проинициализирован
            for (int i = 0; i < State.LongLength; i++)
                    result.State[i] = State[i];
				
			// State - не изменяется, кроме того, что C и B - очищены (это не влияет на криптографическое состояние).
			// Верно

			// Полная ли копия объекта произведена?
			// Копирование 

			// allocator - не произведено (найдены ошибки: исправлено)
			// allocatorForSaveBytes - не произведено
			// INPUT - подразумевается, что не нужно производить
			// inputTo - не копируется, потому что не нужно (найдена ошибка: добавлено в описание)
			// inputReady - не копируется, так как не копируется inputTo
			// output - не копируется, потому что не нужно
			// State - произведено


			// result
			// 1:Равен состоянию объекта this: либо Полный, либо Очищен
			// Верно и для групп 2 и 3, т.к. изменения именно в State.
			// 4:Данные не готовы
			// Верно. Данные не готовы, т.к. ничего не копировалось
			// 5:Данные на вход не готовы
			// Аналогично группе 4
            return result;
        }

		/*
		Используемые объекты и их состояния:
		result
			(состояние на выходе из функции)
			1:Равен состоянию объекта this: либо Полный, либо Очищен
			2:Равен состоянию объекта this: либо Введён вектор инициализации, либо Очищен
			3:Равен состоянию объекта this: либо Введён ключ, либо Очищен
			4:Данные не готовы
			5:Данные на вход не готовы
		State
			Не изменяется
			state.Blong
				Очищены
			state.Clong
				Очищены
		*/

        /// <summary>Сюда можно добавлять байты для ввода</summary>
        protected readonly BytesBuilderForPointers INPUT = new BytesBuilderForPointers(); // Не забыт ли вызов InputBytesImmediately при добавлении сюда?
        public    const    int InputSize = 64;

        /// <summary>Это массив для немедленного введения в Sponge на следующем шаге</summary>
		// inputTo никогда не null 02amwFLcmkbN
        protected readonly Record inputTo;
        /// <summary>Если <see langword="true"/>, то в массиве inputTo ожидают данные. Можно вызывать calStep</summary>
        protected          bool   inputReady   = false;
        /// <summary>Если <see langword="true"/>, то в массиве inputTo ожидают данные. Можно вызывать calStep</summary>
        public             bool   isInputReady => inputReady;


        public readonly BytesBuilderForPointers output = new BytesBuilderForPointers();

        /// <summary>Количество элементов, которые доступны для вывода без применения криптографических операций</summary>
        public long outputCount => output.Count;

        /// <summary>Ввести рандомизирующие байты (в том числе, открытый вектор инициализации). Не выполняет криптографических операций</summary>
        /// <param name="bytesToInput">Рандомизирующие байты. Копируются. bytesToInput должны быть очищены вручную</param>
        public void InputBytes(byte[] bytesToInput)
        {
			// bytesToInput не должен быть равен нулю.
			// Вставил проверки в код
			// В процессе работы программы он не изменяется, т.к. CloneBytes всегда копирует объект
            INPUT.add(BytesBuilderForPointers.CloneBytes(bytesToInput, allocator));
			// Объект, пришедший из CloneBytes очистится в InputBytesImmediately, когда его будут изымать
			// Если объект не изымут, он будет очищен в функции Clear
			// allocator будет скопирован в список, однако это ничего не значит, т.к. он не содержит данных, не изменяется и находится под сборщиком мусора

			// InputBytesImmediately может вызываться в любом состоянии, кроме 1:Уничтожен
			// Вставил в InputBytesImmediately проверку на такое состояние: будет исключение, если это неверно
            InputBytesImmediately();
        }

		/*
		Используемые объекты и их состояния:
		bytesToInput
			MCaZG5ctJub4.Готов к использованию
		Объект без имени, создающийся CloneBytes
			MCaZG5ctJub4.Готов к использованию
		this.INPUT
			MCaZG5ctJub4.Готов к использованию
		allocator
			MCaZG5ctJub4.Готов к использованию
		Объекты из InputBytesImmediately
			// INPUT
			inputTo (возвращаемое значение getBytesAndRemoveIt всегда совпадает с inputTo)
				MCaZG5ctJub4.Готов к использованию
				1:Любое, кроме Уничтожен
				2:Связанный или Не связанный
				3:Любое
					_
			inputReady
				Любое
		*/

        /// <summary>Ввести рандомизирующие байты (в том числе, открытый вектор инициализации). Не выполняет криптографических операций</summary>
        /// <param name="bytesToInput">Рандомизирующие байты. Копируются. bytesToInput должны быть очищены вручную</param>
        /// <param name="len">Длина рандомизирующей последовательности</param>
        public void InputBytes(byte * bytesToInput, long len)
        {
			// Аналогично функции выше. Разницы нет.
            INPUT.add(BytesBuilderForPointers.CloneBytes(bytesToInput, 0, len, allocator));
            InputBytesImmediately();
        }

		/*
		Используемые объекты и их состояния:
		bytesToInput
			MCaZG5ctJub4.Готов к использованию
		INPUT
			MCaZG5ctJub4.Готов к использованию
		безымянный объект, созданный CloneBytes
			MCaZG5ctJub4.Готов к использованию
		allocator
			MCaZG5ctJub4.Готов к использованию

		Объекты из InputBytesImmediately
			//INPUT
			inputTo (возвращаемое значение getBytesAndRemoveIt всегда совпадает с inputTo)
				MCaZG5ctJub4.Готов к использованию
				1:Любое, кроме Уничтожен
				2:Связанный или Не связанный
				3:Любое
			inputReady
				Любое
		*/

        /// <summary>Ввести рандомищирующие байты. Не выполняет криптографических операций.</summary>
        /// 
        /// 
        /// <param name="data"></param>
        public void InputBytesWithoutClone(Record data)
        {
			// Аналогично функции выше
			// Data будет очищена автоматически. добавил комментарий насчёт этого
            INPUT.add(data);
            InputBytesImmediately();
        }
		/*
		Используемые объекты и их состояния:
		data
			1:Полный
			2:Связанный или 2:Не связанный
			3:Любой
		INPUT
			MCaZG5ctJub4.Готов к использованию

		Объекты из InputBytesImmediately
			INPUT
			inputTo (возвращаемое значение getBytesAndRemoveIt всегда совпадает с inputTo)
				1:Любое, кроме Уничтожен
				2:Связанный или Не связанный
				3:Любое
			inputReady
		*/

        /// <summary>Ввести секретный ключ и ОВИ (вместе с криптографическим преобразованием)</summary>
        /// <param name="key">Ключ</param> // добавлено про очистку
        /// <param name="OIV">Открытый вектор инициализации, не более InputSize (не более 64 байтов). Может быть null</param>
        public void InputKeyAndStep(byte * key, long key_length, byte * OIV, long OIV_length)
        {
			// Если this уничтожен, то INPUT == null, то есть случится исключение
			// Если countOfBlocks > 0  - это проверка на то, что ничего не ожидает ввода
			// Если что-то ожидает ввода до ключа, это всегда вопрос к этому вводу: что это?
			// Плюс, можно вводить кратно InputSize, тогда ввода не будет ожидать
			// В общем, ожидание ввода здесь быть не должно
            if (INPUT.countOfBlocks > 0)
                throw new ArgumentException("key must be input before the generation or input an initialization vector (or see InputKeyAndStep code)", "key");

			// InputSize == 64, это размер блока. OIV не должен быть выше размера вводимого блока
			// В целом, этого вполне достаточно. То есть проверка не приводит к ложному срабатыванию
			// OIV_length может быть ниже нуля, но на это проверки нет. Добавил проверку ниже
            if (OIV_length > InputSize)
                throw new ArgumentException("Keccak_PRNG_20201128.InputKeyAndStep: OIV_length > InputSize", "OIV");

			// INPUT не равен нулю (если равен, уже выше было бы исключение).
			// INPUT инициализируется в конструкторе (инициализатор поля)
			// Проверка на key != null и его длину добавлена
			// Работать должно всегда
            INPUT.add(key, key_length);
			// Это можно вызывать в любом состоянии. Если "Уничтожен", то будет выдано исключение
			// Это вводит данные непосредственно в inputTo
            InputBytesImmediately();
            do
            {
				// Сейчас inputReady должен быть true
				// На всякий случай в InputBytesImmediately выше был добавлен true
                calcStep(Overwrite: false);
				// Ещё довыбираем байты. true для того, чтобы довыбрать некратные 64-ём байты
                InputBytesImmediately(true);
            }
            while (inputReady);	// Пока ещё есть

            // Завершаем ввод ключа конструкцией Overwrite, которая даёт некую необратимость состояния в отношении ключа
            if (OIV != null)
            {	// OIV точно будет введён за один шаг. Т.к. он не более InputSize и InputBytesImmediately(true) до этого выдал inputReady = false, а это значит, что INPUT.Count == 0
				// Исправлена ошибка в InputBytesImmediately (не было проверки на Count > 0, всегда устанавливался inputReady)
				// OIV должен быть проинициализирован. Проверка выше.
				// Добавлена проверка на то, что OIV_length > 0
                INPUT.add(OIV, OIV_length);
                InputBytesImmediately(true);
                calcStep(Overwrite: true);          // xor, к тому же, даёт больше ПЭМИН, так что просто Overwrite, хотя особо смысла в этом нет, т.к. xor в других операциях тоже идёт (но не с ключевой информацией)
            }
            else
            {
				// Вводим нули
                inputTo.Clear();
                inputReady = true;
                calcStep(Overwrite: true);
            }

            if (INPUT.countOfBlocks > 0)
            {
                INPUT.clear();
                Clear(true);
                throw new ArgumentException("key must be a multiple of 64 bytes", "key");
            }
        }
		/*
		Используемые объекты и их состояния:
		key
			MCaZG5ctJub4.Не создан (null)
			MCaZG5ctJub4.Готов к использованию
		OIV
			MCaZG5ctJub4.Не создан (null)
			MCaZG5ctJub4.Готов к использованию
		INPUT
			MCaZG5ctJub4.Готов к использованию
		inputReady
			false
				false
				true
		inputTo
			1:Любое, кроме Уничтожен
			2:Связанный или Не связанный
			3:Любое
		Объекты из InputBytesImmediately
			INPUT
			inputTo (возвращаемое значение getBytesAndRemoveIt всегда совпадает с inputTo)
			inputReady
		*/
// После проверки, в вызове Clear добавлены вопросы: "?."
        public override void Clear(bool GcCollect = false)
        {
			// Добавлены ?. вместо .
            inputTo.Clear();

            INPUT  .clear();
            output .clear();

            inputReady = false;

            base.Clear(GcCollect);
        }
		/*
		Используемые объекты и их состояния:
		INPUT
			MCaZG5ctJub4.Очищен
				MCaZG5ctJub4.Очищен
				MCaZG5ctJub4.Готов к использованию
		inputReady
		inputTo
			1:Очищен
				1:Любое, кроме Уничтожен
			2:Связанный или Не связанный
			3:Любое

		output
			MCaZG5ctJub4.Очищен
			Объект может находится в копии по ссылке у пользователя

		Объекты, используемые в base.Clear:
			State
		*/

        public override void Dispose(bool disposing)
        {
            inputTo?.Dispose();
			inputTo = null;		// Это уже добавлено в ходе проверки
            base.Dispose(disposing);        // Clear вызывается здесь
        }
		/*
		Используемые объекты и их состояния:
		inputTo
			MCaZG5ctJub4.Ссылка на объект очищена
			1:Уничтожен
				1:Любое
			2:Освобождён
				2:Связанный или Не связанный
			3:Без аллокатора
				3:Любое
		Объекты, используемые в base.Clear (Dispose лишь вызывает Clear):
			State
				MCaZG5ctJub4.Очищен
					MCaZG5ctJub4.Очищен
					MCaZG5ctJub4.Готов к использованию
		*/

        /// <summary>Переносит байты из очереди ожидания в массив байтов для непосредственного ввода в криптографическое состояние. Не выполняет криптографических операций</summary>
        protected void InputBytesImmediately(bool ForOverwrite = false)
        {// Функция изменена после проверки других функций
            if (!inputReady)
            {
                if (INPUT.Count >= InputSize)
                {
                    // TODO: сделать тесты на верность getBytesAndRemoveIt и, по возможности, на его использование
                    INPUT.getBytesAndRemoveIt(inputTo);
                    inputReady = true;
                }
                else
                if (ForOverwrite)
                {
                    inputTo.Clear();
                    INPUT.getBytesAndRemoveIt(inputTo);
                    inputReady = true;
                }
            }
        }

		/*
		Используемые объекты и их состояния:
		this
			1:Любое, кроме Уничтожен
		INPUT
		inputTo (возвращаемое значение getBytesAndRemoveIt всегда совпадает с inputTo)
			1:Любое, кроме Уничтожен
			2:Связанный или Не связанный
			3:Любое
		inputReady
			Любое
				Любое
		*/

        /// <summary>Выполняет шаг keccak и сохраняет полученный результат в output</summary>
        public void calcStepAndSaveBytes()
        { // Немного изменено (добавлен проходящий ниже параметр inputReadyCheck)
            calcStep(SaveBytes: true);
        }

		/*
		Используемые объекты и их состояния:
		те же, что и в calcStep. Проверка, видимо, идентична calcStep
		*/

        /// <summary>Расчитывает шаг губки keccak. Если есть InputSize (64) байта для ввода (точнее, inputReady == true), то вводит первые 64-ре байта</summary>
        /// <param name="SaveBytes">Если <see langword="null"/>, выход не сохраняется</param>
        /// <param name="Overwrite">Если <see langword="true"/>, то вместо xor применяет перезапись внешней части состояния на вводе данных (конструкция Overwrite)</param>
        // TODO: Разобраться с тем, что состояние не зафиксировано в памяти, а может перемещаться
        public void calcStep(bool SaveBytes = false, bool Overwrite = false)
        {// Функция была изменена в ходе проверки, в частности, добавлен контроль inputReadyCheck
            Keccak_abstract.KeccakStatesArray.getStatesArray(out GCHandle handle, this.State, out byte * S, out byte * B, out byte * C, out byte * Base, out ulong * Slong, out ulong * Blong, out ulong * Clong);
            try
            {
                // InputBytesImmediately();    // Это на всякий случай добавлено
                if (inputReady)
                {
					// input не нужно обнулять далее
					// input всегда создан, если создан inputTo (а он создаётся в конструкторе)
					// inputTo обнуляется только в в Dispose
                    byte * input = inputTo.array;

					// Ввод данных верен. Вводится массив input
					// Всегда вводится блок 64 байта. Если меньше, то он просто выравнивается нулями без различающих paddings
                    if (Overwrite)
                        Keccak_InputOverwrite64_512(message: input, len: InputSize, S: S);
                    else
                        Keccak_Input_512(message: input, len: InputSize, S: S);

					// Мы использовали ввод, так что нам теперь не нужно, поэтому inputReady сбрасываем
					// Добавил обнуление inputTo
                    inputReady = false;
					// Подготавливаем на следующий раз данные, если есть
                    InputBytesImmediately();
                }

				// Вызываем функцию преобразования. Она всегда может быть вызвана, если объект не уничтожен
				// Добавил проверку на State != null в начале функции
                Keccackf(a: Slong, c: Clong, b: Blong);

			// Если нужно сохранить байты, сохраняем
                if (SaveBytes)
                {
					// Это всегда создано. А если не создано, будет исключение
					// Всегда создаётся InputSize, так и должно быть (хотя пользователь мог бы и поднастроить меньшее количество)
                    var result = allocatorForSaveBytes.AllocMemory(InputSize);
					// result только что был создан, array тоже должен быть всегда создан (или будет OutOfMemory)
                    Keccak_Output_512(output: result.array, len: InputSize, S: S);

					// output создаётся при создании объекта и никогда не null
					// add всегда можно вызвать, объект не копируется, то есть будет уничтожен
                    output.add(result);
                }
            }
            finally
            {
                Keccak_abstract.KeccakStatesArray.handleFree(handle);
            }
        }

		/*
		Используемые объекты и их состояния:
		handle и полученные от неё S, B, C, Slong, Clong, Blong и т.п.
			Состояния меняются по ходу функции
			MCaZG5ctJub4.Очищен
			MCaZG5ctJub4.Уничтожен
		State
			MCaZG5ctJub4.Готов к использованию
		inputReady
			Любой
				Любой
		inputTo
			1:Если inputReady, то "Полный", иначе любое, кроме "Уничтожен"
			2:Связанный или Не связанный
			3:Любое
			Происходит копирование: проверить по cFNNZfO63pYL
		input
			-
		result
			-
		output
			-
		*/

        /// <summary>Выдаёт случайные криптостойкие значения байтов. Выгодно использовать при большом количестве байтов (64 и более). Выполняет криптографические операции, если байтов не хватает</summary>
        /// <param name="output">Массив, в который записывается результат</param>
        /// <param name="len">Количество байтов, которые необходимо записать. Используйте outputCount, чтобы узнать, сколько байтов уже готово к выводу (без выполнения криптографических операций)</param>
        public void getBytes(Record outputRecord, long len)
        {
			// Если что, будет просто доступ по null
			// output не нужно очищать: это приёмник результата
            var output = outputRecord.array;

            // Проверяем уже готовые байты
			// Если они есть, их нужно отработать
			// this.output есть всегда, он создан в инициализаторе поля объекта
            if (this.output.Count > 0)
            {
				// Выводим то, что доступно. 
                var readyLen = this.output.Count;
				// Если readyLen > len, то нам столько байтов всё равно не нужно. Поэтому уменьшаем их количество для упрощения
                if (readyLen > len)
                {
                    readyLen = len;
                }

				// Выделяем память, и сразу же её удаляем в том же блоке.
				// getBytesAndRemoveIt либо выделит память сам, либо выделим мы, так что да, смысл есть
				// Память сама очистится и освободится при автоматическом вызове Dispose
                using var b = this.output.getBytesAndRemoveIt(  AllocMemory(readyLen)  );

				// Что, если данных для b не хватило?
				// Так не может быть, ведь readyLen = this.output.Count
				// Именно из этого объекта мы берём данные
				// Так что, если он нормально работает, байтов всегда должно хватить для полного заполнения b

				// Байты копируются в массив результата: из b в output, являющийся полем outputRecord
				// Все объекты созданы
				// Копируется b.len байтов источника, то есть размер b
				// В readyLen байтов приёмника. Единственная проблема, если len > output. На это проверки нет. Надо сделать
                BytesBuilder.CopyTo(b.len, readyLen, b.array, output);

				// Сколько байтов вывели, настолько приращаем output
				// Чтобы затем писать уже после этих байтов
				// если же не выводили байтов, то output останется прежним. Всё верно
                output += readyLen;
				// Уменьшаем количество данных, которое нам нужно было вывести
                len    -= readyLen;

                if (len <= 0)
                    return;
            }

            // Если готовых байтов нет, то начинаем вычислять те, что ещё не готовы
            // И сразу же их записываем
            Keccak_abstract.KeccakStatesArray.getStatesArray(out GCHandle handle, this.State, out byte * S, out _, out _, out _, out _, out _, out _);
            try
            {
				// Верно, пока потребность больше нуля
                while (len > 0)
                {	
					// Перед calcStep можно было бы вызвать InputBytesImmediately
					// Не сохраняем результат, потому что мы этот результат берём далее вручную
                    calcStep();
					// Можно вызвать всегда
					// Мы уже проверили, что длина output всегда не менее len. Так что мы не будем записывать во-вне массива
                    Keccak_Output_512(output: output, len: (byte) (len >= 64 ? 64 : len), S: S);
                    len    -= 64;	// здесь потребность может стать отрицательной, но нам не имеет смысла уточнять
                    output += 64;
                }
            }
            finally
            {
                Keccak_abstract.KeccakStatesArray.handleFree(handle);
            }
        }
		/*
		Используемые объекты и их состояния:
		outputRecord
			1:Любое, кроме Уничтожен
			2:Любое
			3:Любое
		output
			-
		this.output
			MCaZG5ctJub4.Готов к использованию
			Может быть в копии у программиста-пользователя
		readyLen
			-
		len
			Некорректное состояние внутри функции (не меняется в вызывающей функции)
				Корректное состояние
		b = getBytesAndRemoveIt(AllocMemory(readyLen) (он же безымянный, выделенный AllocMemory)
			-
		handle и S
			-
		*/
        
        public byte getByte()
        {
            if (this.output.Count <= 0)
            {
				// После проверки добавлен вызов InputBytesImmediately
                calcStepAndSaveBytes();
            }

			// Не очень эффективно, но верно
			// Изменил на stackalloc
            using var b = output.getBytesAndRemoveIt(  AllocMemory(1)  );

            var result = b.array[0];

            return result;
        }
		/*
		Используемые объекты и их состояния:
		output
			MCaZG5ctJub4.Готов к использованию
			Может быть в копии у программиста-пользователя
		calcStepAndSaveBytes: см. выше
		b = AllocMemory(1)
			-
		*/

        /// <summary>Выдаёт случайное криптостойкое число от 0 до cutoff включительно. Это вспомогательная функция для основной функции генерации случайных чисел</summary>
        /// <param name="cutoff">Максимальное число (включительно) для генерации. cutoff должен быть близок к ulong.MaxValue или к 0x8000_0000__0000_0000U, иначе неопределённая отсрочка будет очень долгой</param>
        /// <returns>Случайное число в диапазоне [0; cutoff]</returns>
        public ulong getUnsignedInteger(ulong cutoff = ulong.MaxValue, Record arrayAt8Length = null)
        {
			// b не равно нулю, всегда создано
			// b связано с внешним объектом arrayAt8Length
            var b = arrayAt8Length ?? AllocMemory(8);
			// На случай проблем с длиной arrayAt8Length, вставил проверку
            try
            {
                while (true)
                {
                    if (this.output.Count < 8)
                    {
                        calcStepAndSaveBytes();
                    }

					// Собираем байты сюда
                    output.getBytesAndRemoveIt(b);

                    BytesBuilderForPointers.BytesToULong(out ulong result, b.array, 0, b.len);

                    if (cutoff < 0x8000_0000__0000_0000U)
                        result &= 0x7FFF_FFFF__FFFF_FFFFU;  // Сбрасываем старший бит, т.к. он не нужен никогда

                    if (result <= cutoff)
                        return result;
                }
            }
            finally
            {
				// Это, вроде бы, верно. arrayAt8Length никогда не обнуляется в этой подпрограмме
                if (arrayAt8Length == null)
                    b.Dispose();
				// Добавил b.Clear() в ином случае
            }
            
        }
		/*
		Используемые объекты и их состояния:
		output
			MCaZG5ctJub4.Готов к использованию
			Может быть в копии у программиста-пользователя
		arrayAt8Length
			MCaZG5ctJub4.Ссылка на объект очищена
			MCaZG5ctJub4.Готов к использованию
				1:Любое, кроме Уничтожен

		b = arrayAt8Length ?? AllocMemory(8)
			-
		*/

        /// <summary>Получает случайное значение в диапазоне, указанном в функции getCutoffForUnsignedInteger</summary>
        /// <param name="min">Минимальное значение</param>
        /// <param name="cutoff">Результат функции getCutoffForUnsignedInteger</param>
        /// <param name="range">Результат функции getCutoffForUnsignedInteger</param>
        /// <returns>Случайное число в указанном диапазоне</returns>
        public ulong getUnsignedInteger(ulong min, ulong cutoff, ulong range, Record arrayAt8Length = null)
        {
            var random = getUnsignedInteger(cutoff, arrayAt8Length) % range;

            return random + min;
        }
		/*
		Используемые объекты и их состояния:
		arrayAt8Length
			MCaZG5ctJub4.Ссылка на объект очищена
			MCaZG5ctJub4.Готов к использованию
				1:Любое, кроме Уничтожен
		*/

        /// <summary>Вычисляет параметры для применения в getUnsignedInteger</summary>
        /// <param name="min">Минимальное значение для генерации</param>
        /// <param name="max">Максимальное значнеие для генерации (включительно)</param>
        /// <param name="cutoff">Параметр cutoff для передачи getUnsignedInteger</param>
        // TODO: хорошо протестировать
        public void getCutoffForUnsignedInteger(ulong min, ulong max, out ulong cutoff, out ulong range)
        {
            range = max - min + 1;

            if (range >= 0x8000_0000__0000_0000U)
            {
                cutoff = range;
                return;
            }

            var mod = (0x8000_0000__0000_0000U) % range;

            if (mod == 0)
            {
                cutoff = 0x8000_0000__0000_0000U;
                return;
            }

            var result = 0x8000_0000__0000_0000U - mod;

            if (result % range != 0)
                throw new Exception("Fatal error: Keccak_PRNG_20201128.getCutoffForUnsignedInteger");

            cutoff = result;
        }
		/*
		Используемые объекты и их состояния:
		Нет объектов, подлежащих контролю
		Исправлена ошибка: метод сделан статическим
		*/

        /// <summary>Осуществляет перестановки таблицы 2-хбайтовых целых чисел</summary>
        /// <param name="table">Исходная таблица для перестановок длиной не более int.MaxValue</param>
        public void doRandomPermutationForUShorts(ushort[] table)
        {
			// Если table == null, то всё сразу вылетит, что хорошо
            // Иначе всё равно будет слишком долго
            if (table.LongLength > int.MaxValue)
                throw new ArgumentException("doRandomCubicPermutationForUShorts: table is very long");
            if (table.Length <= 3)
                throw new ArgumentException("doRandomCubicPermutationForUShorts: table is very short");

            var len = (ulong) table.LongLength;

            // Алгоритм тасования Дурштенфельда
            // https://ru.wikipedia.org/wiki/Тасование_Фишера_—_Йетса
			// Здесь будет всё удалено
            using var b8 = allocator.AllocMemory(8);
            for (ulong i = 0; i < len - 1; i++)
            {
                getCutoffForUnsignedInteger(0, (ulong) len - i - 1, out ulong cutoff, out ulong range);
                var index = getUnsignedInteger(0, cutoff, range, b8) + i;

                do2Permutation(i, index);
            }

			// Здесь нет проблем
            void do2Permutation(ulong i1, ulong i2)
            {
                var a     = table[i1];
                table[i1] = table[i2];
                table[i2] = a;
            }
            /*
            void do3Permutation(int i1, int i2, int i3)
            {
                var a1    = table[i1];
                var a2    = table[i2];
                var a3    = table[i3];

                table[i1] = a2;
                table[i2] = a3;
                table[i3] = a1;
            }*/
        }
		/*
		Используемые объекты и их состояния:
		table
			MCaZG5ctJub4.Готов к использованию
		allocator
			MCaZG5ctJub4.Готов к использованию
		b8
			-
		*/
    }
}
