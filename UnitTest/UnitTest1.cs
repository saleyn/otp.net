using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Otp;

namespace Otp
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        public UnitTest1()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
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

        [TestMethod]
        public void TestFormat()
        {
            {
                Erlang.Object obj1 = Erlang.Object.Format("a");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Atom));
                Assert.AreEqual("a", (obj1 as Erlang.Atom).atomValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("$a");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Char));
                Assert.AreEqual('a', (obj1 as Erlang.Char).charValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("'Abc'");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Atom));
                Assert.AreEqual("Abc", (obj1 as Erlang.Atom).atomValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{'true', 'false', true, false}");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Tuple));
                Erlang.Tuple t = obj1.Cast<Erlang.Tuple>();
                Assert.AreEqual(4, t.arity());
                foreach (Erlang.Object term in t.elements())
                    Assert.IsInstanceOfType(term, typeof(Erlang.Boolean));
                Assert.AreEqual(true, t[0].boolValue());
                Assert.AreEqual(false,t[1].boolValue());
                Assert.AreEqual(true, t[2].boolValue());
                Assert.AreEqual(false,t[3].boolValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("\"Abc\"");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.String));
                Assert.AreEqual("Abc", (obj1 as Erlang.String).stringValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("Abc");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Var));
                Assert.AreEqual("Abc", (obj1 as Erlang.Var).name());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("1");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Long));
                Assert.AreEqual(1, (obj1 as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("1.23");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Double));
                Assert.AreEqual(1.23, (obj1 as Erlang.Double).doubleValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("V");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Var));
                Assert.AreEqual("V", (obj1 as Erlang.Var).name());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{1}");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Tuple));
                Assert.AreEqual(1, (obj1 as Erlang.Tuple).arity());
                Assert.IsInstanceOfType((obj1 as Erlang.Tuple)[0], typeof(Erlang.Long));
                Assert.AreEqual(1, ((obj1 as Erlang.Tuple)[0] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj0 = Erlang.Object.Format("[]");
                Assert.IsInstanceOfType(obj0, typeof(Erlang.List));
                Assert.AreEqual(0, (obj0 as Erlang.List).arity());
                Erlang.Object obj1 = Erlang.Object.Format("[1]");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.List));
                Assert.AreEqual(1, (obj1 as Erlang.List).arity());
                Assert.IsInstanceOfType((obj1 as Erlang.List)[0], typeof(Erlang.Long));
                Assert.AreEqual(1, ((obj1 as Erlang.List)[0] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("[{1,2}, []]");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.List));
                Assert.AreEqual(2, (obj1 as Erlang.List).arity());
                Assert.IsInstanceOfType((obj1 as Erlang.List)[0], typeof(Erlang.Tuple));
                Assert.AreEqual(2, ((obj1 as Erlang.List)[0] as Erlang.Tuple).arity());
                Assert.AreEqual(0, ((obj1 as Erlang.List)[1] as Erlang.List).arity());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("{a, [b, 1, 2.0, \"abc\"], {1, 2}}");
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Tuple));
                Assert.AreEqual(3, (obj1 as Erlang.Tuple).arity());
            }
            {
                Erlang.Object obj1 = Erlang.Object.Format("~w", 1);
                Assert.IsInstanceOfType(obj1, typeof(Erlang.Long));
                Assert.AreEqual(1, (obj1 as Erlang.Long).longValue());
                Erlang.Object obj2 = Erlang.Object.Format("{~w, ~w,~w}", 1, 2, 3);
                Assert.IsInstanceOfType(obj2, typeof(Erlang.Tuple));
                Assert.AreEqual(3, (obj2 as Erlang.Tuple).arity());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[0], typeof(Erlang.Long));
                Assert.AreEqual(1, ((obj2 as Erlang.Tuple)[0] as Erlang.Long).longValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[1], typeof(Erlang.Long));
                Assert.AreEqual(2, ((obj2 as Erlang.Tuple)[1] as Erlang.Long).longValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[2], typeof(Erlang.Long));
                Assert.AreEqual(3, ((obj2 as Erlang.Tuple)[2] as Erlang.Long).longValue());
            }
            {
                Erlang.Object obj2 = Erlang.Object.Format("{~w, ~w,~w,~w, ~w}", 1.0, 'a', "abc", 2, true);
                Assert.IsInstanceOfType(obj2, typeof(Erlang.Tuple));
                Assert.AreEqual(5, (obj2 as Erlang.Tuple).arity());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[0], typeof(Erlang.Double));
                Assert.AreEqual(1.0, ((obj2 as Erlang.Tuple)[0] as Erlang.Double).doubleValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[1], typeof(Erlang.Char));
                Assert.AreEqual('a', ((obj2 as Erlang.Tuple)[1] as Erlang.Char).charValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[2], typeof(Erlang.String));
                Assert.AreEqual("abc", ((obj2 as Erlang.Tuple)[2] as Erlang.String).stringValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[3], typeof(Erlang.Long));
                Assert.AreEqual(2, ((obj2 as Erlang.Tuple)[3] as Erlang.Long).longValue());
                Assert.IsInstanceOfType((obj2 as Erlang.Tuple)[4], typeof(Erlang.Boolean));
                Assert.AreEqual(true, ((obj2 as Erlang.Tuple)[4] as Erlang.Boolean).booleanValue());
            }
        }
    }
}
