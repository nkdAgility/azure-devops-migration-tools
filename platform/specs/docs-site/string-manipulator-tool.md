---
applyTo: "docs/content/docs/reference/tools/stringmanipulatortool/index.md"
---

# String Manipulator Tool page

What the reference page at `docs/content/docs/reference/tools/stringmanipulatortool/index.md` must say, so it matches
`platform/specs/core/string-manipulator.md`.

- The built-in pattern removes only the characters the SOAP API cannot save: control characters other than tab, line
  feed and carriage return; unpaired surrogates; and U+FFFE and U+FFFF. The page shows that pattern in full, so readers
  can copy it into their own `Manipulators`.
- Everything else is kept, and the page says so with examples: accented letters, letters outside the Basic
  Multilingual Plane (such as Osage and CJK Extension B), emoji, and DEL and the C1 controls.
- Setting your own `Manipulators` replaces the built-in pattern.
- Removing emoji is opt-in, with its own pattern, and the page says that pattern also removes letters outside the
  Basic Multilingual Plane.
- The ASCII-only pattern is shown only as a legacy option that removes every non-ASCII letter.
- The generated data file, `docs/data/classes/reference.tools.stringmanipulatortool.yaml`, comes from
  `GenerateDocs.ps1` and is never edited by hand.
