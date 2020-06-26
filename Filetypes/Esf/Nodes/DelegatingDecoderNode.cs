using System.IO;

namespace Filetypes
{
    ///<summary>A <see cref="CodecNode{T}"/> that takes its binary read and write functions as delegates on creation.</summary>
    public abstract class DelegatingDecoderNode<T>: CodecNode<T>
    {
        #region Constructors
        /**
         * <summary>A constructor that stores delegates for reading a representation of a Node from a String or<see cref= "BinaryReader" /> as well as writing the Node's data with a <see cref="BinaryWriter"/>.</summary>
         * 
         * <param name = "conv" > A delegate that converts a String representation of the Node to a <typeparamref name = "T" /> representation of the Node's underlying data.</param>
         * <param name = "reader" > A delegate that reads a binary representation of the Node and creates a<typeparamref name="T"/> representation of the Node's underlying data.</param>
         * <param name = "writer" > A delegate that writes a binary representation of the Node's underlying data.</param>
         */
        public DelegatingDecoderNode(Converter<T> conv, ValueReader<T> reader, ValueWriter<T> writer) : base(conv)
        {
            Read = reader;
            Write = writer;
        }
        #endregion

        #region Methods
        ///<inheritdoc />
        protected override T ReadValue(BinaryReader reader, EsfType readAs) => Read(reader);

        ///<inheritdoc />
        public override void WriteValue(BinaryWriter writer) => Write(writer, Value);
        #endregion

        #region Fields and Properties
        ///<summary>A delegate for storing a function for reading the Node's data from a <see cref="BinaryReader"/>.</summary>
        protected ValueReader<T> Read;
        ///<summary>A delegate for storing a function for writing the Node's data using a <see cref="BinaryWriter"/>.</summary>
        protected ValueWriter<T> Write;
        #endregion
    }
}
