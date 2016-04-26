using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace fangpu_terminal
{
    class BufferConverter
    {
        /// <summary>
        /// Convert buffer to integer from start byte to end byte
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static int BufferToInt(byte[] buffer, int start, int end)
        {

            int length = end - start + 1;
            byte[] bytes = new byte[length];
            Array.Copy(buffer, start, bytes, 0, length);
            Array.Reverse(bytes);
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                result = result + (bytes[i] << 8 * i);
            }
            return result;
        }
        static readonly char[] wtype = "BWD".ToCharArray();
        /// <summary>
        /// Buffer to Dictionary
        /// </summary>
        /// <param name="daq"></param>
        /// <param name="buffer"></param>
        /// <param name="start"></param>
        /// <param name="size"></param>
        /// <param name="area"></param>
        /// <param name="Wordlen"></param>
        public static void BufferDump(PlcDAQCommunicationObject daq, byte[] buffer, int start, int size, string area, int Wordlen)
        {
            if (buffer == null)
                return;

            string key;
            for (int i = 0; i < size; i = i + Wordlen)
            {
                int value = 0;
                if (area.Equals("I"))
                    key = area + (start + i).ToString();
                else
                    key= area +wtype[Wordlen-1]+(start + i).ToString(); 
                byte[] bytes = new byte[Wordlen];
                Array.Copy(buffer, i, bytes, 0, Wordlen);
                Array.Reverse(bytes);
                for (int j = 0; j < Wordlen; j++)
                {
                    value = value + (bytes[j]<<8*j);
                }
                daq.aream_data[key] = value;
                daq.bytes_data[key] = bytes;
            }

        }
    }
}
