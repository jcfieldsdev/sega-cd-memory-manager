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
using System.Text.RegularExpressions;
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
            byte[] data = BlockEncoder.DecodeIfProtected(_data, _protect);

            var contents = new List<byte>(length);
            contents.AddRange(data);
            contents.AddRange(name);
            contents.Add(_protect);

            File.WriteAllBytes(path, contents.ToArray());
        }

        public void Rename(string name)
        {
            if (name.Length == 0 || name.Length > (int)RecordLength.Name)
            {
                throw new Exception("The specified entry name is not a valid length.");
            }

            var pattern = new Regex(@"[^A-Z0-9_]");

            if (pattern.IsMatch(name))
            {
                throw new Exception("The specified entry name contains invalid characters.");
            }

            _name = name;
        }
    }
}
