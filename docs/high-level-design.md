# monkey-droid High Level Design

The monkey-droid application is a remote control GUI for sending commands (via TCP on the local network) to the Monkey Hi Hat music visualizer running on another computer, and 
for receiving data in response to those commands. This is a cross-platform .NET 10 app using Avalonia supporting Windows, Linux, and Android. The basic design is to select a server (the Windows or Linux PC running Monkey Hi Hat), then swipe back and forth to different views, using buttons and lists to send commands.

The Monkey Hi Hat documentation provides a comprehensive list of human-available commands [here](https://www.monkeyhihat.com/docs/index.php#/commands-and-keys), and there are a handful of not-publicly-documented commands specific to monkey-droid's unique UI requirements (such as collecting a list of visualizations).


## General Guidelines

### UI Specifications

* The UI has a portrait-mode layout on all platforms. The window is resizable on desktop targets.
* A three-bar "hamburger" menu is always available at the top left.
* Most views are list-oriented with a few buttons at the bottom, or consist of grids of buttons.
* Custom fonts (TTF) are packaged and deployed with the app to provide icons and a consistent look.
* There are several simple multiple-choice popup menus (lists) required.
* Three types of pop-up (overlay) messages are required:
  * Simple message dismissed by Ok button.
  * Prompt message presenting Ok and Cancel buttons.
  * Short list of menu options; tapping or clicking outside the menu will dismiss it.
* The Windows executable, Android executable, and Android APK must have a suitable icon (/docs/icon.png).

### Technology and Dependencies

* Data about the Monkey Hi Hat servers is stored as JSON in a plain text file.
* [CommandLineSwitchPipe](https://github.com/MV10/CommandLineSwitchPipe) is used to send and receive data.
* The UI framework is Avalonia 11 using the CommunityToolkit MVVM implementation.
* The default Avalonia ViewLocator is not used, all views must be explicitly registered. This improves AOT and code trimming.
* Avalonia compiled bindings and central package management are both in use.


## Hamburger Menu

The Hamburger Menu is available on all swipe-sequence views.

### Hamburger menu entries

* Help
* Docs
* Get support
* About
* (separator)
* Clear auto-select server
* (separator)
* Wipe all saved data

### Hamburger menu behaviors

* Docs: Opens the default browser to https://www.monkeyhihat.com/docs/index.php#/using-monkey-droid
* Get support: Opens the default browser to https://www.monkeyhihat.com/docs/index.php#/troubleshooting
* About: Loads the About Screen view
* Clear auto-select server: Resets the auto-selected server name to blank (stored in the server data file)
* Wipe all saved data: Prompts for confirmation, then blanks (does not delete) the server data file, then resets application state as if restarted


## Views: General Information

* When a server has been selected, the user can change views. Some views are not in this sequence.
* If no server is selected, the user can't navigate away from the Server List view and the nav buttons are hidden.
* User changes views by clicking left-arrow or right-arrow buttons in the toolbar, or on Android, by swiping left or right.
* A Splash Screen view is shown while the app is loading (and for a minimum of 5 seconds).
* The Help Screen and About Screen views are dismissed by tapping or clicking on an OK button.
* At startup if no server file exists, go to the Server Editor view in add mode. 
* After saving the first and only server, this will become the auto-selected server, select it and go to the Playlist view.
* At startup if the server file exists and an auto-selected server name exists, select it and go to the Playlist view.
* At startup if the server file exists but no auto-selected server name exists, go to the Server List view.

### Views in swipe sequence
 
* Server List
* Playlists
* Common Controls
* Visualizers
* FX
* Console

### Views not in swipe sequence

* Splash Screen
* About Screen
* Help Screen
* Server Editor


## Stored Data

* Data is serialized as JSON in a plain text file.
* An auto-selected server name is stored (empty string if none).
* Each server is identified by a unique name corresponding to that device's network name.
* Server entries are stored in alphabetical order by server name.
* Each server has a name, a main TCP port number, and an alternate port number.
* The server name and main port are required, the alternate port is optional.
* Server names can be up to 63 characters long, but a warning should be shown above 15 characters that it may not be compatible with all networks.
* The port range is 1 to 65535. If a user chooses a port number below 49152, show a warning that this may interfere with reserved ports.
* The default port numbers when adding a new server are 50001 and 50002.
* Each server has three arrays of data: visualizations, fx, and playlists.
* Visualization and FX data have three elements: name (string), description (string), and audio (0/1 boolean).
* Playlist data only has a name (string) element.
* The data in the arrays should be stored in sorted order by name.
* Store timestamps indicating when each array was stored.


## Views: Screen Content 

* Many of the buttons trigger a communications event.
* Communications events will be described separately from the content of the views.

### Splash Screen view

* The Splash is shown during loading (and for a minimum of 3 seconds).
* There is no titlebar.
* It is blank with a circular-masked version of the `docs/icon.png` image.
* It cannot be dismissed by the user.

### About Screen view

* The About Screen is shown when loaded from the Hamburger Menu.
* The titlebar is labeled "About".
* It is dismissed by tapping it (user returns to previous view).
* It has a smaller circular-masked version of the `docs/icon.png` image like Splash Screen uses.
* Below the icon are three lines: the app name (monkey-droid), the app version, and `https://www.monkeyhihat.com/`

### Server Editor view

* The Server Editor view can be used in add mode or edit mode.
* The titlebar is labeled "Add Server" or "Edit Server" according to the mode.
* The Server Editor is shown when:
  * The program is launched and there are no known servers.
  * The user requests to add a new server.
  * The user requests to edit an existing server.
* It contains three labeled input fields:
  * Server name:
  * TCP port:
  * Alternate TCP port:
  * Data validation is described in the Stored Data section.
* The bottom of the screen has Save and Cancel buttons.
* Cancel is not available if there are no known servers.
* Save is not available until at least a name and valid port is entered.
* Upon exiting, the user is sent to the Server List view.
* If the saved entry is new (not edited) and is the only server in the list:
  * Also save the server name as auto-selected.
  * Show an overlay message, "This server will be auto-selected at startup."

### Server List view

* List of known (saved) servers sorted alphabetically by name.
* List items are two-row showing the server name, then below, in smaller text, the TCP port and, if available, the alternate port.
* If the server is the auto-selected one, "(auto-selected)" will be shown below the server name after the TCP ports. 
* Each list item has a "selected" indicator (Unicode U+25CF, de-selected is U+25CB) and only one server can be selected at any time.
* User cannot swipe or advance to another view if no server is selected.
* When the user taps a server entry in the list a simple menu opens with these options:
  * Select: marks the server as selected and navigates the user to the Playlist view.
  * Test: executes the CommandLineSwitchPipe's `TryConnect` and displays a "Success" or "Failed" overlay message.
  * Edit: opens the server data in the Server Editor view in edit mode.
  * Auto-select at startup / Disable auto-select: saves the server name for startup auto-select, or clears the auto-select name
  * Delete: shows an Ok / Cancel overlay message warning that the server will be permanently deleted.
  * Cancel: dismisses the menu.
* If the only server entry is deleted, the user will be forced to view the Server Editor in add mode again.
* There is an Add button at the bottom of the screen below the list, which opens the Server Editor view in add mode.

### Visualizers view

* The view is accessed by swiping or using the nav buttons.
* The titlebar is labeled with the view name.
* Below the titlebar in small text is the timestamp for the data list, if any.
* The main content is a list of the known visualizers loaded from the selected server.
* List items are two-row showing an icon and the visualizer name, then below, in smaller text, the visualizer description.
* The icon indicates whether the visualizer is music-reactive. Music reactive uses Unicode U+266A and non-reactive is U+3030.
* Selecting any list item sends a two-string command, `--load` and the selected visualizer name.
* Below the list at the bottom of the screen are two buttons:
  * A load-list icon button (specific icon TBD) which is narrow (20% of the width)
  * A "Reload" button (80% of the width)
* The load-list button:
  * Clears the timestamp 
  * Clears any existing visualizer list
  * Begins a background communications loop reading data from the server (detailed later)
* The "Reload" button sends a `--reload` command to the server. Read but discard the response.

### FX view

* The view is accessed by swiping or using the nav buttons.
* The titlebar is labeled with the view name.
* Below the titlebar in small text is the timestamp for the data list, if any.
* The main content is a list of the known effects (FX) loaded from the selected server.
* List items are two-row showing an icon and the FX name, then below, in smaller text, the FX description.
* The icon indicates whether the FX is music-reactive. Music reactive uses Unicode U+266A and non-reactive is U+3030.
* Selecting any list item sends a two-string command, `--fx` and the selected FX name.
* Below the list at the bottom of the screen is one button:
  * A load-list icon button (specific icon TBD) which is narrow (20% of the width)
* The load-list button:
  * Clears the timestamp
  * Clears any existing FX list
  * Begins a background communications loop reading data from the server (detailed later)

### Playlists view

* The view is accessed by swiping or using the nav buttons.
* The titlebar is labeled with the view name.
* Below the titlebar in small text is the timestamp for the data list, if any.
* The main content is a list of the known playlists loaded from the selected server.
* List items are single row showing the playlist name only.
* Selecting any list item sends a two-string command, `--playlist` and the selected playlist name.
* Below the list at the bottom of the screen are three buttons:
  * A load-list icon button (specific icon TBD) which is narrow (20% of the width)
  * An "Add FX" button (40% of the width)
  * A "Next viz" button (40% of the width)
* The load-list button:
  * Clears the timestamp
  * Clears any existing playlist list
  * Reads playlist filenames from the server (detailed later)
* The "Add FX" button sends `--next` and `fx` arguments to the server. Read but discard the response.
* The "Next viz" button sends a `--next` argument to the server. Read but discard the response.

### Common Controls view

* The view is accessed by swiping or using the nav buttons.
* The titlebar is labeled with the view name.
* The page is a two-column list of buttons.
* Left column buttons:
  * What?: sends two args, `--show` and `what`; reads but discards the response.
  * Standby: sends a `--standby` arg to the server; reads but discards the response.
  * Fullscreen: sends a `--fullscreen` arg to the server; reads but discards the response.
  * FPS: sends an `--fps` arg to the server; reads and displays the response in an overlay.
* Right column buttons:
  * Track: sends two args, `--show` and `track`; reads but discards the response.
  * Quit: sends a `--quit` arg to the server; reads but discards the response.
  * Idle: sends an `--idle` arg to the server; reads but discards the response.
  * Info: sends an `--info` arg to the server; reads and displays the response in an overlay.

### Console view

* The view is accessed by swiping or using the nav buttons.
* The titlebar is labeled with the view name.
* Below the titlebar show "Enter --help, dashes are optional"
* The content area is like a terminal, there is a scroll buffer, and the user enters commands into an input field at the bottom.
* Provide a small button labeled "Send" to the right of the data entry area below the scroll buffer.
* Split the user's input on spaces to create the string array of arguments to transmit to the server.
* If the user enters a command (the first input word) without a `--` prefix, add it before sending to the server.
* The user's command line should be scrolled up into the display area after sending.
* Any response from the server is output and scrolled up the display area.
* The scroll-back buffer should be 999 lines of text. It is not saved between sessions.
* Up to 20 of the user's command history are stored. It is not saved between sessions.
* The up/down arrows in the input field scroll through the command history.

## Communications

### The CommandLineSwitchPipe library

* The communication functions exposed by CommandLineSwitchPipe are static in nature.
* They all accept a server name and port number.
* If the server's primary port fails and an alternate port is available, try the alternate port.
* The `TryConnect` and `TrySendArgs` methods return a boolean indicating success or failure.
* `TrySendArgs` also requires a string array of command line switches (aka arguments) to transmit to the server.
* The `QueryResponse` method returns a string and is used after `TrySendArgs` succeeds.
* Monkey Hi Hat may respond with a string beginning with "ERR" to indicate an error. 
* Failure is silent except in response to the Test command from the Server List view, or in response to error messages (exception: Console view, just output to history).

### Monkey Hi Hat arguments

* The Monkey Hi Hat server program accepts command-line arguments prefixed by `--` then additional arguments as individual values.
* When sending these via CommandLineSwitchPipe, they are split at spaces into a string array for transmission.
* User input examples:
  * `--load foobar` (two-string array)
  * `--info` (one-string array)
  * `--help` (one-string array)
  * `next fx` (two-string array, program sends `--next` instead of `next`)
* The full list of possible commands is documented here: https://raw.githubusercontent.com/MV10/monkeyhihat.com/refs/heads/master/docs/commands-and-keys.md

### File list data retrieval from server

* The command to read filenames for each list type is two arguments, `--md.list` and the type:
  * Visualizers: `viz`
  * FX: `fx`
  * Playlists: `playlists`
* The response to the list request is a string separated by CommandLineSwitchPipe's `SeparatorControlCode`
* Playlists only have filenames, no detail data.
* The load-list buttons on the visualizer and FX views perform a series of operations:
  * The filename list is retrieved with a single two-argument command.
  * The UI is not updated with the filenames at that point.
  * Every 500ms a detail request is sent for each filename.
  * The UI is updated as the details are retrieved for each filename.
  * The server data is written only after all details are retrieved.
* Playlists do not have detail data to read.
* During background processing of visualizer details, send two arguments, `--md.detail` and the visualizer filename
* During background processing of fx details, send two arguments, `--md.detailfx` and the fx filename
* The response is a single string:
  * The first character is `0` or `1` where 1 indicates the visualizer or FX is audio-reactive, and 0 is not.
  * The remainder of the string is the description of the file.
* If an error is signaled (response begins with "ERR") during background processing, abort the download loop. 
