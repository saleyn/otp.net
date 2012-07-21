/*``The contents of this file are subject to the Erlang Public License,
* Version 1.1, (the "License"); you may not use this file except in
* compliance with the License. You should have received a copy of the
* Erlang Public License along with this software. If not, it can be
* retrieved via the world wide web at http://www.erlang.org/.
* 
* Software distributed under the License is distributed on an "AS IS"
* basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See
* the License for the specific language governing rights and limitations
* under the License.
* 
* The Initial Developer of the Original Code is Ericsson Utvecklings AB.
* Portions created by Ericsson are Copyright 1999, Ericsson Utvecklings
* AB. All Rights Reserved.''
* 
 * Converted from Java to C# by Vlad Dumitrescu (vlad_Dumitrescu@hotmail.com)
*/
namespace Otp
{
    using System;

    /*
    * Provides a stream for encoding Erlang terms to external format, for
    * transmission or storage.
    * 
    * <p> Note that this class is not synchronized, if you need
    * synchronization you must provide it yourself.
    *
    **/
    public class OtpOutputStream
    {
        /*The default initial size of the stream. **/
        public const int defaultInitialSize = 2048;
        
        /*The default increment used when growing the stream. **/
        public const int defaultIncrement = 2048;
        
        private byte[] buf = null;
        private int _size = 0;
        private int _count = 0;

        /*
        * Create a stream with the default initial size.
        **/
        public OtpOutputStream():this(defaultInitialSize)
        {
        }
        
        /*
        * Create a stream with the specified initial size.
        **/
        public OtpOutputStream(int size)
        {
            this._size = size;
            buf = new byte[size];
            _count = 0;
        }
        
        /*
        * Create a stream containing the serialized Erlang term.
        **/
        public OtpOutputStream(Erlang.Object o):this(o, false, false) {}

        /*
        * Create a stream containing the serialized Erlang term.
        * Optionally include in the beginning Erlang protocol version byte.
        **/
        public OtpOutputStream(Erlang.Object o, bool writeVersion)
            : this(o, writeVersion, false) {}

        /*
        * Create a stream containing the serialized Erlang term.
        * Optionally include in the beginning Erlang protocol version byte.
        **/
        public OtpOutputStream(Erlang.Object o, bool writeVersion, bool writePktSize)
            : this()
        {
            if (!writePktSize)
            {
                encodeObject(o, writeVersion);
                return;
            }

            write4BE(0); // make space for length data,
                         // but final value is not yet known
            encodeObject(o, writeVersion);
            poke4BE(0, this._size - 4);
        }

        private void encodeObject(Erlang.Object o, bool writeVersion)
        {
            if (writeVersion)
                this.write(OtpExternal.versionTag);
            this.write_any(o);
        }

        // package scope
        /*
        * Get the contents of the output stream as an input stream instead.
        * This is used internally in {@link OtpCconnection} for tracing
        * outgoing packages.
        *
        * @param offset where in the output stream to read data from when
        * creating the input stream. The offset is necessary because header
        * contents start 5 bytes into the header buffer, whereas payload
        * contents start at the beginning
        *
        * @return an input stream containing the same raw data.
        **/
        internal virtual OtpInputStream getOtpInputStream(int offset)
        {
            return new OtpInputStream(buf, offset, _count - offset);
        }
        
        public Erlang.Binary ToBinary()
        {
            byte[] tmp = new byte[_count];
            Array.Copy(buf, 0, tmp, 0, _count);
            return new Otp.Erlang.Binary(tmp);
        }

        /*
        * Reset the stream so that it can be reused.
        **/
        public virtual void reset()
        {
            _count = 0;
        }

        /*
        * Get internal buffer
        **/
        public byte[] getBuffer() { return buf; }

        /*
        * Get the current capacity of the stream. As bytes are added the
        * capacity of the stream is increased automatically, however this
        * method returns the current size.
        *
        * @return the size of the internal buffer used by the stream.
        **/
        public int size()
        {
            return _size;
        }

        /*
        * Get the current position in the stream.
        *
        * @return the current position in the stream.
        **/
        public int getPos()
        {
            return _count;
        }

        /*
        * Get the number of bytes in the stream.
        *
        * @return the number of bytes in the stream.
        **/
        public int count()
        {
            return _count;
        }

        /*
        * Write one byte to the stream.
        *
        * @param b the byte to write.
        *
        **/
        public virtual void  write(byte b)
        {
            if (_count >= _size)
            {
                // System.err.println("Expanding buffer from " + size + " to " + size + defaultIncrement);
                byte[] tmp = new byte[_size + defaultIncrement];
                Array.Copy(buf, 0, tmp, 0, _count);
                _size += defaultIncrement;
                buf = tmp;
            }
            
            buf[_count++] = b;
        }
        
        /*
        * Write an array of bytes to the stream.
        *
        * @param buf the array of bytes to write.
        *
        **/
        public virtual void  write(byte[] buf)
        {
            if (_count + buf.Length > _size)
            {
                // System.err.println("Expanding buffer from " +
                // size + " to " + buf.length + size + defaultIncrement);
                byte[] tmp = new byte[_size + buf.Length + defaultIncrement];
                Array.Copy(this.buf, 0, tmp, 0, _count);
                _size += defaultIncrement + buf.Length;
                this.buf = tmp;
            }
            
            Array.Copy(buf, 0, this.buf, _count, buf.Length);
            _count += (int) (buf.Length);
        }
        
        /*
        * Write the contents of the stream to an OutputStream.
        *
        * @param os the OutputStream to write to.
        *
        * @exception C#.io.IOException if there is an error writing to
        * the OutputStream.
        **/
        public virtual void  writeTo(System.IO.Stream os)
        {
            os.Write(buf, 0, _count);
            os.Flush();
        }

        public void writeTo(System.Net.Sockets.Socket sock)
        {
            sock.Send(buf, _count, System.Net.Sockets.SocketFlags.None);
        }

        /*
        * Write the low byte of a value to the stream.
        *
        * @param n the value to use.
        *
        **/
        public virtual void  write1(long n)
        {
            write((byte) (n & 0xff));
        }
        
        /*
        * Write an array of bytes to the stream.
        *
        * @param buf the array of bytes to write.
        *
        **/
        public virtual void  writeN(byte[] bytes)
        {
            write(bytes);
        }
        
        /*
        * Write the low two bytes of a value to the stream in big endian
        * order.
        *
        * @param n the value to use.
        **/
        public virtual void  write2BE(long n)
        {
            write((byte) ((n & 0xff00) >> 8));
            write((byte) (n & 0xff));
        }
        
        /*
        * Write the low four bytes of a value to the stream in big endian
        * order.
        *
        * @param n the value to use.
        **/
        public virtual void  write4BE(long n)
        {
            write((byte) ((n & 0xff000000) >> 24));
            write((byte) ((n & 0xff0000) >> 16));
            write((byte) ((n & 0xff00) >> 8));
            write((byte) (n & 0xff));
        }
        
        /*
        * Write the low two bytes of a value to the stream in little endian
        * order.
        *
        * @param n the value to use.
        **/
        public virtual void  write2LE(long n)
        {
            write((byte) (n & 0xff));
            write((byte) ((n & 0xff00) >> 8));
        }
        
        /*
        * Write the low four bytes of a value to the stream in little
        * endian order.
        *
        * @param n the value to use.
        **/
        public virtual void  write4LE(long n)
        {
            write((byte) (n & 0xff));
            write((byte) ((n & 0xff00) >> 8));
            write((byte) ((n & 0xff0000) >> 16));
            write((byte) ((n & 0xff000000) >> 24));
        }
        
        /*
        * Write the low four bytes of a value to the stream in bif endian
        * order, at the specified position. If the position specified is
        * beyond the end of the stream, this method will have no effect.
        *
        * Normally this method should be used in conjunction with {@link
        * #getPos() getPos()}, when is is necessary to insert data into the
        * stream before it is known what the actual value should be. For
        * example:
        *
        <pre>
        int pos = s.getPos();
        s.write4BE(0); // make space for length data,
        // but final value is not yet known
        
        [ ...more write statements...]
        
        // later... when we know the length value
        s.poke4BE(pos, length);
        </pre>
        
        *
        * @param offset the position in the stream.
        * @param n the value to use.
        **/
        public virtual void  poke4BE(int offset, int n)
        {
            if (offset < _count)
            {
                buf[offset + 0] = ((byte) ((n & 0xff000000) >> 24));
                buf[offset + 1] = ((byte) ((n & 0xff0000) >> 16));
                buf[offset + 2] = ((byte) ((n & 0xff00) >> 8));
                buf[offset + 3] = ((byte) (n & 0xff));
            }
        }
        
        /*
        * Write a string to the stream as an Erlang atom.
        *
        * @param atom the string to write.
        **/
        public virtual void  write_atom(System.String atom)
        {
            this.write1(OtpExternal.atomTag);
            this.write2BE(atom.Length);
            //UPGRADE_NOTE: This code will be optimized in the future;
            byte[] tmpBytes;
            int i;
            string tmpStr;
            tmpStr = atom;
            tmpBytes = new byte[tmpStr.Length];
            i = 0;
            while (i < tmpStr.Length)
            {
                tmpBytes[i] = (byte) tmpStr[i];
                i++;
            }
            this.writeN(tmpBytes);
        }

        public int encode_size(Erlang.Object o)
        {
            if (o is Erlang.Atom) return 1 + 2 + o.atomValue().Length;
            else if (o is Erlang.Boolean) return 1 + 2 + (o.boolValue()
                                                      ? Erlang.Boolean.s_true.atomValue().Length
                                                      : Erlang.Boolean.s_false.atomValue().Length);
            else if (o is Erlang.Binary) return 5 + o.binaryValue().Length;
            else if (o is Erlang.Long)
            {
                long l = o.longValue();
                if ((l & 0xff) == l) return 2;
                else if ((l <= OtpExternal.erlMax) && (l >= OtpExternal.erlMin)) return 5;
                return long_arity(l);
            }
            else if (o is Erlang.Byte) return 1 + 1;
            else if (o is Erlang.Double) return 9;
            else if (o is Erlang.String)
            {
                string l = o.stringValue();
                if (l.Length == 0) return 1;
                if (l.Length < 0xffff) return 2 + l.Length;
                return 1 + 4 + 2 * l.Length;

            }
            else if (o is Erlang.List)
            {
                Erlang.List l = o.listValue();
                if (l.arity() == 0)
                    return 1;
                int sz = 5;
                for (int i = 0; i < l.arity(); i++)
                    sz += encode_size(l[i]);
                return sz;
            }
            else if (o is Erlang.Tuple)
            {
                Erlang.Tuple l = o.tupleValue();
                int sz = 1 + (l.arity() < 0xff ? 1 : 4);
                for (int i = 0; i < l.arity(); i++)
                    sz += encode_size(l[i]);
                return sz;
            }
            else if (o is Erlang.Pid)
            {
                Erlang.Pid p = o.pidValue();
                return 1 + (1 + 2 + p.node().Length) + 4 + 4 + 1;
            }
            else if (o is Erlang.Ref)
            {
                Erlang.Ref p = o.refValue();
                int[] ids = p.ids();
                return 1 + (1 + 2 + p.node().Length) + 1 + 4 * ids.Length;
            }
            else if (o is Erlang.Port)
            {
                Erlang.Port p = o.portValue();
                return 1 + (1 + 2 + p.node().Length) + 4 + 1;
            }
            else
                throw new Erlang.Exception("Unknown encode size for object: " + o.ToString());
        }

        /*
        * Write an array of bytes to the stream as an Erlang binary.
        *
        * @param bin the array of bytes to write.
        **/
        public virtual void  write_binary(byte[] bin)
        {
            this.write1(OtpExternal.binTag);
            this.write4BE(bin.Length);
            this.writeN(bin);
        }
        
        /*
        * Write a boolean value to the stream as the Erlang atom 'true' or
        * 'false'.
        *
        * @param b the boolean value to write.
        **/
        public virtual void  write_boolean(bool b)
        {
            this.write_atom((b ? Erlang.Boolean.s_true : Erlang.Boolean.s_false).atomValue());
        }
        
        /*
        * Write a single byte to the stream as an Erlang integer.
        *
        * @param b the byte to use.
        **/
        public virtual void  write_byte(byte b)
        {
            this.write_long(b, false);
        }
        
        /*
        * Write a character to the stream as an Erlang integer.
        *
        * @param c the character to use.
        **/
        public virtual void  write_char(char c)
        {
            this.write_long(c, false);
        }
        
        /*
        * Write a double value to the stream. 
        *
        * @param d the double to use.
        **/
        public virtual void  write_double(double d)
        {
            this.write1(OtpExternal.newFloatTag);

            byte[] data = BitConverter.GetBytes(d);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(data);
            }
            this.write(data);
            /*
            double val;
            int exp = 0;
            int sign = 0;
            System.String str;
            
            // remove sign to simplify decimal shift
            if (d >= 0)
            {
                val = d;
            }
            else
            {
                sign = 1;
                val = - d;
            }
            
            // move the decimal point until we have a single units digit
            if (System.Math.Sign(val) != 0)
            {
                // until greater than or equal to 1.0  -> multiply by 10
                while (val < 1.0)
                {
                    val *= 10;
                    exp--;
                }
                // until strictly less than 10.0 -> divide by 10
                while (val >= 10.0)
                {
                    val /= 10;
                    exp++;
                }
            }

            // get 20 decimal digits, put sign back, add new exponent
            //UPGRADE_TODO: The equivalent in .NET for Class C#.math.BigDecimal.ROUND_HALF_EVEN will be considered in a future release.;
            //val = val.setScale(20, BigDecimal.ROUND_HALF_EVEN);
            //UPGRADE_TODO: The equivalent in .NET for Class C#.math.BigDecimal.toString will be considered in a future release.;
            str = (sign == 1?"-":"") + System.Convert.ToString(val) + "e" + System.Convert.ToString(exp);

            // write the value
            this.write1(OtpExternal.floatTag);
            //UPGRADE_NOTE: This code will be optimized in the future;
            byte[] tmpBytes;
            int i;
            string tmpStr;
            tmpStr = str;
            tmpBytes = new byte[tmpStr.Length];
            i = 0;
            while (i < tmpStr.Length)
            {
                tmpBytes[i] = (byte) tmpStr[i];
                i++;
            }
            this.writeN(tmpBytes);
            
            // pad with zeros to 31 bytes
            //UPGRADE_NOTE: This code will be optimized in the future;
            byte[] tmpBytes2;
            int i2;
            string tmpStr2;
            tmpStr2 = str;
            tmpBytes2 = new byte[tmpStr2.Length];
            i2 = 0;
            while (i2 < tmpStr2.Length)
            {
                tmpBytes2[i2] = (byte) tmpStr2[i2];
                i2++;
            }
            int i3 = (int) (tmpBytes2.Length);
             for (; i3 < 31; i3++)
                this.write1(0);
            */
        }
        
        
        /*
        * Write a float value to the stream. 
        *
        * @param f the float to use.
        **/
        public virtual void  write_float(float f)
        {
            this.write_double(f);
        }
        
        /*
        * Write a long to the stream.
        *
        * @param l the long to use.
        **/
        public void write_long(long l) { write_long(l, l < 0); }

        private void write_long(long l, bool isNegative)
        {
            if ((l & 0xff) == l)
            {
                // will fit in one byte
                this.write1(OtpExternal.smallIntTag);
                this.write1(l);
            }
            else if ((l <= OtpExternal.erlMax) && (l >= OtpExternal.erlMin))
            {
                this.write1(OtpExternal.intTag);
                this.write4BE(l);
            }
            else
            {
                int  neg = isNegative ? 1 : 0;
                ulong v  = (ulong)(isNegative ? -l : l);
                byte arity = 0;
                this.write1(OtpExternal.smallBigTag);
                int arity_pos = _count;
                this.write1(0);  // Fill in later
                this.write1(neg); // sign
                while (v != 0) {
                    this.write1((byte)(v & 0xff));  // write lowest byte
                    v >>= 8;                // shift unsigned
                    arity++;
                }
                buf[arity_pos] = arity;
            }
        }

        static public int long_arity(long l)
        {
            ulong v = (ulong)(l < 0 ? -l : l);
            int sz = 3; /* Type, arity and sign */
            while (v != 0) { sz++; v >>= 8; }
            return sz;
        }

        /*
        * Write a positive long to the stream.
        *
        * @param ul the long to use.
        **/
        public virtual void  write_ulong(ulong ul)
        {
            this.write_long((long)ul, false);
        }
        
        /*
        * Write an integer to the stream.
        *
        * @param i the integer to use.
        **/
        public virtual void  write_int(int i)
        {
            this.write_long(i, i < 0);
        }
        
        /*
        * Write a positive integer to the stream.
        *
        * @param ui the integer to use.
        **/
        public virtual void  write_uint(uint ui)
        {
            this.write_long(ui, false);
        }
        
        /*
        * Write a short to the stream.
        *
        * @param s the short to use.
        **/
        public virtual void  write_short(short s)
        {
            this.write_long(s, false);
        }
        
        /*
        * Write a positive short to the stream.
        *
        * @param s the short to use.
        **/
        public virtual void write_ushort(ushort us)
        {
            this.write_long(us, false);
        }
        
        /*
        * Write an Erlang list header to the stream. After calling this
        * method, you must write 'arity' elements to the stream followed by
        * nil, or it will not be possible to decode it later.
        *
        * @param arity the number of elements in the list.
        **/
        public virtual void  write_list_head(int arity)
        {
            if (arity == 0)
            {
                this.write_nil();
            }
            else
            {
                this.write1(OtpExternal.listTag);
                this.write4BE(arity);
            }
        }
        
        /*
        * Write an empty Erlang list to the stream.
        **/
        public virtual void  write_nil()
        {
            this.write1(OtpExternal.nilTag);
        }
        
        /*
        * Write an Erlang tuple header to the stream. After calling this
        * method, you must write 'arity' elements to the stream or it will
        * not be possible to decode it later.
        *
        * @param arity the number of elements in the tuple.
        **/
        public virtual void  write_tuple_head(int arity)
        {
            if (arity < 0xff)
            {
                this.write1(OtpExternal.smallTupleTag);
                this.write1(arity);
            }
            else
            {
                this.write1(OtpExternal.largeTupleTag);
                this.write4BE(arity);
            }
        }
        
        /*
        * Write an Erlang PID to the stream. 
        *
        * @param node the nodename.
        *
        * @param id an arbitrary number. Only the low order 15 bits will
        * be used.
        *
        * @param serial another arbitrary number. Only the low order 3 bits
        * will be used.
        *
        * @param creation yet another arbitrary number. Only the low order
        * 2 bits will be used.
        * 
        **/
        public virtual void  write_pid(System.String node, int id, int serial, int creation)
        {
            this.write1(OtpExternal.pidTag);
            this.write_atom(node);
            this.write4BE(id & 0x7fff); // 15 bits
            this.write4BE(serial & 0x7); // 3 bits
            this.write1(creation & 0x3); // 2 bits
        }
        
        /*
        * Write an Erlang port to the stream. 
        *
        * @param node the nodename.
        *
        * @param id an arbitrary number. Only the low order 18 bits will
        * be used.
        *
        * @param creation another arbitrary number. Only the low order 2
        * bits will be used.
        * 
        **/
        public virtual void  write_port(System.String node, int id, int creation)
        {
            this.write1(OtpExternal.portTag);
            this.write_atom(node);
            this.write4BE(id & 0x3ffff); // 18 bits
            this.write1(creation & 0x3); // 2 bits
        }
        
        /*
        * Write an old style Erlang ref to the stream. 
        *
        * @param node the nodename.
        *
        * @param id an arbitrary number. Only the low order 18 bits will
        * be used.
        *
        * @param creation another arbitrary number. Only the low order 2
        * bits will be used.
        * 
        **/
        public virtual void  write_ref(System.String node, int id, int creation)
        {
            this.write1(OtpExternal.refTag);
            this.write_atom(node);
            this.write4BE(id & 0x3ffff); // 18 bits
            this.write1(creation & 0x3); // 2 bits
        }
        
        /*
        * Write a new style (R6 and later) Erlang ref to the stream.
        *
        * @param node the nodename.
        *
        * @param ids an array of arbitrary numbers. Only the low order 18
        * bits of the first number will be used. If the array contains only
        * one number, an old style ref will be written instead. At most
        * three numbers will be read from the array.
        *
        * @param creation another arbitrary number. Only the low order
        * 2 bits will be used.
        * 
        **/
        public virtual void  write_ref(System.String node, int[] ids, int creation)
        {
            int arity = (int) (ids.Length);
            if (arity > 3)
                arity = 3;
            // max 3 words in ref
            
            if (arity == 1)
            {
                // use old method
                this.write_ref(node, ids[0], creation);
            }
            else
            {
                // r6 ref
                this.write1(OtpExternal.newRefTag);
                
                // how many id values
                this.write2BE(arity);
                
                this.write_atom(node);
                
                // note: creation BEFORE id in r6 ref
                this.write1(creation & 0x3); // 2 bits
                
                // first int gets truncated to 18 bits
                this.write4BE(ids[0] & 0x3ffff);
                
                // remaining ones are left as is
                 for (int i = 1; i < arity; i++)
                    this.write4BE(ids[i]);
            }
        }
        
        /*
        * Write a string to the stream.
        *
        * @param s the string to write.
        **/
        public virtual void  write_string(System.String s)
        {
            int len = s.Length;
            
            switch (len)
            {
                case 0: 
                    this.write_nil();
                    break;
                
                default:
                    System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                    Byte[] bytebuf = encoding.GetBytes(s);
                    if (bytebuf.Length < 0xffff)
                    {
                        this.write1(OtpExternal.stringTag);
                        this.write2BE(len);
                        this.writeN(bytebuf);
                    }
                    else
                    {
                        this.write_list_head(len);
                        for (int i2 = 0; i2 < len; i2++)
                            this.write_byte(bytebuf[i2]);
                        this.write_nil();
                    }

                    /*
                    //UPGRADE_NOTE: This code will be optimized in the future;
                    byte[] tmpBytes;
                    int i;
                    string tmpStr;
                    tmpStr = s;
                    tmpBytes = new byte[tmpStr.Length];
                    i = 0;
                    while (i < tmpStr.Length)
                    {
                        tmpBytes[i] = (byte) tmpStr[i];
                        i++;
                    }
                    byte[] bytebuf = tmpBytes;

                    if (bytebuf.Length == len)
                    {
                        // Usual
                        this.write1(OtpExternal.stringTag);
                        this.write2BE(len);
                        this.writeN(bytebuf);
                    }
                    else
                    {
                        // Unicode
                        char[] charbuf = s.ToCharArray();
                        
                        this.write_list_head(len);
                        
                         for (int i2 = 0; i2 < len; i2++)
                            this.write_char(charbuf[i2]);
                        
                        this.write_nil();
                    }
                    */
                    break;
                
            }
        }
        
        
        /*
        This does not work when char > 1 byte Unicode is used
        
        public void write_string(String s) {
        this.write1(OtpExternal.stringTag);
        this.write2BE(s.length());
        this.writeN(s.getBytes());
        }*/
        
        /*
        * Write an arbitrary Erlang term to the stream.
        *
        * @param o the Erlang term to write.
        */
        public virtual void write_any(Erlang.Object o)
        {
            // calls one of the above functions, depending on o
            o.encode(this);
        }

        static void write_any(ref byte[] buf, Erlang.Object o)
        {
            
        }
    }
}