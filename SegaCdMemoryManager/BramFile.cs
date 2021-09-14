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
			DirectorySize = 32
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

		private const string Identifier = "SEGA_CD_ROM\x00\x01\x00\x00\x00RAM_CARTRIDGE___";

		private string _path;
		private byte[] _data;
        private List<SaveEntry> _entries;
		private int _filesUsed;
		private int _blocksFree;

		public string Path { get => _path; set => _path = value; }
		public List<SaveEntry> Entries { get => _entries; }
		public int FilesUsed { get => _filesUsed; }
		public int BlocksFree { get => _blocksFree; }
		public int FileSize { get => _data.Length; }

		public BramFile(string path = "")
        {
            _data = path == "" ? NewFile() : OpenFile(path);
            _path = path;

			_entries = new List<SaveEntry>();

			ReadFooter();
			ReadEntries();
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

		private void ReadFooter()
        {
			int start = _data.Length - (int)Format.BlockSize;

			// values are stored four times each for redundancy
			byte[] filesUsedSlice = _data.Skip(start + (int)FooterOffset.FilesUsed).Take(8).ToArray();
			byte[] blocksFreeSlice = _data.Skip(start + (int)FooterOffset.BlocksFree).Take(8).ToArray();

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

		private void ReadEntries()
        {
			for (int i = 0; i < _filesUsed; i++)
            {
				int blockStart = _data.Length - (int)Format.BlockSize - (i + 1) * (int)Format.BlockSize;
				byte[] block = DecodeBlock(_data.Skip(blockStart).Take((int)Format.BlockSize).ToArray());

				// each block contains data for up to two entries
				for (int j = 0; j < 2; j++)
                {
					int recordStart = (1 - j) * (int)Format.DirectorySize / 2;
					byte[] record = block.Skip(recordStart).Take((int)Format.DirectorySize / 2).ToArray();

					string name = System.Text.Encoding.ASCII.GetString(record, (int)RecordOffset.Name, (int)RecordLength.Name);
					int protect = record[(int)RecordOffset.Protect];

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

					byte[] data = _data.Skip(start * (int)Format.BlockSize).Take(size * (int)Format.BlockSize).ToArray();

					_entries.Add(new SaveEntry(name, protect, data));
                }
			}

			CountEntries();
		}

		private byte[] DecodeBlock(byte[] block)
        {
			byte[] directory = new byte[(int)Format.DirectorySize + 4];
			uint bits = 0;

			for (int i = 0, j = 0; j < (int)Format.DirectorySize + 4; i++)
            {
				int shift = (i * 2) % 8;
				bits |= (uint)((block[i] & 0xfc) << shift);

				if (shift > 0)
                {
					directory[j] = (byte)((bits & 0xff00) >> 8);
					j++;
                }

				bits <<= 8;
            }

			return directory.Skip(2).Take((int)Format.DirectorySize).ToArray();
        }

		private byte[] EncodeDirectory(byte[] directory)
        {
			return null;
        }

		private void CountEntries()
		{
			int bytesUsed = 0;

			foreach (var entry in _entries)
			{
				bytesUsed += entry.Size * (int)Format.BlockSize;
			}

			_filesUsed = _entries.Count;

			// subtracts header and footer blocks, directory, and save files
			int directorySize = (int)Math.Ceiling((float)_filesUsed / 2) * (int)Format.BlockSize;
			_blocksFree = (_data.Length - 2 * (int)Format.BlockSize - directorySize - bytesUsed) / (int)Format.BlockSize;

			// if file contains even number of entries, subtract one free block since
			// next block of data will require adding another directory block
			if (_filesUsed % 2 == 0)
			{
				_blocksFree--;
			}
		}

		public void WriteFile()
        {

        }

		public void AddEntry(SaveEntry newEntry)
        {
			if (_blocksFree < newEntry.Size)
            {
				throw new Exception("Not enough remaining space to add that entry.");
            }

			foreach (var entry in _entries)
            {
				if (entry.Name == newEntry.Name)
                {
					throw new Exception($"An entry named {entry.Name} already exists in this file.");
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
			int protect = data[data.Length - 1];

			var entry = new SaveEntry(name, protect, data.Take(offset).ToArray());
			AddEntry(entry);
        }
    }
}
