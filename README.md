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
of Erlang interface for .NET called NFX.Erlang.  NFX is a library that will be
open sourced in Jan 2014. Pre-release documentation is available
[here](http://itadapter.com/nfxhelp/) (under NFX.Erlang section).
