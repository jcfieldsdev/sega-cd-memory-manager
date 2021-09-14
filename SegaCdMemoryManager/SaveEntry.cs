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

        private string _name;
        private int _protect;
        private byte[] _data;

        public string Name { get => _name; set => _name = value; }
        public int Size { get => _data.Length / (int)Format.BlockSize; }

        public SaveEntry(string name, int protect, byte[] data)
        {

            _name = name;
            _protect = protect;
            _data = data;
        }

        public void WriteFile(string path)
        {
            int length = _data.Length + (int)RecordLength.Name + (int)RecordLength.Protect;
            byte[] name = Encoding.ASCII.GetBytes(_name.PadRight((int)RecordLength.Name, '\0'));

            List<byte> list = new List<byte>(length);
            list.AddRange(_data);
            list.AddRange(name);
            list.Add((byte)_protect);

            File.WriteAllBytes(path, list.ToArray());
        }
    }
}
