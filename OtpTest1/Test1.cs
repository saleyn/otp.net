using System;
using System.Collections.Generic;
using System.Text;
using Otp;

namespace Otp
{
    class Test1
    {
        private static void OnReadWrite(AbstractConnection con, AbstractConnection.Operation op,
            long lastBytes, long totalBytes, long totalMsgs)
        {
            System.Console.WriteLine(String.Format("{0} {1} bytes (total: {2} bytes, {3} msgs)",
                (op == AbstractConnection.Operation.Read ? "Read " : "Written "),
                lastBytes, totalBytes, totalMsgs));
        }

		static public void Main(String[] args)
        {
            System.Console.Out.WriteLine("Otp test...");

            string cookie = OtpNode.defaultCookie;
            string host = System.Net.Dns.GetHostName();
            string remote = (args[0].IndexOf('@') < 0) ? args[0] + "@" + host : args[0];
            string nodename = Environment.UserName + "123@" + host;

            AbstractConnection.traceLevel = OtpTrace.Type.sendThreshold;

            if (args.Length < 1)
            {
                System.Console.Out.WriteLine(
                    "Usage: {0} remotenode [cookie] [-notrace]\n" +
                    "    nodename  - is the name of the remote Erlang node\n" +
                    "    cookie    - is the optional cookie string to use\n" +
                    "    -name     - this node name\n" +
                    "    -wire     - wire-level tracing\n" +
                    "    -notrace  - disable debug trace\n",
                    Environment.GetCommandLineArgs()[0]);
                return;
            }
            else if (args.Length > 1 && args[1][0] != '-')
            {
                cookie = args[1].ToString();
            }

            for (int i = 0; i < args.Length; i++) {
                if (args[i].Equals("-wire"))
                    AbstractConnection.traceLevel = OtpTrace.Type.wireThreshold;
                else if (args[i].Equals("-notrace"))
                    AbstractConnection.traceLevel = OtpTrace.Type.defaultLevel;
                else if (args[i].Equals("-name") && i+1 < args.Length) {
                    nodename = args[i++ + 1];
                    if (nodename.IndexOf('@') < 0)
                        nodename += '@' + host;
                }
            }

            OtpNode node = new OtpNode(false, nodename, cookie, true);

            System.Console.Out.WriteLine("This node is called {0} and is using cookie='{1}'.",
                node.node(), node.cookie());

            OtpCookedConnection.ConnectTimeout = 2000;
            OtpCookedConnection conn = node.connection(remote);
            conn.OnReadWrite += OnReadWrite;

            if (conn == null)
            {
                Console.WriteLine("Can't connect to node " + remote);
                return;
            }

            // If using short names or IP address as the host part of the node name,
            // get the short name of the peer.
            remote = conn.peer.node();

            System.Console.Out.WriteLine("   successfully connected to node " + remote + "\n");

            OtpMbox mbox = null;

            try
            {
                mbox = node.createMbox();

                {
                    Otp.Erlang.Object reply = mbox.rpcCall(
                        remote, "lists", "reverse", new Otp.Erlang.List("Abcdef!"));
                    System.Console.Out.WriteLine("<= [REPLY1]:" + (reply == null ? "null" : reply.ToString()));
                }

                {
                    Otp.Erlang.Object reply = mbox.rpcCall(
                        remote, "global", "register_name",
                        new Otp.Erlang.List(new Otp.Erlang.Atom("me"), mbox.self()));

                    System.Console.Out.WriteLine("<= [REPLY2]:" + (reply == null ? "null" : reply.ToString()));
                }

                {
                    Otp.Erlang.Object reply = mbox.rpcCall(remote, "global", "register_name", new Otp.Erlang.List(new Otp.Erlang.Atom("me"), mbox.self()), 5000);
                    System.Console.Out.WriteLine("<= [REPLY3]:" + (reply == null ? "null" : reply.ToString()));
                }

                {
                    Otp.Erlang.Object reply = mbox.rpcCall(
                        remote, "io", "format",
                        new Otp.Erlang.List(
                            "Test: ~w -> ~w\n",
                            new Otp.Erlang.List(mbox.self(), new Otp.Erlang.Atom("ok"))
                        ));

                    System.Console.Out.WriteLine("<= [REPLY4]:" + (reply == null ? "null" : reply.ToString()));
                }

                while (true)
                {
                    Otp.Erlang.Object msg = mbox.receive();
                    if (msg is Otp.Erlang.Tuple)
                    {
                        Otp.Erlang.Tuple m = msg as Otp.Erlang.Tuple;
                        if (m.arity() == 2 && m.elementAt(0) is Otp.Erlang.Pid)
                        {
                            mbox.send(m.elementAt(0) as Otp.Erlang.Pid, m.elementAt(1));
                        }
                    }
                    System.Console.Out.WriteLine("IN msg: " + msg.ToString() + "\n");
                }

            }
            catch (System.Exception e)
            {
                System.Console.Out.WriteLine("Error: " + e.ToString());
            }
            finally
            {
                node.closeMbox(mbox);
            }

            node.close();
        }
	}
}
