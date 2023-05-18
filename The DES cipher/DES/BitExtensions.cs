using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace The_DES_cipher.DES
{
    internal static class BitExtensions
    {
        public static byte GetBit(this byte b, int bitnum)  // метод, возвращающий бит из числа, находящийся в позиции bitnum
        {
            if ((b & (1 << bitnum - 1)) != 0) // вычисляется значение маски, содержащей единицу в битовой позиции,
                                              // соответственно числу bitnum, затем выполняется операция битового И между
                                              // числом b и маской, если результат не равен нулю, то метод возвращает 1, иначе 0
                return 1;
            else
                return 0;
        }
        public static byte GetBit(this int b, int bitnum)
        {
            if ((b & (1 << bitnum - 1)) != 0)
                return 1;
            else
                return 0;

        }
        public static byte[] ByteToBits(this byte[] bytes)
        {
            List<byte> result = new List<byte>();    // битовая маска для преобразования массива байт в массив бит
            for (int i = 0; i < bytes.Length; i++)  // цикл, который "пробегается" по массиву байт
            {
                for (int j = 8; j >= 1; j--)        // цикл, который "пробегается" по битам числа
                {
                    result.Add(GetBit(bytes[i], j)); // добавляет в маску бит j числа bytes[i]
                }
            }

            return result.ToArray();                 // возвращает массив бит
        }
        public static byte[] BitsToByte(this byte[] bits)
        {
            Array.Reverse(bits);                    // переворачивает массив бит
            int bytes = bits.Length / 8;            // в каждом байте содержится 8 бит, считает, сколько байт содержится в массиве бит
            if ((bits.Length % 8) != 0) bytes++;    // если бит в последнем блоке не хватает, то добавляет к количеству байт еще 1
            byte[] result = new byte[bytes];        // заготовка для массива байт
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bits.Length; i++)   // цикл "пробегается" по массиву бит
            {
                if (bits[i] == 1)
                {
                    result[byteIndex] |= (byte)(((byte)1) << bitIndex); // элемент массива байт дополняется битом с помощью побитовой операции ИЛИ
                }
                bitIndex++;
                if (bitIndex == 8)  // если достиг 8 бита, сбрасывает счетчик битов, и прибавляет к счетчику байт еденицу
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return result;  // возвращает массив байт
        }
        
        public static int[] BitsToInt(this bool[] bits)
        {
            Array.Reverse(bits);
            int bytes = bits.Length / 32;
            if ((bits.Length % 32) != 0) bytes++;
            int[] result = new int[bytes];
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bits.Length; i++)
            {
                if (bits[i])
                {
                    result[byteIndex] |= (int)(((int)1) << bitIndex);
                }
                bitIndex++;
                if (bitIndex == 32)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }
            return result;
        }

        public static byte[] IntToBits(this int n)
        {
            List<byte> bits = new List<byte>();
            for (int j = 32; j >= 1; j--)
            {
                bits.Add(GetBit(n, j));
            }
            return bits.ToArray();
        }

        public static byte[] FromHex(this string hex)   // возвращает массив байт из шестнадцатиричного кода
        {
            byte[] raw = new byte[hex.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return raw;
        }

        public static byte[] LeftShift(this byte[] data, int n)    // метод представляет собой циклический побитовый сдвиг влево на n позиций
        {
            byte[] result = new byte[data.Length];
            byte[] shiftedBits = new byte[n];
            Array.Copy(data, shiftedBits, n);
            for (int i = 0; i < data.Length - n; i++)
            {
                result[i] = data[i + n];
            }
            Array.Copy(shiftedBits, 0, result, data.Length - n, n);
            return result;
        }

        public static byte[] StringToBits(this string s, Encoding encoding) // метод переводит строку в массив бит
        {
            byte[] bytes = encoding.GetBytes(s);
            byte[] bits = ByteToBits(bytes).ToArray();
            return bits;
        }

        public static byte[] XOR(this byte[] x, byte[] y)   // метод, возвращающий результат операции XOR над x и y
        {
            byte[] result = new byte[x.Length];
            for (int i = 0; i < x.Length; i++)          // "пробегается" по битам x
            {
                try
                {
                    result[i] = (byte)(x[i] ^ y[i]);    // если x имеет одинаковую размерность, то выполняется операция XOR над битами
                }
                catch
                {
                    result[i] = x[i];                   // Иначе бит в позиции i будет равен x[i]
                }
                
            }
            return result;                              // возвращает результат операции
        }

        public static byte[] IntToBytes(this int intValue)  // метод, конвертирующий число int в массив byte
        {
            byte[] intBytes = BitConverter.GetBytes(intValue);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);
            byte[] result = intBytes;
            return result;
        }
    }
}