#region Usings

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Sbatman.Networking
{
    /// <summary>
    ///     The Packet class is a light class that is used for serialising and deserialising data into and from the network
    ///     stream.
    ///     This allows easy passing over variables over the network and guarantees order.
    /// </summary>
    public class Packet : IDisposable
    {
        /// <summary>
        ///     This is the initial size of the internal byte array size of the packet
        /// </summary>
        private const int INITAL_DATA_SIZE = 128;

        /// <summary>
        ///     This 4 byte sequence is used to improve start of packet regognition, it isnt the sole desciptor of the packet start
        ///     as this would possibly cause issues
        ///     with packets with byte transferes within them that happened to contains this sequence.
        /// </summary>
        public static readonly byte[] PacketStart = { 0, 48, 21, 0 };

        /// <summary>
        ///     The type id of the packet
        /// </summary>
        public readonly UInt16 Type;

        /// <summary>
        ///     The internal data array of the packet
        /// </summary>
        protected byte[] _Data = new byte[INITAL_DATA_SIZE];

        /// <summary>
        ///     The current position in the internal data array
        /// </summary>
        protected UInt32 _DataPos;

        /// <summary>
        ///     Whether the packet is disposed
        /// </summary>
        protected bool _Disposed;

        /// <summary>
        ///     A copy of all the bojects packed in this packet
        /// </summary>
        protected List<object> _PacketObjects;

        /// <summary>
        ///     The number of paramerters that are stored in the packet
        /// </summary>
        protected UInt16 _ParamCount;

        /// <summary>
        ///     A temp copy of the byte array generated by this packet, this is used as a cache for packets with multiple targets
        /// </summary>
        protected byte[] _ReturnByteArray;

        /// <summary>
        ///     Creates a new packet with the specified type id
        /// </summary>
        /// <param name="type">The packets type ID</param>
        public Packet(UInt16 type)
        {
            Type = type;
        }

        /// <summary>
        ///     Disposes the packet, destroying all internals buffers and caches
        /// </summary>
        public void Dispose()
        {
            _ReturnByteArray = null;
            if (_Disposed) return;
            _Disposed = true;
            if (_PacketObjects != null)
            {
                _PacketObjects.Clear();
                _PacketObjects = null;
            }
            _Data = null;
        }

        /// <summary>
        ///     Creates a deep copy of this packet
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public Packet Copy()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            Packet p = new Packet(Type)
            {
                _Data = new byte[_Data.Length]
            };
            _Data.CopyTo(p._Data, 0);
            p._DataPos = _DataPos;
            if (_PacketObjects != null) p._PacketObjects = new List<object>(_PacketObjects);
            p._ParamCount = _ParamCount;
            p._ReturnByteArray = _ReturnByteArray;
            return p;
        }


        /// <summary>
        ///     Adds a double to the packet
        /// </summary>
        /// <param name="d">double to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddDouble(Double d)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 9 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.DOUBLE;
            BitConverter.GetBytes(d).CopyTo(_Data, (int)_DataPos);
            _DataPos += 8;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a byte array to the packet
        /// </summary>
        /// <param name="byteArray">The bytearray to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddBytePacket(byte[] byteArray)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            UInt32 size = (UInt32) byteArray.Length;
            while (_DataPos + (size + 5) >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.BYTE_PACKET;
            BitConverter.GetBytes(byteArray.Length).CopyTo(_Data, (int)_DataPos);
            _DataPos += 4;
            byteArray.CopyTo(_Data, (int)_DataPos);
            _DataPos += size;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a float to the packet
        /// </summary>
        /// <param name="f">The float to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddFloat(float f)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 5 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.FLOAT;
            BitConverter.GetBytes(f).CopyTo(_Data, (int)_DataPos);
            _DataPos += 4;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a boolean to the packet
        /// </summary>
        /// <param name="b">The bool to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddBool(bool b)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 5 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.BOOL;
            BitConverter.GetBytes(b).CopyTo(_Data, (int)_DataPos);
            _DataPos += 1;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a long to the packet
        /// </summary>
        /// <param name="l">The long to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddInt64(Int64 l)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 9 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.INT64;
            BitConverter.GetBytes(l).CopyTo(_Data, (int)_DataPos);
            _DataPos += 8;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds an int32 to the packet
        /// </summary>
        /// <param name="i">The int 32 to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddInt32(Int32 i)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 5 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.INT32;
            BitConverter.GetBytes(i).CopyTo(_Data, (int)_DataPos);
            _DataPos += 4;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds an int64 to the packet
        /// </summary>
        /// <param name="i">The int64 to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddUInt64(UInt64 i)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 9 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.UINT64;
            BitConverter.GetBytes(i).CopyTo(_Data, (int)_DataPos);
            _DataPos += 8;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds an Int16 to the packet
        /// </summary>
        /// <param name="i">The int16 to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddInt16(Int16 i)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 3 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.INT16;
            BitConverter.GetBytes(i).CopyTo(_Data,(int) _DataPos);
            _DataPos += 2;
            _ParamCount++;
        }
        /// <summary>
        ///     Adds an Int16 to the packet
        /// </summary>
        /// <param name="i">The int16 to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddUInt16(UInt16 i)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 3 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.UINT16;
            BitConverter.GetBytes(i).CopyTo(_Data, (int)_DataPos);
            _DataPos += 2;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a Uint32 to the packet
        /// </summary>
        /// <param name="u">The uint32 to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddUInt32(UInt32 u)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            _ReturnByteArray = null;
            while (_DataPos + 5 >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.UINT32;
            BitConverter.GetBytes(u).CopyTo(_Data, (int)_DataPos);
            _DataPos += 4;
            _ParamCount++;
        }

        /// <summary>
        ///     Adds a UTF8 String to the packet
        /// </summary>
        /// <param name="s">The String to add</param>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public void AddString(string s)
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            byte[] byteArray = Encoding.UTF8.GetBytes(s);
            _ReturnByteArray = null;
            UInt32 size = (UInt32)byteArray.Length;
            while (_DataPos + (size + 5) >= _Data.Length) ExpandDataArray();
            _Data[_DataPos++] = (byte)ParamTypes.UTF8_STRING;
            BitConverter.GetBytes(byteArray.Length).CopyTo(_Data, (int)_DataPos);
            _DataPos += 4;
            byteArray.CopyTo(_Data, (int)_DataPos);
            _DataPos += size;
            _ParamCount++;
        }

        /// <summary>
        ///     Converts the back to a bytearray
        /// </summary>
        /// <returns>A byte array representing the packet</returns>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public byte[] ToByteArray()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            if (_ReturnByteArray != null) return _ReturnByteArray;
            _ReturnByteArray = new byte[12 + _DataPos];
            PacketStart.CopyTo(_ReturnByteArray, 0);
            BitConverter.GetBytes(_ParamCount).CopyTo(_ReturnByteArray, 4);
            BitConverter.GetBytes(12 + _DataPos).CopyTo(_ReturnByteArray, 6);
            BitConverter.GetBytes(Type).CopyTo(_ReturnByteArray, 10);
            Array.Copy(_Data, 0, _ReturnByteArray, 12, (int)_DataPos);
            return _ReturnByteArray;
        }

        /// <summary>
        ///     Converts a byte array to a packet
        /// </summary>
        /// <param name="data">the byte array to convery</param>
        /// <returns>Returns a packet build from a byte array</returns>
        public static Packet FromByteArray(byte[] data)
        {
            Packet returnPacket = new Packet(BitConverter.ToUInt16(data, 10))
            {
                _ParamCount = BitConverter.ToUInt16(data, 4),
                _Data = new byte[BitConverter.ToInt32(data, 6) - 12]
            };
            returnPacket._DataPos = (UInt32)returnPacket._Data.Length;
            Array.Copy(data, 12, returnPacket._Data, 0, returnPacket._Data.Length);
            returnPacket.UpdateObjects();
            return returnPacket;
        }

        /// <summary>
        ///     Returns the list of objects within this packet
        /// </summary>
        /// <returns>An array of the contained objects</returns>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        public object[] GetObjects()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            return _PacketObjects.ToArray();
        }

        /// <summary>
        ///     Ensures the packet bojects array correctly represents the objects that should be within this packet
        /// </summary>
        /// <exception cref="ObjectDisposedException">Will throw if packet is disposed</exception>
        protected void UpdateObjects()
        {
            if (_Disposed) throw new ObjectDisposedException(ToString());
            if (_PacketObjects != null)
            {
                _PacketObjects.Clear();
                _PacketObjects = null;
            }
            _PacketObjects = new List<Object>(_ParamCount);
            int bytepos = 0;
            try
            {
                for (int x = 0; x < _ParamCount; x++)
                {
                    switch ((ParamTypes)_Data[bytepos++])
                    {
                        case ParamTypes.DOUBLE:
                            _PacketObjects.Add(BitConverter.ToDouble(_Data, bytepos));
                            bytepos += 8;
                            break;
                        case ParamTypes.FLOAT:
                            _PacketObjects.Add(BitConverter.ToSingle(_Data, bytepos));
                            bytepos += 4;
                            break;
                        case ParamTypes.INT32:
                            _PacketObjects.Add(BitConverter.ToInt32(_Data, bytepos));
                            bytepos += 4;
                            break;
                        case ParamTypes.BOOL:
                            _PacketObjects.Add(BitConverter.ToBoolean(_Data, bytepos));
                            bytepos += 1;
                            break;
                        case ParamTypes.INT64:
                            _PacketObjects.Add(BitConverter.ToInt64(_Data, bytepos));
                            bytepos += 8;
                            break;
                        case ParamTypes.BYTE_PACKET:
                            byte[] data = new byte[BitConverter.ToInt32(_Data, bytepos)];
                            bytepos += 4;
                            Array.Copy(_Data, bytepos, data, 0, data.Length);
                            _PacketObjects.Add(data);
                            bytepos += data.Length;
                            break;
                        case ParamTypes.UINT32:
                            _PacketObjects.Add(BitConverter.ToUInt32(_Data, bytepos));
                            bytepos += 4;
                            break;
                        case ParamTypes.UINT64:
                            _PacketObjects.Add(BitConverter.ToUInt64(_Data, bytepos));
                            bytepos += 8;
                            break;
                        case ParamTypes.INT16:
                            _PacketObjects.Add(BitConverter.ToInt16(_Data, bytepos));
                            bytepos += 2;
                            break;
                        case ParamTypes.UTF8_STRING:
                            byte[] data3 = new byte[BitConverter.ToInt32(_Data, bytepos)];
                            bytepos += 4;
                            Array.Copy(_Data, bytepos, data3, 0, data3.Length);
                            _PacketObjects.Add(Encoding.UTF8.GetString(data3, 0, data3.Length));
                            bytepos += data3.Length;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
             
            }
        }

        /// <summary>
        ///     Increases the size of the internal data array
        /// </summary>
        protected void ExpandDataArray()
        {
            _ReturnByteArray = null;
            byte[] newData = new byte[_Data.Length * 2];
            _Data.CopyTo(newData, 0);
            _Data = newData;
        }     

        /// <summary>
        ///     An enum containing supported types
        /// </summary>
        protected enum ParamTypes
        {
            FLOAT,
            DOUBLE,
            INT16,
            UINT16,
            INT32,
            UINT32,
            INT64,
            UINT64,
            BOOL,
            BYTE_PACKET,
            UTF8_STRING,
        };
    }
}