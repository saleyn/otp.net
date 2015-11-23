# OTP.NET #

This project implements Erlang Distribution Protocol in .NET and also provides class
abstractions of all Erlang primitive types with ability to match terms.

This is a fork of OTP.NET found in: http://jungerl.sourceforge.net
This folk has many bug fixes, and new features, such as Erlang term construction from
strings `Erlang.Object.Format()`, pattern matching `Erlang.Object.match()`, and more
(see `release_notes.txt` for changes).

For sample use of OTP.NET see `OtpTest1/Test1.cs`.

## NOTE ##

This project is being phased out into an updated and more powerful implementation
of Erlang interface for .NET called NFX.Erlang. The project is located at
https://github.com/aumcode/nfx. Pre-release documentation is available
[here](http://itadapter.com/nfxhelp/) (under NFX.Erlang section).

See the features of the new library described here:
* [Overview blog](http://blog.aumcode.com/2013/10/nfx-native-interoperability-of-net-with.html)
* [Interop with Erlang/Mnedia and RPC abstraction](https://www.youtube.com/watch?v=o9utCAMLydA)

## What is implemented in NFX.Erlang? ##
* Support of all Erlang types and their mapping to corresponding CLR types
* Pattern matching of Erlang types
* String parsing (e.g. `"{10, 'abc', A::int(), [B::atom(), C::list()], \"hello\"}."`) into the
  corresponing Erlang type
* Erlang term serialization/deserialization
* Full support of OTP distributed protocol
* Initiation of Erlang.NET node starting via configurable dependency injection or through code
* Distributed mailbox monitoring and linking
* RPC calls from Erlang to .NET and from .NET to Erlang
* I/O server in .NET so that output of an RPC call from .NET to `io:put_chars("hello\n")` can
  be relayed back to .NET and handled there
 
## What is not implemented yet in NFX.Erlang? ##
* Support for Erlang maps introduced in R18.
