using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace The_DES_cipher.DES
{
    public class Des
    {
        public event EventHandler<int> BlockProcess;
        public event EventHandler<int> Start;
        public event EventHandler<Exception> Stop;
        public Mode Mode { get; set; }          // свойство режима шифрования
        public Encoding Encoding { get; set; }  // свойство кодировки открытого текста
        public string Key { get; set; }         // свойство, хранящее ключ шифрования
        public string IV { get; set; }          // свойство, хранящее вектор инициализации (используется при Mode = Mode.CBC)
        private byte[] _key { get; set; }       // закрытое свойство ключа, переведенного из строки в массив байт
        private byte[] _iv { get; set; }        // закрытое свойство вектора инициализации, переведенного из строки в массив байт
        public List<RoundKey> RoundKeys { get => _roundKeys.ToList(); } // свойство, представляющее механизм инкапсуляции для приватного свойства _roundKeys
        public List<Block> Blocks { get => _blocksTable.ToList(); }     // свойство, представляющее механизм инкапсуляции для приватного свойства _blocksTable
        private RoundKey[] _roundKeys { get; set; } // свойство, хранящее в себе полную информацию о раундовых ключах
        private Block[] _blocksTable { get; set; }  // свойство, хранящее в себе полную информацию о блоках
        private byte[][] _blocks;                   // зубчатый массив для хранения исходных блоков входных данных

        public Des()                    // конструктор класса Des, нужен для начальной инициализации класса
        {
            Mode = Mode.ECB;            // инициализация режима шифрования
            Encoding = Encoding.ASCII;  // инициализация кодировки текста
        }

        private byte[] Permutation(byte[] data, int[] table)    // метод, представляющий собой механизм перестановки бит data, соответственно таблице table
        {
            byte[] output = new byte[table.Length];             // заготовка для заполнения бит из массива data по таблице table
            for (int i = 0; i < table.Length; i++)              // цикл, который "пробегается" по таблице
            {
                output[i] = data[table[i] - 1];                 // заполнение i позиции заготовки, данные из таблицы представляют индекс числа в массиве data
            }
            return output;                                      // возвращает заготовку output как результат перестановки по таблице table
        }
        private void GenerateKeys() // метод, генерирующий раундовые ключи
        {
            _roundKeys = new RoundKey[16];                      // задает размерность массива раундовых ключей
            byte[] keyPc1 = Permutation(_key, Tables.PC1);      // первоначальная перестановка по функции PC1
            byte[] roundkey = keyPc1;                           // переменная для хранения раундового ключа
            byte[] C = new byte[28];                            // переменная для хранения половины C
            byte[] D = new byte[28];                            // переменная для хранения половины D
            for (int i = 0; i < 16; i++)                        // цикл, представляющий механизм раундов (итераций)
            {
                _roundKeys[i] = new RoundKey                    // создает новый объект RoundKey, предсставляющий подробные данные о генерации ключа
                {
                    Round = i + 1,                              // номер раунда
                    PC1 = keyPc1.ToArray(),                     // результат перестановки PC1
                };
                Array.Copy(roundkey, 0, C, 0, C.Length);        // все последующие Array.Copy(arr1, index1, arr2, index2, length)
                                                                // копируют данные из массива arr1 начиная с индекса index1
                                                                // в массив arr2, начиная с индекса index2 и длиной length,
                                                                // является системным методом и не нуждается в множественном объяснении
                Array.Copy(roundkey, 28, D, 0, D.Length);
                _roundKeys[i].C0 = C.ToArray();                 // запись в объект с ключами данных о Ci-1, .ToArray() используется потому, что данные
                                                                // в .NET имеют ссылочный тип, и если постоянно записвать
                                                                // C без .ToArray() для каждой итерации, то абсолютно
                                                                // во всех ключах могут быть однаковые данные, чего быть не должно
                _roundKeys[i].D0 = D.ToArray();                 // запись в объект с ключами данных о Di-1
                C = BitExtensions.LeftShift(C, Tables.Shift[i]);// циклический сдвиг влево на основании таблицы сдвига в зависимотси от раунда
                D = BitExtensions.LeftShift(D, Tables.Shift[i]);
                _roundKeys[i].C = C.ToArray();                  // запись в объект с ключами данных о Ci
                _roundKeys[i].D = D.ToArray();                  // запись в объект с ключами данных о Di
                Array.Copy(C, 0, roundkey, 0, C.Length);
                Array.Copy(D, 0, roundkey, C.Length, D.Length);
                _roundKeys[i].PrePC2 = roundkey.ToArray();              // запись в объект с ключами данных о CiDi. прямо перед перестановкой PC2
                _roundKeys[i].Key = Permutation(roundkey, Tables.PC2);  // запись в объект с ключами данных о раундовом ключе,
                                                                        // раундовый ключ является результатом перестановки по таблице PC2
            }
        }
        private byte[] Sbox(byte[] sboxCellInput, int[,] sbox)  // метод, возвращающий результат операции по S блоку sbox
        {
            byte[] rowInBits = { sboxCellInput[0], sboxCellInput[5] };  // представляет значение строки в двоичном двухбитном формате
            byte[] colInBits = { sboxCellInput[1], sboxCellInput[2], sboxCellInput[3], sboxCellInput[4] };  // представляет значение столбца в двоичном четырехбитном формате
            byte[] rowInByte = BitExtensions.BitsToByte(rowInBits);                  // конвертирует массив бит в байты
            byte[] colInByte = BitExtensions.BitsToByte(colInBits);
            int row = (int)(Convert.ToDecimal(rowInByte[0]));           // получает числовое значение строки для навигации по S блоку
            int col = (int)(Convert.ToDecimal(colInByte[0]));           // получает числовое значение столбца для навигации по S блоку
            int temp = sbox[row, col];                                  // хранит временное значение числа из S блока
            byte[] b = BitExtensions.IntToBytes(temp);                               // конвертирует число в массив байт
            byte[] sboxCellOutput = new byte[4];
            Array.Copy(BitExtensions.ByteToBits(b).ToArray(), 28, sboxCellOutput, 0, 4);
            return sboxCellOutput;                                      // возвращает B'i
        }
        private void SetBlocksOfPlainText(byte[] plain) // метод, разбивающий открытый текст на блоки
        {
            int numOfBlocks = plain.Length / 64;    // вычисляет количество блоков, путем деления на разменость блока
            numOfBlocks = (plain.Length % 64 != 0) ? ++numOfBlocks : numOfBlocks;   // дополнительная проверка,
                                                                                    // если общая длина шифртекста не делится на 64 без остатка,
                                                                                    // то к количеству блоков добавляется еще 1 блок
            _blocks = new byte[numOfBlocks][];      // задает количество массивов byte[] зубчатому массиву, хранящему блоки исходного текста
            byte[] block = new byte[64];            // создает массив, представляющий блок
            int i, j = 0, count = 0;                // j - количество блоков, count - счетчик заполненых битов в блоке
            for (i = 0; i < plain.Length && j < numOfBlocks; i++)   //цикл для прохода по всем элементам открытого текста
            {
                if (count == 63)                // если count равен 63, занчит текущий блок закончился, и нужно создавать новый
                {
                    block[count] = plain[i];    // записывает 64-й бит блока
                    _blocks[j] = block;         // сохраняет заполненный блок в массив блоков
                    block = new byte[64];       // создает новый блок
                    j++;                        // прибавляет переменной j еденицу
                    count = 0;                  // сбрасывает счетчик
                }
                else                            // иначе заполняет незаполненный блок
                {
                    block[count] = plain[i];    // заполняет блок
                    ++count;                    // прибавляет к счетчику еденицу
                    if (i == plain.Length - 1)  // если достигнет конец открытого текста, то все равно сохраняем блок в массив блоков
                        _blocks[j] = block;
                }
            }
            if (count != 0)                     // если значение счетчика небыло сброшено,
                                                // то есть размер последнего блока не равен 64 бит, то
            {
                for (int k = count; k < 64; k++)
                {
                    _blocks[j][k] = 0;          // заполняет оставшиеся биты нулями
                }
            }
        }
        private void SetBlocksOfencyptedMessage(byte[] ciphed)  // метод, разбивающий шифр текст на блоки
        {
            int numOfBlock = ciphed.Length / 8;                 // вычисляет количество блоков
            _blocks = new byte[numOfBlock][];
            byte[] temp = new byte[8];                          // массив, используемый для заполнения каждого блока
            for (int i = 0; i < numOfBlock; i++)                // цикл для прохождения по каждому блоку
            {
                for (int j = 0; j < temp.Length; j++)           // цикл проходится по всем байтам в текущем блоке
                {
                    temp[j] = (byte)ciphed[j + (i * 8)];        // каждый байт шифр текста копируется в массив temp
                }
                Array.Reverse(temp);                            // массив переворачивается, это нужно для правильного отображения массива бит
                _blocks[i] = (BitExtensions.ByteToBits(temp)).ToArray();     // блок шифр текста в двоичном коде записывается в массив блоков
            }
        }
        public byte[] Encryption(byte[] plainText)  // метод, шифрующий открытый текст
        {
            _key = BitExtensions.StringToBits(Key, Encoding.ASCII);    // ключ конвертируется из строки в массив байт
            if (Mode == Mode.CBC)                       // проверка, используется ли режим шифрования CBC
            {
                _iv = BitExtensions.StringToBits(IV, Encoding);        // если используется, то конвертируется вектор инициализации
            }
            GenerateKeys();                             // генерируются раундовые ключи
            SetBlocksOfPlainText(plainText);            // открытый текст разбивается на блоки
            _blocksTable = new Block[_blocks.GetLength(0)]; // создается список делатьной информации о каждом блоке
            byte[] IP;                                  // переменная для хранения результата перестановки IP
            byte[] L = new byte[32];                    // переменная для хранения левой половины
            byte[] R = new byte[32];                    // переменная для хранения правой половины
            byte[] EP;                                  // переменная для хранения результата перестановки E/P
            byte[] xorRoundKey;                         // переменная для хранения результата XOR(E/P, Ki)
            byte[] SBox = new byte[32];                 // переменная для хранения результата перестановки SBox
            byte[] SBoxCells = new byte[6];             // переменная для хранения бит Bj, чтобы передавать их в Sj(Bj)
            byte[] SBoxPermutation;                     // переменная для хранения результата перестановки P, также является результатом ƒ
            byte[] xorToL = null;                       // переменная для хранения результата XOR(Li, ƒ)
            byte[] LR = new byte[64];                   // переменная для хранения результата конкатенации R16 и L16
            byte[] LRpermutation = new byte[64];        // переменная для хранения реузльтата перестановки IP inverse
            byte[] temp = null;                         // временная переменная для хранения данных текущего блока
            byte[] output = null;                       // переменная для вывода шифр текста
            for (int i = 0; i < _blocks.GetLength(0); i++) // цикл, который "пробегается" по всем блокам
            {
                _blocksTable[i] = new Block()   // создает объект с детальной информацией о блоке в ходе его преобразования
                {
                    Source = _blocks[i].ToArray()   // записывает исходный блок открытого текста Ti,
                                                    // все последующие операции, связанные с массивом _blocksTable
                                                    // являются записью последовательности шифрования в объект с
                                                    // детальной информацией о блоке Block, и не будут комментироваться.
                };
                switch (Mode)                       // определяет действия для определенного режима шифрования
                {
                    case Mode.CBC:                  // если режим шифрования CBC, то:
                        if (i == 0)                     // если блок первый по счету, то
                            _blocks[i] = BitExtensions.XOR(_blocks[i], _iv);    // Ti = XOR(Ti, IV)
                        else                            // иначе
                        {
                            Array.Reverse(LRpermutation);
                            _blocks[i] = BitExtensions.XOR(_blocks[i], LRpermutation); // Ti = XOR(Ti, Ci-1), где Ci-1 - блок шифр текста, полученный на предыдущей итерации
                        }
                        _blocksTable[i].XorCBC = _blocks[i].ToArray();
                        break;
                }
                IP = Permutation(_blocks[i], Tables.IP);    // начальная перестановка бит IP
                _blocksTable[i].IP = IP.ToArray();          
                Array.Copy(IP, 0, L, 0, L.Length);
                Array.Copy(IP, L.Length, R, 0, R.Length);
                _blocksTable[i].L0 = L.ToArray();
                _blocksTable[i].R0 = R.ToArray();
                for (int j = 0; j < 16; j++)                // цикл, состоящий из 16 итераций преобразования по функции Фейстеля
                {
                    EP = Permutation(R, Tables.E);          // расширяющая перестановка E/P с 32 бит до 48 бит

                    _blocksTable[i].EP.Add(EP.ToArray());   // добавляет в список EP результат перестановки E/P,
                                                            // все последующие операции связанные с _blocksTable
                                                            // и имеющие .Add являются схожими с текущей и не будут комментироваться.

                    _blocksTable[i].RoundKey.Add(_roundKeys[j].Key.ToArray());
                    xorRoundKey = BitExtensions.XOR(EP, _roundKeys[j].Key); // XOR(E/P, Ki)
                    _blocksTable[i].XorToRoundKey.Add(xorRoundKey.ToArray());
                    for (int k = 0, l = 0; k < xorRoundKey.Length; k += 6, l++) // вычисление 6 бит для кажого Bj
                    {
                        Array.Copy(xorRoundKey, k, SBoxCells, 0, SBoxCells.Length);
                        byte[] b = Sbox(SBoxCells, Tables.SBox[l]); //выполнение функции Sj(Bj) для S блоков
                        Array.Copy(b, 0, SBox, 4 * l, 4);
                    }
                    _blocksTable[i].SBox.Add(SBox.ToArray());
                    SBoxPermutation = Permutation(SBox, Tables.P);  // перестановка P
                    _blocksTable[i].SBoxPermutation.Add(SBoxPermutation.ToArray());
                    xorToL = BitExtensions.XOR(SBoxPermutation, L); // XOR(Li, ƒ)
                    _blocksTable[i].XorToL.Add(xorToL.ToArray());
                    L = R;                                          // Li = Ri-1
                    R = xorToL;                                     // Ri = XOR(Li-1, ƒ)
                    _blocksTable[i].L.Add(L.ToArray());
                    _blocksTable[i].R.Add(R.ToArray());
                }
                R = L;                                  // R = L16 (половины меняются местами в соответствии с алгоритмом DES)
                L = xorToL;                             // L = R16
                _blocksTable[i].LEnd = L.ToArray();
                _blocksTable[i].REnd = R.ToArray();
                Array.Copy(L, 0, LR, 0, L.Length);
                Array.Copy(R, 0, LR, L.Length, R.Length);
                _blocksTable[i].LR = LR.ToArray();
                LRpermutation = Permutation(LR, Tables.IPInverse);  // конечная перестановка IP inverse, которая является инверсией функции перестановки IP
                _blocksTable[i].IPInverse = LRpermutation.ToArray();
                temp = BitExtensions.BitsToByte(LRpermutation);     // запись во временную переменную массива байт, конвертированного из массива бит
                Array.Reverse(temp);                                // временная переменная разворачивается,
                                                                    // так как двоичный код подразумевает чтение справа на лево,
                                                                    // а десятичные числа читаются слева на право.

                if (output == null)                                 // если массив для вывода пустой, то
                    output = temp;                                  // присваивает значение массива temp к output
                else                                                // иначе
                    output = output.Concat(temp).ToArray();         // соединяет уже существующие блоки с еще одним
            }
            return output;                                          // в конце итераций возвращает вывод
        }
        public byte[] Decryption(byte[] cipherText) // метод, расшифровывающий шифр текст
        {
            // этот метод почти ничем не отличается от метода шифрования, но все же отличия есть, они и будут прокоментированны

            Array.Reverse(cipherText);  // переворачивает шифр текст,
                                        // так как метод с разбиением блоков будет переводить данные в
                                        // двоичный формат, который чистается справа на лево.

            _key = BitExtensions.StringToBits(Key, Encoding.ASCII);
            if (Mode == Mode.CBC)
            {
                _iv = BitExtensions.StringToBits(IV, Encoding);
            }
            GenerateKeys();
            SetBlocksOfencyptedMessage(cipherText); // разбивает закрытый текст на блоки
            _blocksTable = new Block[_blocks.GetLength(0)];
            byte[] IP;
            byte[] L = new byte[32];
            byte[] R = new byte[32];
            byte[] EP;
            byte[] xorRoundKey;
            byte[] SBox = new byte[32];
            byte[] SBoxCells = new byte[6];
            byte[] SBoxPermutation;
            byte[] xorToL = null;
            byte[] LR = new byte[64];
            byte[] LRPermutation = new byte[64];
            byte[] temp;
            byte[] output = null;
            for (int i = _blocks.GetLength(0) - 1; i >= 0 ; i--)    // цикл в методе расшифрования следует обратному алгоритму,
                                                                    // соответственно блоки будут расшифровываться в обратном порядке
            {
                _blocksTable[i] = new Block()
                {
                    Source = _blocks[i].ToArray()
                };
                IP = Permutation(_blocks[i], Tables.IP);
                _blocksTable[i].IP = IP.ToArray();
                Array.Copy(IP, 0, L, 0, L.Length);
                Array.Copy(IP, L.Length, R, 0, R.Length);
                _blocksTable[i].L0 = L.ToArray();
                _blocksTable[i].R0 = R.ToArray();
                for (int j = 0; j < 16; j++)
                {
                    EP = Permutation(R, Tables.E);
                    _blocksTable[i].EP.Add(EP.ToArray());
                    _blocksTable[i].RoundKey.Add(_roundKeys[16 - j - 1].Key.ToArray());
                    xorRoundKey = BitExtensions.XOR(EP, _roundKeys[16 - j - 1].Key);    // так как расшифрование следует в обратном порядке,
                                                                                        // то раудовые ключи будут применяться в обратном порядке.
                    _blocksTable[i].XorToRoundKey.Add(xorRoundKey.ToArray());
                    for (int k = 0, l = 0; k < xorRoundKey.Length; k += 6, l++)
                    {
                        Array.Copy(xorRoundKey, k, SBoxCells, 0, SBoxCells.Length);
                        byte[] b = Sbox(SBoxCells, Tables.SBox[l]);
                        Array.Copy(b, 0, SBox, 4 * l, 4);
                    }
                    _blocksTable[i].SBox.Add(SBox.ToArray());
                    SBoxPermutation = Permutation(SBox, Tables.P);
                    _blocksTable[i].SBoxPermutation.Add(SBoxPermutation.ToArray());
                    xorToL = BitExtensions.XOR(SBoxPermutation, L);
                    _blocksTable[i].XorToL.Add(xorToL.ToArray());
                    L = R;
                    R = xorToL;
                    _blocksTable[i].L.Add(L.ToArray());
                    _blocksTable[i].R.Add(R.ToArray());
                }
                R = L;
                L = xorToL;
                _blocksTable[i].LEnd = L.ToArray();
                _blocksTable[i].REnd = R.ToArray();
                Array.Copy(L, 0, LR, 0, L.Length);
                Array.Copy(R, 0, LR, L.Length, R.Length);
                _blocksTable[i].LR = LR.ToArray();
                LRPermutation = Permutation(LR, Tables.IPInverse);
                _blocksTable[i].IPInverse = LRPermutation.ToArray();
                switch (Mode)
                {
                    case Mode.CBC: // проверка на режим шифрования CBC будет в конце, потому что Ti сцепляется с Ci-1
                        if (i == _blocks.GetLength(0) - 1)                                  // Если блок последний по счету, то
                            LRPermutation = BitExtensions.XOR(LRPermutation, _iv);          // Ti = XOR(Ti, IV)
                        else                                                                // Иначе
                            LRPermutation = BitExtensions.XOR(LRPermutation, _blocks[i+1]); // Ti = XOR(Ti, Ci-1)
                        _blocksTable[i].XorCBC = LRPermutation.ToArray();
                        break;
                }
                temp = BitExtensions.BitsToByte(LRPermutation);
                Array.Reverse(temp);
                if (output == null)
                    output = temp.ToArray();
                else
                    output = output.Concat(temp).ToArray();
            }

            return output;
        }

        public byte[] FileEncryption(byte[] plainText)  // метод, шифрующий открытый текст
        {
            _key = BitExtensions.StringToBits(Key, Encoding.ASCII);    // ключ конвертируется из строки в массив байт
            if (Mode == Mode.CBC)                       // проверка, используется ли режим шифрования CBC
            {
                _iv = BitExtensions.StringToBits(IV, Encoding);        // если используется, то конвертируется вектор инициализации
            }
            GenerateKeys();                             // генерируются раундовые ключи
            SetBlocksOfPlainText(plainText);            // открытый текст разбивается на блоки
            Start?.Invoke(this, _blocks.GetLength(0));
            byte[] IP;                                  // переменная для хранения результата перестановки IP
            byte[] L = new byte[32];                    // переменная для хранения левой половины
            byte[] R = new byte[32];                    // переменная для хранения правой половины
            byte[] EP;                                  // переменная для хранения результата перестановки E/P
            byte[] xorRoundKey;                         // переменная для хранения результата XOR(E/P, Ki)
            byte[] SBox = new byte[32];                 // переменная для хранения результата перестановки SBox
            byte[] SBoxCells = new byte[6];             // переменная для хранения бит Bj, чтобы передавать их в Sj(Bj)
            byte[] SBoxPermutation;                     // переменная для хранения результата перестановки P, также является результатом ƒ
            byte[] xorToL = null;                       // переменная для хранения результата XOR(Li, ƒ)
            byte[] LR = new byte[64];                   // переменная для хранения результата конкатенации R16 и L16
            byte[] LRpermutation = new byte[64];        // переменная для хранения реузльтата перестановки IP inverse
            byte[] temp = null;                         // временная переменная для хранения данных текущего блока
            byte[] output = null;                       // переменная для вывода шифр текста
            for (int i = 0; i < _blocks.GetLength(0); i++) // цикл, который "пробегается" по всем блокам
            {
                BlockProcess?.Invoke(this, i + 1);
                switch (Mode)                       // определяет действия для определенного режима шифрования
                {
                    case Mode.CBC:                  // если режим шифрования CBC, то:
                        if (i == 0)                     // если блок первый по счету, то
                            _blocks[i] = BitExtensions.XOR(_blocks[i], _iv);    // Ti = XOR(Ti, IV)
                        else                            // иначе
                        {
                            Array.Reverse(LRpermutation);
                            _blocks[i] = BitExtensions.XOR(_blocks[i], LRpermutation); // Ti = XOR(Ti, Ci-1), где Ci-1 - блок шифр текста, полученный на предыдущей итерации
                        }
                        break;
                }
                IP = Permutation(_blocks[i], Tables.IP);    // начальная перестановка бит IP
                Array.Copy(IP, 0, L, 0, L.Length);
                Array.Copy(IP, L.Length, R, 0, R.Length);
                for (int j = 0; j < 16; j++)                // цикл, состоящий из 16 итераций преобразования по функции Фейстеля
                {
                    EP = Permutation(R, Tables.E);          // расширяющая перестановка E/P с 32 бит до 48 бит
                    xorRoundKey = BitExtensions.XOR(EP, _roundKeys[j].Key); // XOR(E/P, Ki)
                    for (int k = 0, l = 0; k < xorRoundKey.Length; k += 6, l++) // вычисление 6 бит для кажого Bj
                    {
                        Array.Copy(xorRoundKey, k, SBoxCells, 0, SBoxCells.Length);
                        byte[] b = Sbox(SBoxCells, Tables.SBox[l]); //выполнение функции Sj(Bj) для S блоков
                        Array.Copy(b, 0, SBox, 4 * l, 4);
                    }
                    SBoxPermutation = Permutation(SBox, Tables.P);  // перестановка P
                    xorToL = BitExtensions.XOR(SBoxPermutation, L); // XOR(Li, ƒ)
                    L = R;                                          // Li = Ri-1
                    R = xorToL;                                     // Ri = XOR(Li-1, ƒ)
                }
                R = L;                                  // R = L16 (половины меняются местами в соответствии с алгоритмом DES)
                L = xorToL;                             // L = R16
                Array.Copy(L, 0, LR, 0, L.Length);
                Array.Copy(R, 0, LR, L.Length, R.Length);
                LRpermutation = Permutation(LR, Tables.IPInverse);  // конечная перестановка IP inverse, которая является инверсией функции перестановки IP
                temp = BitExtensions.BitsToByte(LRpermutation);     // запись во временную переменную массива байт, конвертированного из массива бит
                Array.Reverse(temp);                                // временная переменная разворачивается,
                                                                    // так как двоичный код подразумевает чтение справа на лево,
                                                                    // а десятичные числа читаются слева на право.

                if (output == null)                                 // если массив для вывода пустой, то
                    output = temp;                                  // присваивает значение массива temp к output
                else                                                // иначе
                    output = output.Concat(temp).ToArray();         // соединяет уже существующие блоки с еще одним
            }
            Stop?.Invoke(this, null);
            return output;
        }
        public byte[] FileDecryption(byte[] cipherText) // метод, расшифровывающий шифр текст
        {
            Array.Reverse(cipherText);  // переворачивает шифр текст,
                                        // так как метод с разбиением блоков будет переводить данные в
                                        // двоичный формат, который чистается справа на лево.

            _key = BitExtensions.StringToBits(Key, Encoding.ASCII);
            if (Mode == Mode.CBC)
            {
                _iv = BitExtensions.StringToBits(IV, Encoding);
            }
            GenerateKeys();
            SetBlocksOfencyptedMessage(cipherText); // разбивает закрытый текст на блоки
            Start?.Invoke(this, _blocks.GetLength(0));
            byte[] IP;
            byte[] L = new byte[32];
            byte[] R = new byte[32];
            byte[] EP;
            byte[] xorRoundKey;
            byte[] SBox = new byte[32];
            byte[] SBoxCells = new byte[6];
            byte[] SBoxPermutation;
            byte[] xorToL = null;
            byte[] LR = new byte[64];
            byte[] LRPermutation = new byte[64];
            byte[] temp;
            byte[] output = null;
            int blength = _blocks.GetLength(0) - 1;
            for (int i = blength; i >= 0; i--)    // цикл в методе расшифрования следует обратному алгоритму,
                                                                   // соответственно блоки будут расшифровываться в обратном порядке
            {
                BlockProcess?.Invoke(this, blength - i + 1);
                IP = Permutation(_blocks[i], Tables.IP);
                Array.Copy(IP, 0, L, 0, L.Length);
                Array.Copy(IP, L.Length, R, 0, R.Length);
                for (int j = 0; j < 16; j++)
                {
                    EP = Permutation(R, Tables.E);
                    xorRoundKey = BitExtensions.XOR(EP, _roundKeys[16 - j - 1].Key);    // так как расшифрование следует в обратном порядке,
                                                                                        // то раудовые ключи будут применяться в обратном порядке.
                    for (int k = 0, l = 0; k < xorRoundKey.Length; k += 6, l++)
                    {
                        Array.Copy(xorRoundKey, k, SBoxCells, 0, SBoxCells.Length);
                        byte[] b = Sbox(SBoxCells, Tables.SBox[l]);
                        Array.Copy(b, 0, SBox, 4 * l, 4);
                    }
                    SBoxPermutation = Permutation(SBox, Tables.P);
                    xorToL = BitExtensions.XOR(SBoxPermutation, L);
                    L = R;
                    R = xorToL;
                }
                R = L;
                L = xorToL;
                Array.Copy(L, 0, LR, 0, L.Length);
                Array.Copy(R, 0, LR, L.Length, R.Length);
                LRPermutation = Permutation(LR, Tables.IPInverse);
                switch (Mode)
                {
                    case Mode.CBC: // проверка на режим шифрования CBC будет в конце, потому что Ti сцепляется с Ci-1
                        if (i == _blocks.GetLength(0) - 1)                                  // Если блок последний по счету, то
                            LRPermutation = BitExtensions.XOR(LRPermutation, _iv);          // Ti = XOR(Ti, IV)
                        else                                                                // Иначе
                            LRPermutation = BitExtensions.XOR(LRPermutation, _blocks[i + 1]); // Ti = XOR(Ti, Ci-1)
                        break;
                }
                temp = BitExtensions.BitsToByte(LRPermutation);
                Array.Reverse(temp);
                if (output == null)
                    output = temp.ToArray();
                else
                    output = output.Concat(temp).ToArray();
            }
            Stop?.Invoke(this, null);
            return output;
        }
    }
}