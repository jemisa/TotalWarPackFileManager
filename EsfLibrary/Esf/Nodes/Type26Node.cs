using System.IO;

namespace EsfLibrary
{
    /**
     * <summary>A <see cref="CodecNode{Type26}"/> to represent data stored in an ESF file entry with an <see cref="EsfType"/> of <see cref="EsfType.UNKNOWN_26"/>.</summary>
     * TODO verify properties inherited from EsfNode are properly set by constructors rather than after creation in other code (The copy constructor likely fails this, others are uncertain).
     */
    public class Type26Node: CodecNode<Type26>
    {
        #region Constructors
        ///<summary>A generic constructor for Type26Nodes.</summary>
        public Type26Node() : base(Parse) { }

        /**
         * <summary>A constructor that instantiates a Type26Node with the given data.</summary>
         * 
         * <param name="value">A <see cref="Type26"/> containing the data that this node should contain.</param>
         */
        public Type26Node(Type26 value) : base(Parse)
        {
            Value = value;
        }

        /**
         * <summary>A copy constructor for Type26Nodes.</summary>
         * 
         * <param name="toCopy">The Type26Node to be copied.</param>
         */
        public Type26Node(Type26Node toCopy) : this()
        {
            Value = new Type26(toCopy.Value);
        }
        #endregion

        #region Methods
        /**
         * <summary>A copy function inherited from <see cref="EsfNode"/>.</summary>
         * <remarks>The implementation of this is equivalent to calling the copy constructor.</remarks>
         * 
         * <returns>A deep copy of the calling object.</returns>
         */
        public override EsfNode CreateCopy() => new Type26Node(this);

        ///<inheritdoc />
        public override void WriteValue(BinaryWriter writer) => Value.ToBinary(writer);

        ///<inheritdoc />
        protected override Type26 ReadValue(BinaryReader reader, EsfType readAs)
        {
            return new Type26(reader);
        }

        /**
         * <summary>Creates a copy of the node's underlying data from a string encoding.</summary>
         * 
         * <param name="value">A string representation of a Type26Node's underlying data to be parsed.</param>
         * <returns>A <see cref="Type26"/> representing the parsed data from <paramref name="value"/>.</returns>
         * 
         * TODO It may make sense to make this method an inherited part of the base class.
         */
        static Type26 Parse(string value) => new Type26(value);
        #endregion
    }
}
