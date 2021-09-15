using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SegaCdMemoryManager
{
    class BramFile
    {
        enum Format
        {
            FileSize = 8192,
            BlockSize = 64,
            DirectorySize = 32,
            Underscores = 11
        }

        enum FooterOffset
        {
            BlocksFree = 0x10,
            FilesUsed = 0x18
        }

        enum RecordOffset
        {
            Name = 0x0,
            Protect = 0xb,
            Start = 0xc,
            Size = 0xe
        }

        enum RecordLength
        {
            Name = 11
        }

        private readonly struct Record
        {
            public Record(string name, byte protect, int start, int size)
            {
                Name = name;
                Protect = protect;
                Start = start;
                SizeInBlocks = size;
            }

            public string Name { get; }
            public byte Protect { get; }
            public int Start { get; }
            public int SizeInBlocks { get; }
        }

        private const string Identifier = "SEGA_CD_ROM\x00\x01\x00\x00\x00RAM_CARTRIDGE___";

        private string _path;
        private readonly List<SaveEntry> _entries;
        private readonly byte[] _headerBlock;
        private int _filesUsed;
        private int _blocksFree;
        private int _fileSize;

        public string Path { get => _path; set => _path = value; }
        public List<SaveEntry> Entries { get => _entries; }
        public int FilesUsed { get => _filesUsed; }
        public int BlocksFree { get => _blocksFree; }
        public int SizeInBytes { get => _fileSize; }

        public BramFile(string path = "")
        {
            byte[] data = path == "" ? NewFile() : OpenFile(path);

            _path = path;
            _entries = new List<SaveEntry>();
            _headerBlock = data.Take((int)Format.BlockSize).ToArray();
            _fileSize = data.Length;

            ReadFooter(data);
            ReadEntries(data);
        }

        private byte[] NewFile()
        {
            return Properties.Resources.blank;
        }

        private byte[] OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("The specified file does not exist.");
            }

            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Length > 0 && fileInfo.Length % (int)Format.FileSize != 0)
            {
                throw new Exception("The specified file is not a valid save file.");
            }

            byte[] data = File.ReadAllBytes(path);
            string identifier = System.Text.Encoding.ASCII.GetString(data, data.Length - Identifier.Length, Identifier.Length);

            if (identifier != Identifier)
            {
                throw new Exception("The specified file has an invalid footer.");
            }

            return data;
        }

        private void ReadFooter(byte[] data)
        {
            int start = data.Length - (int)Format.BlockSize;

            // values are stored four times each for redundancy
            byte[] filesUsedSlice = data.Skip(start + (int)FooterOffset.FilesUsed).Take(8).ToArray();
            byte[] blocksFreeSlice = data.Skip(start + (int)FooterOffset.BlocksFree).Take(8).ToArray();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(filesUsedSlice);
                Array.Reverse(blocksFreeSlice);
            }

            int filesUsed1 = BitConverter.ToInt16(filesUsedSlice, 0);
            int filesUsed2 = BitConverter.ToInt16(filesUsedSlice, 2);
            int filesUsed3 = BitConverter.ToInt16(filesUsedSlice, 4);
            int filesUsed4 = BitConverter.ToInt16(filesUsedSlice, 6);

            if (filesUsed1 != filesUsed2 || filesUsed1 != filesUsed3 || filesUsed1 != filesUsed4) 
            {
                throw new Exception("Invalid value for files used in file footer.");
            }

            int blocksFree1 = BitConverter.ToInt16(blocksFreeSlice, 0);
            int blocksFree2 = BitConverter.ToInt16(blocksFreeSlice, 2);
            int blocksFree3 = BitConverter.ToInt16(blocksFreeSlice, 4);
            int blocksFree4 = BitConverter.ToInt16(blocksFreeSlice, 6);

            if (blocksFree1 != blocksFree2 || blocksFree1 != blocksFree3 || blocksFree1 != blocksFree4)
            {
                throw new Exception("Invalid value for blocks free in file footer.");
            }

            if (filesUsed1 == 0 && blocksFree1 == 0)
            {
                throw new Exception("Invalid file footer.");
            }

            _filesUsed = filesUsed1;
            _blocksFree = blocksFree1;
        }

        private void ReadEntries(byte[] data)
        {
            for (int i = 0; i < _filesUsed; i++)
            {
                int blockStart = data.Length - (int)Format.BlockSize - (i + 1) * (int)Format.BlockSize;
                byte[] block = DecodeBlock(data.Skip(blockStart).Take((int)Format.BlockSize).ToArray());

                // each block contains data for up to two entries
                for (int j = 0; j < 2; j++)
                {
                    int recordStart = (1 - j) * (int)Format.DirectorySize / 2;
                    byte[] record = block.Skip(recordStart).Take((int)Format.DirectorySize / 2).ToArray();

                    string name = System.Text.Encoding.ASCII.GetString(record, (int)RecordOffset.Name, (int)RecordLength.Name);
                    byte protect = record[(int)RecordOffset.Protect];

                    byte[] startSlice = record.Skip((int)RecordOffset.Start).Take(2).ToArray();
                    byte[] sizeSlice = record.Skip((int)RecordOffset.Size).Take(2).ToArray();

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(startSlice);
                        Array.Reverse(sizeSlice);
                    }

                    int start = BitConverter.ToInt16(startSlice, 0);
                    int size = BitConverter.ToInt16(sizeSlice, 0);

                    // skips blank entries
                    if (start == 0 && size == 0)
                    {
                        continue;
                    }

                    byte[] entryData = data.Skip(start * (int)Format.BlockSize).Take(size * (int)Format.BlockSize).ToArray();

                    _entries.Add(new SaveEntry(name, protect, entryData));
                }
            }

            CountEntries();
        }
        
        private void CountEntries()
        {
            _filesUsed = _entries.Count;
            _blocksFree = Math.Max(CalculateFreeSpace(_fileSize), 0);
        }

        public int CalculateFreeSpace(int fileSize)
        {
            int bytesUsed = 0;

            foreach (var entry in _entries)
            {
                bytesUsed += entry.SizeInBytes;
            }

            // subtracts header and footer blocks, directory, and save files
            int directorySizeInBlocks = (int)Math.Ceiling((float)_filesUsed / 2);
            int blocksFree = (fileSize - bytesUsed) / (int)Format.BlockSize - directorySizeInBlocks - 2;

            // if file contains even number of entries, subtract one free block since
            // next block of data will require adding another directory block
            if (_filesUsed % 2 == 0)
            {
                blocksFree--;
            }

            return blocksFree;
        }

        private byte[] DecodeBlock(byte[] block)
        {
            if (BitConverter.IsLittleEndian)
            {
                return DecodeBlockLittleEndian(block);
            }
            
            return DecodeBlockBigEndian(block);
        }

        private byte[] DecodeBlockLittleEndian(byte[] block)
        {
            byte[] directory = new byte[(int)Format.DirectorySize + 4];
            uint bits = 0;

            for (int i = 0, j = 0; j < (int)Format.DirectorySize + 4; i++)
            {
                int shift = (i * 2) % 8;
                bits |= (uint)((block[i] & 0x00fc) << shift);

                if (shift > 0)
                {
                    directory[j] = (byte)((bits & 0xff00) >> 8);
                    j++;
                }

                bits <<= 8;
            }

            return directory.Skip(2).Take((int)Format.DirectorySize).ToArray();
        }

        private byte[] DecodeBlockBigEndian(byte[] block)
        {
            byte[] directory = new byte[(int)Format.DirectorySize + 4];
            uint bits = 0;

            for (int i = 0, j = 0; j < (int)Format.DirectorySize + 4; i++)
            {
                int shift = (i * 2) % 8;
                bits |= (uint)((block[i] & 0xfc00) << shift);

                if (shift > 0)
                {
                    directory[j] = (byte)((bits & 0x00ff) >> 8);
                    j++;
                }

                bits <<= 8;
            }

            return directory.Skip(2).Take((int)Format.DirectorySize).ToArray();
        }

        private byte[] EncodeDirectory(byte[] directory)
        {

            if (BitConverter.IsLittleEndian)
            {
                return EncodeDirectoryLittleEndian(directory);
            }

            return EncodeDirectoryBigEndian(directory);
        }

        private byte[] EncodeDirectoryLittleEndian(byte[] directory)
        {
            var contents = new List<byte>();
            contents.AddRange(directory);
            contents.AddRange(new byte[(int)Format.BlockSize / 2]);

            return contents.ToArray();
        }

        private byte[] EncodeDirectoryBigEndian(byte[] directory)
        {
            var contents = new List<byte>();
            contents.AddRange(directory);
            contents.AddRange(new byte[(int)Format.BlockSize / 2]);

            return contents.ToArray();
        }

        public void WriteFile()
        {
            var records = new Record[_filesUsed];
            var contents = new List<byte>(_fileSize);
            int iter = 0, offset = 0;

            // writes header block
            contents.AddRange(_headerBlock);
            offset += _headerBlock.Length;
            
            // writes entry data
            foreach (var entry in _entries)
            {
                var record = new Record(entry.Name, entry.Protect, offset + 1, entry.SizeInBlocks);
                records[iter] = record;
                offset += entry.SizeInBytes;
                iter++;

                contents.AddRange(entry.Data);
            }

            // writes filler bytes
            int directorySizeInBytes = (int)Math.Ceiling((float)_filesUsed / 2) * (int)Format.BlockSize;
            int fillerBytes = _fileSize - (int)Format.BlockSize - offset - directorySizeInBytes;
            contents.AddRange(new byte[fillerBytes]);

            // writes directory
            for (int i = 0; i < (float)_filesUsed / 2; i++)
            {
                var directory = new List<byte>();

                for (int j = 0; j < 2; j++)
                {
                    int index = 2 * i + (1 - j);

                    // handles missing record when odd number of entries
                    if (index > records.Length - 1)
                    {
                        directory.AddRange(new byte[(int)Format.DirectorySize / 2]);
                        continue;
                    }

                    Record record = records[index];
                    byte[] start = BitConverter.GetBytes((short)record.Start);
                    byte[] size = BitConverter.GetBytes((short)record.SizeInBlocks);

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(start);
                        Array.Reverse(size);
                    }

                    directory.AddRange(Encoding.ASCII.GetBytes(record.Name.PadRight((int)RecordLength.Name, '\0')));
                    directory.Add(record.Protect);
                    directory.AddRange(start);
                    directory.AddRange(size);
                }

                contents.AddRange(EncodeDirectory(directory.ToArray()));
            }

            byte[] underscores = Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat('_', (int)Format.Underscores)));
            byte[] blocksFree = BitConverter.GetBytes((short)_blocksFree);
            byte[] filesUsed = BitConverter.GetBytes((short)_filesUsed);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(blocksFree);
                Array.Reverse(filesUsed);
            }

            // writes footer block
            contents.AddRange(underscores);
            contents.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x40 });
            contents.AddRange(blocksFree);
            contents.AddRange(blocksFree);
            contents.AddRange(blocksFree);
            contents.AddRange(blocksFree);
            contents.AddRange(filesUsed);
            contents.AddRange(filesUsed);
            contents.AddRange(filesUsed);
            contents.AddRange(filesUsed);
            contents.AddRange(Encoding.ASCII.GetBytes(Identifier));

            // writes bytes to file
            File.WriteAllBytes(_path, contents.ToArray());
        }

        public void AddEntry(SaveEntry newEntry)
        {
            if (_blocksFree < newEntry.SizeInBlocks)
            {
                throw new Exception($"Not enough remaining space to add the entry {newEntry.Name}.");
            }

            foreach (var entry in _entries)
            {
                if (entry.Name == newEntry.Name)
                {
                    throw new Exception($"An entry named {newEntry.Name} already exists in this file.");
                }
            }

            _entries.Add(newEntry);
            CountEntries();
        }

        public void RemoveEntry(SaveEntry entry)
        {
            _entries.Remove(entry);
            CountEntries();
        }

        public void ImportEntry(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("The specified file does not exist.");
            }

            FileInfo fileInfo = new FileInfo(path);

            if (fileInfo.Length % (int)Format.BlockSize != 0)
            {
                throw new Exception("The specified file is not a valid save entry.");
            }

            byte[] data = File.ReadAllBytes(path);
            int offset = data.Length - (int)RecordLength.Name - 1;
            string name = System.Text.Encoding.ASCII.GetString(data, offset, (int)RecordLength.Name);
            byte protect = data[data.Length - 1];

            if (name.Length == 0 || (protect != 0x00 || protect != 0xff))
            {
                throw new Exception("The specified file is not a valid save entry.");
            }

            var entry = new SaveEntry(name, protect, data.Take(offset).ToArray());
            AddEntry(entry);
        }

        public void Resize(int exponent)
        {
            if (exponent < 3 || exponent > 9)
            {
                throw new Exception("The specified file size is invalid.");
            }

            int fileSize = (int)Math.Pow(2, exponent) * 1024;

            if (CalculateFreeSpace(fileSize) < 0)
            {
                throw new Exception("The specified file size is too small for the current file.");
            }

            _fileSize = fileSize;

            CountEntries();
        }
    }
}
