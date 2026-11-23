# StringManipulatorTool

`StringManipulatorTool` (`src/MigrationTools/Tools/StringManipulatorTool.cs`) cleans the value of every String field
before a work item revision is saved to the target, and cuts it to `MaxStringLength`.

## Manipulators

- The configured `Manipulators` run in order. Each is a regex replacement and can be disabled on its own.
- With `Manipulators` empty, the tool adds its built-in default, which removes only what the SOAP (TFS object model) API
  cannot save: control characters other than tab, line feed and carriage return; unpaired surrogates; and U+FFFE and
  U+FFFF. Every other character is kept, including letters outside the Basic Multilingual Plane, emoji, DEL and the C1
  controls.
- A configured manipulator replaces the default; the default is not added as well.
- The shipped `configuration-default.json` and `configuration-classic.json` enable the tool with `Manipulators` empty,
  so they use the default.

## Length

A value longer than `MaxStringLength` is cut to it, and the cut is written to the work item review log.
