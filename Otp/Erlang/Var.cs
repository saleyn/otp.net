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
* Copyright (c) 2011 Serge Aleynikov <saleyn@gmail.com>
*/
namespace Otp.Erlang
{
    using System;

    /*
    * Provides a C# representation of Erlang strings. 
    **/
    [Serializable]
    public class Var : Erlang.Object
    {
        private static readonly string s_any = "_";

        private string m_var;
        private TermType m_termType;

        /// <summary>
        /// Create an anonymous variable.
        /// </summary>
        public Var() : this(s_any) {}

        /// <summary>
        /// Create an Erlang named variable
        /// </summary>
        /// <param name="name">Variable name</param>
        public Var(string name) : this(name, TermType.Object) {}

        /// <summary>
        /// Create an Erlang typed named variable
        /// </summary>
        /// <param name="name">Variable name</param>
        public Var(string name, TermType type)
        {
            this.m_var = name;
            this.m_termType = type;
        }
        
        public static string Any { get { return s_any; } }

        /// <summary>
        /// Returns type of this class
        /// </summary>
        public override Type Type { get { return GetType(); } }
        
        public override TermType TermType { get { return TermType.Var; } }

        /// <summary>
        /// Returns type of value stored in this variable
        /// </summary>
        public TermType VarTermType { get { return m_termType; } }

        public bool isAny() { return m_var == s_any; }
        /*
        * Get the actual string contained in this object.
        *
        * @return the raw string contained in this object, without regard
        * to Erlang quoting rules.
        * 
        * @see #toString
        **/
        public string name()
        {
            return m_var;
        }

        /*
        * Get the printable version of the string contained in this object.
        *
        * @return the string contained in this object, quoted.
        * 
        * @see #stringValue
        **/
        public override string ToString()
        {
            string tp;
            switch (m_termType)
            {
                case Erlang.TermType.Atom:      tp = "::atom()"; break;
                case Erlang.TermType.Binary:    tp = "::binary()"; break;
                case Erlang.TermType.Boolean:   tp = "::bool()"; break;
                case Erlang.TermType.Byte:      tp = "::byte()"; break;
                case Erlang.TermType.Char:      tp = "::char()"; break;
                case Erlang.TermType.Double:    tp = "::double()"; break;
                case Erlang.TermType.Int:       tp = "::int()"; break;
                case Erlang.TermType.List:      tp = "::list()"; break;
                case Erlang.TermType.Pid:       tp = "::pid()"; break;
                case Erlang.TermType.Port:      tp = "::port()"; break;
                case Erlang.TermType.Ref:       tp = "::ref()"; break;
                case Erlang.TermType.String:    tp = "::string()"; break;
                case Erlang.TermType.Tuple:     tp = "::tuple()"; break;
                case Erlang.TermType.Var:       tp = "::var()"; break;
                default: tp = string.Empty; break;
            }
            return tp == string.Empty
                ? string.Format("\"{0}\"", m_var)
                : string.Format("{0}{1}", m_var, tp);
        }

        public override bool subst(ref Erlang.Object obj, Erlang.VarBind binding)
        {
            if (isAny() || binding == null || binding.Empty)
                throw new UnboundVarException();
            Erlang.Object term = binding[m_var];
            if (term == null)
                throw new UnboundVarException("Variable " + m_var + " not bound!");
            if (!checkType(term))
                throw new InvalidValueType(
                    string.Format("Invalid variable {0} value type (got={1}, expected={2})",
                        m_var, obj.Type, m_termType));
            obj = term;
            return true;
        }

        public override bool match(Erlang.Object pattern, Erlang.VarBind binding)
        {
            if (binding == null)
                return false;
            Erlang.Object value = binding.find(m_var);
            if (value != null)
                return checkType(value) ? value.match(pattern, binding) : false;
            else if (!checkType(pattern))
                return false;
            Erlang.Object term = null;
            binding[m_var] = pattern.subst(ref term, binding) ? term : pattern;
            return true;
        }

        /*
        * Convert this string to the equivalent Erlang external representation.
        *
        * @param buf an output stream to which the encoded string should be
        * written.
        **/
        public override void encode(OtpOutputStream buf)
        {
            throw new EncodeException("Cannot encode vars!");
        }

        /*
        * Determine if two strings are equal. They are equal if they
        * represent the same sequence of characters. This method can be
        * used to compare Strings with each other and with
        * Strings.
        *
        * @param o the String or String to compare to.
        *
        * @return true if the strings consist of the same sequence of
        * characters, false otherwise. 
        **/
        public override bool Equals(System.Object o)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return 1;
        }

        private bool checkType(Erlang.Object value)
        {
            var vt = value.TermType;
            return m_termType == TermType.Object || vt == m_termType;
        }
    }
}
