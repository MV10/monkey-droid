# monkey-droid <img src="https://github.com/MV10/monkey-droid/blob/master/docs/icon.png" height="32px"/>

This is a simple remote-control UI for the [Monkey Hi Hat](https://github.com/MV10/monkey-hi-hat) music visualization app.

Download it from the Monkey Hi Hat [releases](https://github.com/MV10/monkey-hi-hat/releases) page.

This is version 2.0, a complete from-scratch rewrite. It supports Android, Windows, and Linux. I do not support or use iOS / OSX. It's probably possible to build a version for that but I don't have an Apple machine to do the build, it's unlikely to happen. (If you're a .NET dev and want to tackle it _and_ own that build, I'll consider it. Open an Issue to discuss.)

Note that this communicates with Monkey Hi Hat over the local network, so you must set the `UnsecuredPort` option in the Monkey Hi Hat `mhh.conf` configuration file to tell it to listen for commands via TCP (this is enabled and configured by default).

Use the Monkey Hi Hat [Issues](https://github.com/MV10/monkey-hi-hat/issues?q=sort%3Aupdated-desc+is%3Aissue+is%3Aopen) page for any problems or questions, I prefer to keep everything centralized over there.