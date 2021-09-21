/******************************************************************************
 * Sega CD Memory Manager                                                     *
 *                                                                            *
 * Copyright (C) 2021 J.C. Fields (jcfields@jcfields.dev).                    *
 *                                                                            *
 * This program is free software: you can redistribute it and/or modify it    *
 * under the terms of the GNU General Public License as published by the Free *
 * Software Foundation, either version 3 of the License, or (at your option)  *
 * any later version.                                                         *
 *                                                                            *
 * This program is distributed in the hope that it will be useful, but        *
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY *
 * or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License   *
 * for more details.                                                          *
 *                                                                            *
 * You should have received a copy of the GNU General Public License along    *
 * with this program.  If not, see <http://www.gnu.org/licenses/>.            *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegaCdMemoryManager
{
    static class BlockConverter
    {
        enum Format
        {
            BlockSize = 64
        }

        public static byte[] Decode(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                return DecodeLittleEndian(data);
            }

            return DecodeBigEndian(data);
        }

        public static byte[] DecodeIfProtected(byte[] data, bool protect)
        {
            if (protect == true)
            {
                return Decode(data);
            }

            return data;
        }

        private static byte[] DecodeLittleEndian(byte[] encoded)
        {
            int size = encoded.Length / 2 + 2;
            byte[] decoded = new byte[size];
            uint bits = 0;

            for (int encodedIndex = 0, decodedIndex = 0; decodedIndex < size; encodedIndex++)
            {
                int shift = (encodedIndex * 2) % 8;
                bits |= (uint)((encoded[encodedIndex] & 0x00fc) << shift);

                if (shift > 0)
                {
                    decoded[decodedIndex] = (byte)((bits & 0xff00) >> 8);
                    decodedIndex++;
                }

                bits <<= 8;
            }

            return decoded.Skip(2).Take(size).ToArray();
        }

        private static byte[] DecodeBigEndian(byte[] encoded)
        {
            int size = encoded.Length / 2 + 2;
            byte[] decoded = new byte[size];
            uint bits = 0;

            for (int encodedIndex = 0, decodedIndex = 0; decodedIndex < size; encodedIndex++)
            {
                int shift = (encodedIndex * 2) % 8;
                bits |= (uint)((encoded[encodedIndex] & 0xfc00) << shift);

                if (shift > 0)
                {
                    decoded[decodedIndex] = (byte)((bits & 0x00ff) >> 8);
                    decodedIndex++;
                }

                bits <<= 8;
            }

            return decoded.Skip(2).Take(size).ToArray();
        }

        public static byte[] Encode(byte[] data)
        {

            if (BitConverter.IsLittleEndian)
            {
                return EncodeLittleEndian(data);
            }

            return EncodeBigEndian(data);
        }

        public static byte[] EncodeIfProtected(byte[] data, bool protect)
        {
            if (protect == false)
            {
                return Encode(data);
            }

            return data;
        }

        private static byte[] EncodeLittleEndian(byte[] data)
        {
            var contents = new List<byte>();
            contents.AddRange(data);
            contents.AddRange(new byte[(int)Format.BlockSize / 2]);

            return contents.ToArray();
        }

        private static byte[] EncodeBigEndian(byte[] data)
        {
            var contents = new List<byte>();
            contents.AddRange(data);
            contents.AddRange(new byte[(int)Format.BlockSize / 2]);

            return contents.ToArray();
        }
    }
}
