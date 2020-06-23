using System;
using System.IO;
using System.Text;

namespace EsfLibrary
{
    /**
     * <summary>A class to represent the underlying data of a <see cref="Type26Node"/>.</summary>
     * <remarks>This has an odd representation.  Its formatting may not be correct.  The currently observed forms are a long form (10 or 18 bytes) varying based on installed dlc and a short form (9 bytes).</remarks>
     * TODO When the method of determining the length of the data is clarified, making the setters enforce proper data structure would be proper.
     * XXX This will probably need revisiting to correct the binary read when more samples are available.
     * Values from startpos:
     * 0x26 00 00 00 00 00 00 00 00 (short startpos (and save in Compressed_Data)
     * 0x26 08 01 00 00 00 00 00 00 00 (long startpos)
     * Values from saved games:
     * 0x26 01 00 01 20 00 00 00 00 (short community generated)
     * 0x26 10 01 00 00 00 00 00 00 00 ff e9 ff 3f 00 00 00 00 (long community generated with The Hunter & The Beast)
     * 0x26 10 01 00 00 00 00 00 00 00 ff ff 3d 1d 00 00 00 00 (long locally generated with all owned dlc enabled (no The Hunter & The Beast))
     * 0x26 01 08 01 00 00 00 00 00 (short locally generated irregardless of dlc (The Fay Enchantress))
     * 0x26 10 01 00 00 00 00 00 00 00 51 a8 3d 0c 00 00 00 00 (long locally generated with some dlc disabled (The Fay Enchantress))
     * 0x26 10 01 00 00 00 00 00 00 00 05 00 00 00 00 00 00 00 (long Three Kingdoms unmodded (Gongsun Zan))
     * 0x26 05 00 00 00 00 00 00 00 (short Three Kingdoms (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     * 0x26 FF FF 3D 1D 00 00 00 00 (short Warhammer 2 (Tiq Tak To) (Compressed_Data|Campaign_Env|Campaign_Setup|Campaign_Players_Setup|Players_Array|Players_Array - #|Campaign_Player_Setup))
     */
    public class Type26
    {
        #region Constructors
        ///<summary>Initializes a Type26 typical of an entry with all <see cref="Data"/> equal to zero.</summary>
        public Type26()
        {
            FirstByte = 0;
            Data = new byte[7];
        }

        /**
         * <summary>Initializes a Type26 from a binary source.</summary>
         * 
         * <param name="reader">A <see cref="BinaryReader"/> looking at the binary source.  It does not get closed.</param>
         */
        public Type26(BinaryReader reader)
        {
            FirstByte = reader.ReadByte();
#if DEBUG
            Console.WriteLine("Read node Type26 : first byte = {0}", FirstByte);
#endif
            if(FirstByte == 16)
                Data = reader.ReadBytes(16);
            else if(FirstByte == 8)
                Data = reader.ReadBytes(8);
            else
                Data = reader.ReadBytes(7);
        }

        /**
         * <summary>Initializes a Type26 from a string.</summary>
         * 
         * <param name="value">A string that contains a human-readable representation of a Type26.</param>
         */
        public Type26(string value)
        {
            string[] subStrings = value.Split(new char[] { ' ', ',' });
            Data = new byte[subStrings.Length - 6];

            FirstByte = Byte.Parse(subStrings[2]);
            for(uint i = 6u; i < subStrings.Length; ++i)
                Data[i - 6] = Byte.Parse(subStrings[i]);
        }

        /**
         * <summary>Initializes a deep copy of <paramref name="toCopy"/>.</summary>
         * 
         * <param name="toCopy">A Type26 to be copied.</param>
         */
        public Type26(Type26 toCopy)
        {
            FirstByte = toCopy.FirstByte;
            Data = (byte[])toCopy.Data.Clone();
        }
        #endregion

        #region Methods
        /**
         * <summary>Writes the Type26's binary representation with a <see cref="BinaryWriter"/>.</summary>
         * 
         * <param name="writer">The writer to be used.  It is not closed afterward.</param>
         */
        public void ToBinary(BinaryWriter writer)
        {
                writer.Write(FirstByte);
                writer.Write(Data);
        }

        /**
         * <summary>Outputs a human-readable representation of the Type26.</summary>
         * 
         * <returns>A string representing the data of the Type26.</returns>
         */
        public override string ToString()
        {
            int dataLength = Data.Length;
            StringBuilder builder = new StringBuilder(23 + 4 * dataLength);
            builder.AppendFormat("FirstByte = {0}, Data =", FirstByte);
            //XXX This loop can be simplified to an AppendJoin once .NET Standard 2.1 is released
            for(uint i = 0u; i < dataLength; ++i)
            {
                builder.Append(' ');
                builder.Append(Data[i]);
            }
            return builder.ToString();
        }
        #endregion

        #region Fields and Properties
        ///<summary>The data of the Type26.</summary>
        public byte[] Data { get; set; }

        ///<summary>The first byte of the Type26.</summary>
        public byte FirstByte { get; set; }
        #endregion
    }
}