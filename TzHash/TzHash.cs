﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TzHash
{
    public class TzHash : HashAlgorithm
    {
        public const int TzHashLength = 64;
        private GF127[] x;
        public override int HashSize => TzHashLength;

        private ulong[] raw;

        public TzHash()
        {
            Initialize();
        }

        public override void Initialize()
        {
            this.x = new GF127[4];
            this.Reset();
            HashValue = null;
        }

        public void Reset()
        {
            this.x[0] = new GF127(1, 0);
            this.x[1] = new GF127(0, 0);
            this.x[2] = new GF127(0, 0);
            this.x[3] = new GF127(1, 0);
        }

        [SecurityCritical]
        protected override void HashCore(byte[] array, int ibStart, int cbSize)
        {
            _ = HashData(array[ibStart..(ibStart + cbSize)]);
        }

        [SecurityCritical]
        protected override byte[] HashFinal()
        {
            return HashValue = ToByteArray();
        }

        [SecurityCritical]
        private int HashData(byte[] data)
        {
            var n = data.Length;
            for (int i = 0; i < n; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    MulBitRight(ref x[0], ref x[1], ref x[2], ref x[3], (data[i] & (1 << j)) != 0);
                }
            }
            return n;
        }

        public byte[] ComputeHash2(byte[] data)
        {
            _ = HashExternal(data);
            return this.ToByteArray2();
        }

        [SecurityCritical]
        public int HashExternal(byte[] data)
        {
            var n = data.Length;
            this.raw = new ulong[8];
            HashExternal(data, n, ref this.raw[0]);
            return n;
        }

        public byte[] ToByteArray()
        {
            var buff = new byte[HashSize];
            for (int i = 0; i < 4; i++)
            {
                Array.Copy(this.x[i].ToByteArray(), 0, buff, i * 16, 16);
            }
            return buff;
        }

        private byte[] ToByteArray2()
        {
            var n = this.raw.Length;
            var buff = new byte[8 * n];
            for (int i = 0; i < n; i++)
            {
                var b = BitConverter.GetBytes(this.raw[i]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(b);
                }
                Array.Copy(b, 0, buff, i * 8, 8);
            }
            return buff;
        }

        [DllImport("TzHashCLangLibrary.dll", EntryPoint = "hash")]
        private static extern void HashExternal(byte[] data, int n, ref ulong sb);

        public int HashWith10Threads(byte[] data)
        {
            if (data == null) return 0;

            int threadCount = 10;
            var n = data.Length;
            var size = n / threadCount; // the number of bytes each thread needs to handle
            var remaining = n % threadCount;

            var tmp = new byte[threadCount][];
            int j = 0;
            for (j = 0; j < threadCount - 1; j++)
            {
                tmp[j] = data[(j * size)..(j * size + size)];
            }
            // append remaining bytes to the last thread
            tmp[j] = data[(j * size)..];

            var hashes = new GF127[threadCount][];
            for (int i = 0; i < hashes.Length; i++)
            {
                hashes[i] = new GF127[] { new GF127(1, 0), new GF127(0, 0), new GF127(0, 0), new GF127(1, 0) };
            }
            var tasks = new Task[threadCount];
            for (int index = 0; index < threadCount; index++)
            {
                var idx = index;
                var task = new Task(() =>
                {
                    var subData = tmp[idx];
                    for (int i = 0; i < subData.Length; i++)
                    {
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x80) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x40) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x20) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x10) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x08) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x04) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x02) != 0);
                        MulBitRight(ref hashes[idx][0], ref hashes[idx][1], ref hashes[idx][2], ref hashes[idx][3], (subData[i] & 0x01) != 0);
                    }
                });
                tasks[idx] = task;
                task.Start();
            }
            Task.WaitAll(tasks);

            // convert to SL2[]
            var final = new SL2[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                final[i] = new SL2(hashes[i][0], hashes[i][1], hashes[i][2], hashes[i][3]);
            }

            var hash = Concat(final);
            return n;
        }

        // MulBitRight() multiply A (if the bit is 0) or B (if the bit is 1) on the right side 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MulBitRight(ref GF127 c00, ref GF127 c01, ref GF127 c10, ref GF127 c11, bool bit)
        {
            // plan 1
            GF127 t;
            if (bit)
            {   // MulB
                t = c00;
                c00 = GF127.Mul10(c00) + c01; // c00 = c00 * x + c01
                c01 = GF127.Mul11(t) + c01;   // c01 = c00 * (x+1) + c01

                t = c10;
                c10 = GF127.Mul10(c10) + c11; // c10 = c10 * x + c11
                c11 = GF127.Mul11(t) + c11;   // c11 = c10 * (x+1) + c11
            }
            else
            {   // MulA
                t = c00;
                c00 = GF127.Mul10(c00) + c01; // c00 = c00 * x + c01
                c01 = t;                      // c01 = c00

                t = c10;
                c10 = GF127.Mul10(c10) + c11; // c10 = c10 * x + c11
                c11 = t;                      // c11 = c10;
            }

            //// plan 2
            //var r = new SL2(c00, c01, c10, c11);
            //if (bit)
            //    r.MulB();
            //else
            //    r.MulA();
        }

        // Concat() performs combining of hashes based on homomorphic property.
        public static byte[] Concat(byte[][] hs)
        {
            var r = SL2.ID;
            for (int i = 0; i < hs.Length; i++)
            {
                r *= new SL2().FromByteArray(hs[i]);
            }
            return r.ToByteArray();
        }

        public static byte[] Concat(SL2[] hs)
        {
            var r = SL2.ID;
            for (int i = 0; i < hs.Length; i++)
            {
                r *= hs[i];
            }
            return r.ToByteArray();
        }

        // Validate() checks if hashes in hs combined are equal to h.
        public static bool Validate(byte[] h, byte[][] hs)
        {
            var expected = new SL2().FromByteArray(h);
            var actual = new SL2().FromByteArray(Concat(hs));
            return expected.Equals(actual);
        }

        // SubtractR() returns hash a, such that Concat(a, b) == c
        public static byte[] SubstractR(byte[] b, byte[] c)
        {
            var t1 = new SL2().FromByteArray(b);
            var t2 = new SL2().FromByteArray(c);
            var r = t2 * SL2.Inv(t1);
            return r.ToByteArray();
        }

        // SubtractL() returns hash b, such that Concat(a, b) == c
        public static byte[] SubstractL(byte[] a, byte[] c)
        {
            var t1 = new SL2().FromByteArray(a);
            var t2 = new SL2().FromByteArray(c);
            var r = SL2.Inv(t1) * t2;
            return r.ToByteArray();
        }

        // do not use this method, it's for fun, still in development
        public int HashDataParallel(byte[] data)
        {
            var n = data.Length;
            SL2[] t = new SL2[n * 8]; // caution out of memory exception

            Parallel.For(0, n, (i) =>
            {
                for (int j = 0; j < 8; j++)
                {
                    if ((data[i] & (1 << (7 - j))) == 0)
                        t[i * 8 + j] = SL2.A;
                    else
                        t[i * 8 + j] = SL2.B;
                }
            });

            SL2[] r = t;
            do
            {
                r = MulParallel(r);
            } while (r.Length > 1);

            var r0 = r[0];
            x[0] = r0[0][0];
            x[1] = r0[0][1];
            x[2] = r0[1][0];
            x[3] = r0[1][1];
            return n;
        }

        private SL2[] MulParallel(SL2[] data)
        {
            var len = data.Length;
            //if (len == 1) return data;

            SL2[] t = new SL2[len / 2];

            if (len % 2 != 0)
            {
                data = data.Append(new SL2()).ToArray();
                t = t.Append(new SL2()).ToArray();
            }

            Parallel.For(0, t.Length, (i) =>
            {
                t[i] = data[i * 2] * data[i * 2 + 1];
            });

            return t;
            //return MulParallel(t);
        }
    }
}
