using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Otp;
using System.Collections.Concurrent;
using KVP = System.Collections.Generic.KeyValuePair<Otp.Erlang.PatternMatcher.Pattern, Otp.Erlang.VarBind>;

namespace Otp
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestFixture]
    public class UnitTest1
    {
        [Test]
        public void TestEncodeDecode()
        {
            {
                OtpOutputStream os = new OtpOutputStream(new Erlang.Atom("abc"));
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                Assert.IsTrue("abc" == ins.read_atom());
            }
            {
                OtpOutputStream os = new OtpOutputStream(new Erlang.String("string"));
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                Assert.IsTrue("string" == ins.read_string());
            }
            {
                Erlang.Pid pid = new Erlang.Pid("abc", 1, 2, 3);
                OtpOutputStream os = new OtpOutputStream(pid);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                Assert.IsTrue(pid.Equals(ins.read_pid()));
            }
            {
                Erlang.Port p = new Erlang.Port("abc", 1, 2);
                OtpOutputStream os = new OtpOutputStream(p);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                Assert.IsTrue(p.Equals(ins.read_port()));
            }
            {
                Erlang.Ref p = new Erlang.Ref("abc", new int[3] { 1, 2, 3 }, 2);
                OtpOutputStream os = new OtpOutputStream(p);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                Assert.IsTrue(p.Equals(ins.read_ref()));
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_long(1);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                long n = ins.read_long();
                Assert.IsTrue(1 == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_long(0xFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                long n = ins.read_long();
                Assert.IsTrue(0xFFFFFF == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_long(0xFFFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                long n = ins.read_long();
                Assert.IsTrue(0xFFFFFFFF == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_ulong((ulong)0xFFFFFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                ulong n = ins.read_ulong();
                Assert.IsTrue((ulong)0xFFFFFFFFFF == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_ulong((ulong)0xFFFFFFFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                ulong n = ins.read_ulong();
                Assert.IsTrue((ulong)0xFFFFFFFFFFFF == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_ulong((ulong)0xFFFFFFFFFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                ulong n = ins.read_ulong();
                Assert.IsTrue((ulong)0xFFFFFFFFFFFFFF == n);
            }
            {
                OtpOutputStream os = new OtpOutputStream();
                os.write_ulong((ulong)0xFFFFFFFFFFFFFFFF);
                OtpInputStream ins = new OtpInputStream(os.getBuffer(), 0, os.size());
                ulong n = ins.read_ulong();
                Assert.IsTrue((ulong)0xFFFFFFFFFFFFFFFF == n);
            }
        }

        [Test]
        public void TestPatternMatch()
        {
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format("{snapshot, x12, []}");
                Erlang.Object pat = Erlang.Object.Format("{snapshot, N, L}");

                Assert.IsTrue(pat.match(obj, binding));
                Erlang.Atom n = binding.find("N") as Erlang.Atom;
                Erlang.List l = binding.find("L") as Erlang.List;
                Assert.IsNotNull(n);
                Assert.IsNotNull(l);
                Assert.IsTrue(l.Length == 0);
            }
            {
                Erlang.Object pat = Erlang.Object.Format("{test, A, B, C}");
                Erlang.Object obj = Erlang.Object.Format("{test, 10, a, [1,2,3]}");

                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Assert.IsTrue(pat.match(obj, binding));
                Assert.AreEqual(3, binding.Count);
                Assert.AreEqual(10, binding["A"].longValue());
                Assert.AreEqual("a", binding["B"].atomValue());
                Assert.AreEqual("[1,2,3]", binding["C"].ToString());
            }

            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format("[1,a,$b,\"xyz\",{1,10.0},[]]");
                Erlang.Object pat = Erlang.Object.Format("[A,B,C,D,E,F]");

                Assert.IsTrue(pat.match(obj, binding));
                Assert.IsNotNull(binding.find("A") as Erlang.Long);
                Assert.IsNotNull(binding.find("B") as Erlang.Atom);
                Assert.IsNotNull(binding.find("C") as Erlang.Char);
                Assert.IsNotNull(binding.find("D") as Erlang.String);
                Assert.IsNotNull(binding.find("E") as Erlang.Tuple);
                Assert.IsNotNull(binding.find("F") as Erlang.List);
                
                Assert.IsTrue(binding.find("E").Cast<Erlang.Tuple>().arity() == 2);
                Assert.IsTrue(binding.find("F").Cast<Erlang.List>().Length == 0);
            }

            Erlang.Object pattern = Erlang.Object.Format("{test, T}");
            string exp = "{test, ~w}";
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, (int)3);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(3, binding.find("T").intValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, (long)100);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(100, binding.find("T").longValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, 100.0);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(100.0, binding.find("T").doubleValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, "test");
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual("test", binding.find("T").stringValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, true);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(true, binding.find("T").boolValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Object obj = Erlang.Object.Format(exp, 'c');
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual('c', binding.find("T").charValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Pid pid = new Erlang.Pid("tmp", 1, 2, 3);
                Erlang.Object obj = Erlang.Object.Format(exp, pid as Erlang.Object);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(pid, binding.find("T").pidValue());

                obj = Erlang.Object.Format(exp, pid);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(pid, binding.find("T").pidValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.Ref reference = new Erlang.Ref("tmp", 1, 2);
                Erlang.Object obj = Erlang.Object.Format(exp, reference);
                Assert.IsTrue(pattern.match(obj, binding));
                Assert.AreEqual(reference, binding.find("T").refValue());
            }
            {
                Erlang.VarBind binding = new Otp.Erlang.VarBind();
                Erlang.List obj = new Erlang.List(new Erlang.Int(10), new Erlang.Double(30.0),
                    new Erlang.String("abc"), new Erlang.Atom("a"),
                    new Erlang.Binary(new byte[] { 1, 2, 3 }), false, new Erlang.Boolean(true));
                Erlang.Object pat = Erlang.Object.Format("T");
                Assert.IsTrue(pat.match(obj, binding));
                Erlang.Object expected = Erlang.Object.Format("[10, 30.0, \"abc\", 'a', ~w, \'false\', true]",
                    new Erlang.Binary(new byte[] { 1, 2, 3 }));
                Erlang.Object result = binding.find("T");
                Assert.IsTrue(expected.Equals(result));
            }
        }

        [Test]
        public void TestFormat()
        {
            {
                Erlang.Object obj1 = Erlang.Object.Format("a");
                Assert.IsInstanceOf(typeof(Erlang.Atom), obj1);
                Assert.AreEqual("a", (obj1 as Erlang.Atom).atomValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("$a");
                Assert.IsInstanceOf(typeof(Erlang.Char), obj1);
                Assert.AreEqual('a', (obj1 as Erlang.Char).charValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("'Abc'");
                Assert.IsInstanceOf(typeof(Erlang.Atom), obj1);
                Assert.AreEqual("Abc", (obj1 as Erlang.Atom).atomValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{'true', 'false', true, false}");
                Assert.IsInstanceOf(typeof(Erlang.Tuple), obj1);
                Erlang.Tuple t = obj1.Cast<Erlang.Tuple>();
                Assert.AreEqual(4, t.arity());
                foreach (Erlang.Object term in t.elements())
                    Assert.IsInstanceOf(typeof(Erlang.Boolean), term);
                Assert.AreEqual(true, t[0].boolValue());
                Assert.AreEqual(false,t[1].boolValue());
                Assert.AreEqual(true, t[2].boolValue());
                Assert.AreEqual(false,t[3].boolValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("\"Abc\"");
                Assert.IsInstanceOf(typeof(Erlang.String), obj1);
                Assert.AreEqual("Abc", (obj1 as Erlang.String).stringValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("Abc");
                Assert.IsInstanceOf(typeof(Erlang.Var), obj1);
                Assert.AreEqual("Abc", (obj1 as Erlang.Var).name());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("1");
                Assert.IsInstanceOf(typeof(Erlang.Long), obj1);
                Assert.AreEqual(1, (obj1 as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("1.23");
                Assert.IsInstanceOf(typeof(Erlang.Double), obj1);
                Assert.AreEqual(1.23, (obj1 as Erlang.Double).doubleValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("V");
                Assert.IsInstanceOf(typeof(Erlang.Var), obj1);
                Assert.AreEqual("V", (obj1 as Erlang.Var).name());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{1}");
                Assert.IsInstanceOf(typeof(Erlang.Tuple), obj1);
                Assert.AreEqual(1, (obj1 as Erlang.Tuple).arity());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj1 as Erlang.Tuple)[0]);
                Assert.AreEqual(1, ((obj1 as Erlang.Tuple)[0] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj0 = Erlang.Object.Format("[]");
                Assert.IsInstanceOf(typeof(Erlang.List), obj0);
                Assert.AreEqual(0, (obj0 as Erlang.List).arity());
                Erlang.Object obj1 = Erlang.Object.Format("[1]");
                Assert.IsInstanceOf(typeof(Erlang.List), obj1);
                Assert.AreEqual(1, (obj1 as Erlang.List).arity());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj1 as Erlang.List)[0]);
                Assert.AreEqual(1, ((obj1 as Erlang.List)[0] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("[{1,2}, []]");
                Assert.IsInstanceOf(typeof(Erlang.List), obj1);
                Assert.AreEqual(2, (obj1 as Erlang.List).arity());
                Assert.IsInstanceOf(typeof(Erlang.Tuple), (obj1 as Erlang.List)[0]);
                Assert.AreEqual(2, ((obj1 as Erlang.List)[0] as Erlang.Tuple).arity());
                Assert.AreEqual(0, ((obj1 as Erlang.List)[1] as Erlang.List).arity());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{a, [b, 1, 2.0, \"abc\"], {1, 2}}");
                Assert.IsInstanceOf(typeof(Erlang.Tuple), obj1);
                Assert.AreEqual(3, (obj1 as Erlang.Tuple).arity());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("~w", 1);
                Assert.IsInstanceOf(typeof(Erlang.Long), obj1);
                Assert.AreEqual(1, (obj1 as Erlang.Long).longValue());
                Erlang.Object obj2 = Erlang.Object.Format("{~w, ~w,~w}", 1, 2, 3);
                Assert.IsInstanceOf(typeof(Erlang.Tuple), obj2);
                Assert.AreEqual(3, (obj2 as Erlang.Tuple).arity());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj2 as Erlang.Tuple)[0]);
                Assert.AreEqual(1, ((obj2 as Erlang.Tuple)[0] as Erlang.Long).longValue());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj2 as Erlang.Tuple)[1]);
                Assert.AreEqual(2, ((obj2 as Erlang.Tuple)[1] as Erlang.Long).longValue());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj2 as Erlang.Tuple)[2]);
                Assert.AreEqual(3, ((obj2 as Erlang.Tuple)[2] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj2 = Erlang.Object.Format("{~w, ~w,~w,~w, ~w}", 1.0, 'a', "abc", 2, true);
                Assert.IsInstanceOf(typeof(Erlang.Tuple), obj2);
                Assert.AreEqual(5, (obj2 as Erlang.Tuple).arity());
                Assert.IsInstanceOf(typeof(Erlang.Double), (obj2 as Erlang.Tuple)[0]);
                Assert.AreEqual(1.0, ((obj2 as Erlang.Tuple)[0] as Erlang.Double).doubleValue());
                Assert.IsInstanceOf(typeof(Erlang.Char), (obj2 as Erlang.Tuple)[1]);
                Assert.AreEqual('a', ((obj2 as Erlang.Tuple)[1] as Erlang.Char).charValue());
                Assert.IsInstanceOf(typeof(Erlang.String), (obj2 as Erlang.Tuple)[2]);
                Assert.AreEqual("abc", ((obj2 as Erlang.Tuple)[2] as Erlang.String).stringValue());
                Assert.IsInstanceOf(typeof(Erlang.Long), (obj2 as Erlang.Tuple)[3]);
                Assert.AreEqual(2, ((obj2 as Erlang.Tuple)[3] as Erlang.Long).longValue());
                Assert.IsInstanceOf(typeof(Erlang.Boolean), (obj2 as Erlang.Tuple)[4]);
                Assert.AreEqual(true, ((obj2 as Erlang.Tuple)[4] as Erlang.Boolean).booleanValue());
            }
        }

        [Test]
        public void TestFormatVariable()
        {
            var cases = new Dictionary<string, Erlang.TermType> {
                { "B",              Erlang.TermType.Object },
                { "B::int()",       Erlang.TermType.Int },
                { "B::integer()",   Erlang.TermType.Int },
                { "B::string()",    Erlang.TermType.String },
                { "B::atom()",      Erlang.TermType.Atom },
                { "B::float()",     Erlang.TermType.Double },
                { "B::double()",    Erlang.TermType.Double },
                { "B::binary()",    Erlang.TermType.Binary },
                { "B::bool()",      Erlang.TermType.Boolean },
                { "B::boolean()",   Erlang.TermType.Boolean },
                { "B::byte()",      Erlang.TermType.Byte },
                { "B::char()",      Erlang.TermType.Char },
                { "B::list()",      Erlang.TermType.List },
                { "B::tuple()",     Erlang.TermType.Tuple },
                { "B::pid()",       Erlang.TermType.Pid },
                { "B::ref()",       Erlang.TermType.Ref },
                { "B::reference()", Erlang.TermType.Ref },
                { "B::port()",      Erlang.TermType.Port }
            };

            foreach (var p in cases)
            {
                Erlang.Object o = Erlang.Object.Format(p.Key);
                Assert.IsInstanceOf(typeof(Erlang.Var), o);
                Assert.AreEqual(p.Value, o.Cast<Erlang.Var>().VarTermType);
            }

            var pat1 = Erlang.Object.Format("{A::char(), B::tuple(), C::float(), D::list(), [E::string(), F::int()], G::bool()}");
            var obj1 = Erlang.Object.Format("{$a, {1,2,3}, 10.0, [5,6], [\"abc\", 190], true}");

            var binding = new Erlang.VarBind();

            Assert.IsTrue(pat1.match(obj1, binding)); // Match unbound variables
            Assert.IsTrue(pat1.match(obj1, binding)); // Match bound variables

            var obj2 = Erlang.Object.Format("{$a, {1,2,3}, 20.0, [5,6], [\"abc\", 190], true}");

            Assert.IsFalse(pat1.match(obj2, binding)); // Match bound variables

            binding.clear();

            var obj3 = Erlang.Object.Format("{$a, {1,2,3}, 10.0, [5,6], [\"abc\", bad], false}");

            Assert.IsFalse(pat1.match(obj3, binding));
        }

        public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
        {
            public void Add(TKey key, TValue value)
            {
                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        [Test]
        public void TestMatchVariable()
        {
            var cases = new KeyValueList<string, Erlang.Object> {
                { "B",              new Erlang.Int(1) },
                { "B",              new Erlang.Atom("abc") },
                { "B",              new Erlang.String("efg") },
                { "B",              new Erlang.Double(10.0) },
                { "B::int()",       new Erlang.Int(10) },
                { "B::integer()",   new Erlang.Int(20) },
                { "B::string()",    new Erlang.String("xxx") },
                { "B::atom()",      new Erlang.Atom("xyz") },
                { "B::float()",     new Erlang.Double(5.0) },
                { "B::double()",    new Erlang.Double(3.0) },
                { "B::binary()",    new Erlang.Binary(new byte[] {1,2,3}) },
                { "B::bool()",      new Erlang.Boolean(true) },
                { "B::boolean()",   new Erlang.Boolean(false) },
                { "B::byte()",      new Erlang.Byte(1) },
                { "B::char()",      new Erlang.Char('a') },
                { "B::list()",      new Erlang.List(1, 2, 3) },
                { "B::tuple()",     new Erlang.Tuple(new Erlang.Char('a'), 1, "aaa") },
                { "B::pid()",       new Erlang.Pid("xxx", 1, 2, 3) },
                { "B::ref()",       new Erlang.Ref("xxx", 1, 3) },
                { "B::reference()", new Erlang.Ref("xxx", 1, 3) },
                { "B::port()",      new Erlang.Port("xxx", 1, 3) }
            };

            foreach (var p in cases)
            {
                {
                    Erlang.Object pat = Erlang.Object.Format(p.Key);
                    Erlang.Object obj = p.Value;

                    var binding = new Erlang.VarBind();
                    binding["B"] = obj;

                    Assert.IsTrue(pat.match(obj, binding));
                }

                {
                    Erlang.Object pat = Erlang.Object.Format(p.Key);
                    Erlang.Object obj = p.Value;

                    var binding = new Erlang.VarBind();

                    Assert.IsTrue(pat.match(obj, binding));

                    var b = binding["B"];

                    Assert.AreEqual(obj.Type, b.Type);
                    Assert.IsTrue(obj.Equals(b));
                }
            }

            var revCases = cases.Reverse<KeyValuePair<string,Erlang.Object>>().ToList();

            cases.Zip(revCases,
                (p1, p2) => {
                    Erlang.Var    pat = Erlang.Object.Format(p1.Key).AsVar();
                    Erlang.Object obj = p2.Value;

                    var binding = new Erlang.VarBind();

                    if (pat.VarTermType == Erlang.TermType.Object || pat.VarTermType == obj.TermType)
                        Assert.IsTrue(pat.match(obj, binding));
                    else
                        Assert.IsFalse(pat.match(obj, binding));

                    return false;
                }).ToList();
        }

        [Test]
        public void PatternMatchCollectionTest()
        {
            var state = new KVP();

            var pm = new Erlang.PatternMatcher {
                { 0, (_ctx, p, t, b, _args) => state = new KVP(p.ID, b), "{A::integer(), stop}"               },
                {          (p, t, b, _args) => state = new KVP(p.ID, b), "{A::integer(), status}"             },
                { 0, (_ctx, p, t, b, _args) => state = new KVP(p.ID, b), "{A::integer(), {status, B::atom()}}"},
                {          (p, t, b, _args) => state = new KVP(p.ID, b), "{A::integer(), {config, B::list()}}"}
            };

            Assert.AreEqual(1,  pm.Match(Erlang.Object.Format("{10, stop}")));
            Assert.AreEqual(10, state.Value["A"].intValue());

            Assert.AreEqual(2,  pm.Match(Erlang.Object.Format("{11, status}")));
            Assert.AreEqual(11, state.Value["A"].intValue());

            Assert.AreEqual(3,  pm.Match(Erlang.Object.Format("{12, {status, ~w}}", new Erlang.Atom("a"))));
            Assert.AreEqual(12, state.Value["A"].intValue());
            Assert.AreEqual("a",state.Value["B"].atomValue().ToString());

            Assert.AreEqual(4,  pm.Match(Erlang.Object.Format("{13, {config, ~w}}", new Erlang.List())));
            Assert.AreEqual(13, state.Value["A"].intValue());
            Assert.AreEqual(0,  state.Value["B"].listValue().Length);
                                
            Assert.AreEqual(-1, pm.Match(Erlang.Object.Format("{10, exit}")));

            var pts = pm.PatternsToTerm.ToString();

            Assert.AreEqual(
                "[{A::int(),stop},{A::int(),status},{A::int(),{status,B::atom()}},{A::int(),{config,B::list()}}]",
                pts);
        }

    }
}
