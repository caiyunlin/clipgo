# ClipGo

**ClipGo** is a lightweight clipboard monitoring tool that detects specific patterns such as ticket IDs or incident codes and provides instant actions via a pop-up window. These actions include quick access to URLs or launching local executables with parameters.

## Features

- 📋 Real-time clipboard monitoring
- 🔍 Pattern detection via regular expressions
- 🚀 Quick-action dialog with clickable links
- 🌐 Supports URL redirection or local executable launches
- ⚙️ Easily extendable pattern-action definitions

## Example Use Case

1. You copy a ticket ID such as `BOOKKEEPER-115`
2. ClipGo detects it matches a predefined pattern
3. A dialog pops up with a shortcut link:  `https://issues.apache.org/jira/browse/BOOKKEEPER-115`
4. Clicking the link opens the corresponding case page in your browser

## Installation

> ClipGo is developed with **.NET Framework 4.7.2**, ensure it is installed on your Windows system.

1. Clone the repository:
   ```bash
   git clone https://github.com/caiyunlin/clipgo.git
   ```
2. Open ClipGo.sln using Visual Studio (2019 or later)

3. Build the solution

4. Run the executable located in the bin\Debug or bin\Release folder

## Configuration
You can configure patterns and actions in the data\config.xml file.

Each configuration includes:
- A Regex pattern to match clipboard content
- Mutiple URLs template or local executable path with arguments

Example : This matches a 16-digit number and replaces {0} with the matched value.
```xml
    <match type="text" params="^\d{16}$">
        <actionForm>
            <links>
                <link title="Search {0} in Case System" action="https://www.ticket-system.com/ticketId={0}" />
                <link title="Open {0} in BAT" action="data\actions\ticket.bat" />
            </links>
        </actionForm>
    </match>
```

## Roadmap

- [x] Add tray icon with context menu
- [ ] GUI for adding/removing patterns
- [ ] Auto-start on system boot

## Screenshots

> Coming soon!