# monkey-droid <img src="https://github.com/MV10/volts-laboratory/blob/master/misc/mhh-icon.png" height="32px"/>

A simple remote-control UI for the [monkey-hi-hat](https://github.com/MV10/monkey-hi-hat) audio visualization app.

> This repository is a complete from-scratch rewrite of Monkey Droid, which will become version 2.0.

When version 2.0 is released, you will download and install from the Monkey Hi Hat [release](https://github.com/MV10/monkey-hi-hat/releases) page. The currently-available version 1.0.1 is compatible with current releases of Monkey Hi Hat, it just lacks support for many of the newer features. Many painful lessons were learned fighting with the buggy .NET 7 version of MAUI (not to mention the poor, incorrect, outdated, or missing documentation), so the hope is that a clean-slate rewrite will produce a more useful base for future revisions.

Note that this communicates with monkey-hi-hat over the local network, so you must set the `UnsecuredPort` option in the Monkey Hi Hat `mhh.conf` configuration file to tell it to listen for commands via TCP.

Despite the name (we run it on our Android phones), the program is cross-platform -- it should work on Windows 10 1809 or newer, Windows 11, and Android (supposedly v5 or newer, though I can only test against v13). Although .NET MAUI supports Apple devices, this application does not (and will not).

The rewrite is targeting the .NET 8.0 Long Term Service release, which should have three years of support, but .NET MAUI support has a different [lifecycle](https://dotnet.microsoft.com/en-us/platform/support/policy/maui) policy than .NET itself, thanks to dependencies on Android and Apple libraries.

