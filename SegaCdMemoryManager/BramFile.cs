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
            ProtectSize = 32,
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

        enum ProtectState
        {
            Off = 0x00,
            On = 0xff
        }

        private  struct Record
        {
            public string Name { get; set;  }
            public bool Protect { get; set;  }
            public int Start { get; set;  }
            public int SizeInBlocks { get; set;  }
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
                byte[] block = BlockConverter.Decode(data.Skip(blockStart).Take((int)Format.BlockSize).ToArray());

                // each block contains data for up to two entries
                for (int j = 0; j < 2; j++)
                {
                    // already read all entries
                    if (_entries.Count >= _filesUsed)
                    {
                        continue;
                    }

                    int recordStart = (1 - j) * (int)Format.ProtectSize / 2;
                    byte[] record = block.Skip(recordStart).Take((int)Format.ProtectSize / 2).ToArray();

                    string name = System.Text.Encoding.ASCII.GetString(record, (int)RecordOffset.Name, (int)RecordLength.Name);
                    bool protect = (byte)ProtectState.On == record[(int)RecordOffset.Protect];

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
                    byte[] decodedEntryData = BlockConverter.DecodeIfProtected(entryData, protect);
                    _entries.Add(new SaveEntry(name, protect, decodedEntryData));
                }
            }

            CountEntries();
        }
        
        private void CountEntries()
        {
            _filesUsed = _entries.Count;
            _blocksFree = Math.Max(0, CalculateFreeBlocks(_fileSize));
        }

        public int CalculateFreeBlocks(int fileSize)
        {
            // subtracts save files, directory, and header and footer blocks
            int bytesUsed = _entries.Select(entry => entry.SizeInBytes).Sum();
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

        public void WriteFile()
        {
            var records = new Record[_filesUsed];
            var contents = new List<byte>(_fileSize);
            int bytesUsed = 0;

            // writes header block
            contents.AddRange(_headerBlock);
            bytesUsed += _headerBlock.Length;
            
            // writes entry data
            for (int i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                var record = new Record
                {
                    Name = entry.Name,
                    Protect = entry.Protect,
                    Start = bytesUsed + 1,
                    SizeInBlocks = entry.SizeInBlocks
                };

                records[i] = record;
                bytesUsed += entry.SizeInBytes;

                byte[] encodedEntryData = BlockConverter.EncodeIfProtected(entry.Data, entry.Protect);
                contents.AddRange(encodedEntryData);
            }

            // writes filler bytes
            int directorySizeInBytes = (int)Math.Ceiling((float)_filesUsed / 2) * (int)Format.BlockSize;
            int fillerBytes = _fileSize - (int)Format.BlockSize - bytesUsed - directorySizeInBytes;
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
                        directory.AddRange(new byte[(int)Format.ProtectSize / 2]);
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

                    byte[] name = Encoding.ASCII.GetBytes(record.Name.PadRight((int)RecordLength.Name, '\0'));
                    byte protect = record.Protect ? (byte)ProtectState.On : (byte)ProtectState.Off;

                    directory.AddRange(name);
                    directory.Add(protect);
                    directory.AddRange(start);
                    directory.AddRange(size);
                }

                contents.AddRange(BlockConverter.Encode(directory.ToArray()));
            }

            byte[] blocksFree = BitConverter.GetBytes((short)_blocksFree);
            byte[] filesUsed = BitConverter.GetBytes((short)_filesUsed);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(blocksFree);
                Array.Reverse(filesUsed);
            }

            // writes footer block
            contents.AddRange(Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat('_', (int)Format.Underscores))));
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

        public void AddEntry(SaveEntry entryToAdd)
        {
            if (_blocksFree < entryToAdd.SizeInBlocks)
            {
                throw new Exception($"Not enough remaining space to add the entry {entryToAdd.Name}.");
            }

            
            if (_entries.Exists(entry => entry.Name == entryToAdd.Name))
            {
                throw new Exception($"An entry named {entryToAdd.Name} already exists in this file.");
            }

            _entries.Add(entryToAdd);
            CountEntries();
        }

        public void RemoveEntry(SaveEntry entryToRemove)
        {
            _entries.Remove(entryToRemove);
            CountEntries();
        }

        public void ImportEntry(string path)
        {
            if (!File.Exists(path))
            {
                throw new Exception("The specified file does not exist.");
            }

            byte[] data = File.ReadAllBytes(path);
            int offset = data.Length - (int)RecordLength.Name - 1;
            string name = System.Text.Encoding.ASCII.GetString(data, offset, (int)RecordLength.Name);
            bool protect = (byte)ProtectState.On == data[data.Length - 1];

            FileInfo fileInfo = new FileInfo(path);
            long size = fileInfo.Length - (int)RecordLength.Name - 1;

            if (protect && size % (int)Format.ProtectSize != 0)
            {
                throw new Exception("The specified file is not a valid save entry.");
            }
            
            if (!protect && size % (int)Format.BlockSize != 0)
            {
                throw new Exception("The specified file is not a valid save entry.");
            }

            if (name.Length == 0)
            {
                throw new Exception("The specified file is not a valid save entry.");
            }

            var entry = new SaveEntry(name, protect, data.Take(offset).ToArray());
            AddEntry(entry);
        }

        public void RenameEntry(SaveEntry entryToRename, string name)
        {
            if (_entries.Exists(entry => entry.Name == name))
            {
                throw new Exception($"An entry named {entryToRename.Name} already exists in this file.");
            }

            entryToRename.Rename(name);
        }

        public void ProtectEntry(SaveEntry entryToProtect)
        {
            bool protect = !entryToProtect.Protect;

            // protected entries use twice as much space
            if (protect && CalculateFreeBlocks(_fileSize) < entryToProtect.SizeInBlocks)
            {
                throw new Exception($"Not enough free space remaining in file to protect the entry {entryToProtect.Name}.");
            }

            entryToProtect.Protect = protect;
        }

        public void MoveUpEntry(int index)
        {
            if (index <= 0)
            {
                return;
            }

            SaveEntry entryToMove = _entries[index];
            _entries.RemoveAt(index);
            _entries.Insert(index - 1, entryToMove);
        }

        public void MoveDownEntry(int index)
        {
            if (index >= _entries.Count - 1)
            {
                return;
            }

            SaveEntry entryToMove = _entries[index];
            _entries.RemoveAt(index);
            _entries.Insert(index + 1, entryToMove);
        }

        public void Resize(int exponent)
        {
            // valid file sizes are between 8 and 512 KB
            if (exponent < 3 || exponent > 9)
            {
                throw new Exception("The specified file size is invalid.");
            }

            int fileSize = (int)Math.Pow(2, exponent) * 1024;

            if (CalculateFreeBlocks(fileSize) < 0)
            {
                throw new Exception("The specified file size is too small for the current file.");
            }

            _fileSize = fileSize;
            CountEntries();
        }
    }
}
