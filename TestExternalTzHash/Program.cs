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

            // hash
            ulong[] us = new ulong[8];
            Hash(temp, n, ref us[0]);

            var hash = ToByteArray(us);

            for (int i = 0; i < hash.Length; i++)
            {
                Console.Write(string.Format("{0:x2}", hash[i]));
            }

            // should output "601b0a3b8a51c34e3028245e88112aea0fd7571f9de28d6a81f31e58eec1e3310fd7571f9de28d6a81f31e58eec1e3317fb5a404b194d99b33ce18ef5592ec88"

            Console.WriteLine();

            Console.WriteLine("Finished!");
            Console.ReadLine();
        }

        /// <summary>
        /// In windows, TzHashCLangLibrary.dll will be searched; while in linux, libTzHashCLangLibrary.so will be searched.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="n"></param>
        /// <param name="sb"></param>
        [DllImport("TzHashCLangLibrary", EntryPoint = "hash", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Hash(byte[] data, int n, ref ulong sb);

        public static byte[] ToByteArray(ulong[] data)
        {
            var n = data.Length;

            var buff = new byte[8 * n];
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
