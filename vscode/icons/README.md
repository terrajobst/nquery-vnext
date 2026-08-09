# Icons

`nquery-light.svg` and `nquery-dark.svg` are the file icon for `.nql` and `.nqe`, declared as
`contributes.languages[].icon` in `package.json`.

They are the **`database` glyph from [microsoft/vscode-codicons]**, used under
[CC BY 4.0]. The only change is the fill: the original is `currentColor`, which resolves to black
when an SVG is loaded as an image rather than inlined, so each copy carries an explicit color.
Pull a newer glyph from that repository rather than editing the path data by hand.

The colors are **Seti's**, copied from `vs-seti-icon-theme.json` — `_db_light` `#dd4b78` for light
themes and `_db` `#f55385` for dark, the colors Seti draws SQL in. Seti is the theme that will
actually render this icon (see below), so matching its palette makes the file read as a recognized
type in Seti's own color language rather than as an import from somewhere else. The cost of that
choice is a dependency on someone else's palette: if Seti recolors `_db`, these drift out of step
silently, since nothing here reads that file at build time.

Two files because `icon` takes a `light`/`dark` pair, and the key names the **color theme** the
icon is used with — `light` therefore holds the *dark* glyph.

This is a fallback, not the icon most people will see. An icon theme that knows the extension
outranks it; see the "File icons" section of the extension README for why.

[microsoft/vscode-codicons]: https://github.com/microsoft/vscode-codicons
[CC BY 4.0]: https://creativecommons.org/licenses/by/4.0/
