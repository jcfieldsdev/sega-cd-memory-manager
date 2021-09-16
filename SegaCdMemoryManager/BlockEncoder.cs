using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegaCdMemoryManager
{
    static class BlockEncoder
    {
        enum Format
        {
            BlockSize = 64
        }

        enum ProtectState
        {
            Off = 0x00,
            On = 0xff
        }

        public static byte[] Decode(byte[] data)
        {
            if (BitConverter.IsLittleEndian)
            {
                return DecodeLittleEndian(data);
            }

            return DecodeBigEndian(data);
        }

        public static byte[] DecodeIfProtected(byte[] data, byte protect)
        {
            if (protect == (byte)ProtectState.On)
            {
                return Decode(data);
            }

            return data;
        }

        private static byte[] DecodeLittleEndian(byte[] encoded)
        {
            int size = encoded.Length / 2;
            byte[] decoded = new byte[size + 4];
            uint bits = 0;

            for (int encodedIndex = 0, decodedIndex = 0; decodedIndex < size + 4; encodedIndex++)
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
            int size = encoded.Length / 2;
            byte[] decoded = new byte[size + 4];
            uint bits = 0;

            for (int encodedIndex = 0, decodedIndex = 0; decodedIndex < size + 4; encodedIndex++)
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

        public static byte[] EncodeIfProtected(byte[] data, byte protect)
        {
            if (protect == (byte)ProtectState.On)
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
