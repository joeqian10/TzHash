using System;

using System.Runtime.InteropServices;

namespace TestExternalTzHash
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var n = 3000000;
            var temp = new byte[n];

            // hash2
            ulong[] us = new ulong[8];
            Hash(temp, n, ref us[0]);

            var hash = ToByteArray(us);

            for (int i = 0; i < hash.Length; i++)
            {
                Console.Write(string.Format("{0:x2}", hash[i]));
            }

            Console.WriteLine(hash.ToString());
        }

        //[DllImport("TzHashCLangLibrary.dll", EntryPoint = "hash", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        [DllImport("TzHashCLangLibrary.dll", EntryPoint = "hash")]
        public static extern void Hash(byte[] data, int n, ref ulong sb);

        public static byte[] ToByteArray(ulong[] data)
        {
            var n = data.Length;

            var buff = new byte[8*n];
            for (int i = 0; i < n; i++)
            {
                var b = BitConverter.GetBytes(data[i]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }
                Array.Copy(b, 0, buff, i * 8, 8);
            }

            return buff;
        }
    }
}
