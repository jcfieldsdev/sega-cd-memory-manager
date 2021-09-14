using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegaCdMemoryManager
{
    class SaveEntry
    {
        enum Format
        {
            BlockSize = 64
        }

        enum RecordLength
        {
            Name = 11,
            Protect = 1
        }

        private readonly string _name;
        private readonly byte _protect;
        private readonly byte[] _data;

        public string Name { get => _name; }
        public byte Protect { get => _protect; }
        public byte[] Data { get => _data; }
        public int SizeInBlocks { get => _data.Length / (int)Format.BlockSize; }
        public int SizeInBytes { get => _data.Length; }

        public SaveEntry(string name, byte protect, byte[] data)
        {

            _name = name;
            _protect = protect;
            _data = data;
        }

        public void WriteFile(string path)
        {
            int length = _data.Length + (int)RecordLength.Name + (int)RecordLength.Protect;
            byte[] name = Encoding.ASCII.GetBytes(_name.PadRight((int)RecordLength.Name, '\0'));

            var contents = new List<byte>(length);
            contents.AddRange(_data);
            contents.AddRange(name);
            contents.Add(_protect);

            File.WriteAllBytes(path, contents.ToArray());
        }
    }
}
