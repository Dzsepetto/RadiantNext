# RadiantNext

RadiantNext is an experimental C# map editor core for classic `.map` / BSP-based games, with a focus on Call of Duty 1 and Call of Duty 2 style workflows.

The long-term goal is to build a modern Radiant-like editor with stable `.map` parsing, brush editing, validation, and export support.

## Current Status

- Token-based `.map` parser
- `.map` exporter
- Entity, brush, and face model structure
- Convex brush validation
- Roundtrip parser/export tests
- Basic brush geometry building
- Unit tests for parser, exporter, tokenizer, validation, and roundtrip behavior

## Project Goals

- Reliable CoD1/CoD2 `.map` compatibility
- Lossless parse/export roundtrip where possible
- Stable convex brush editing
- Radiant-like workflow
- BSP-compatible map output
- Future visual editor support

## Tech Stack

- C#
- .NET 8
- xUnit

## Repository Structure

```txt
MapMaker/
├── MapMaker.Core       # Core parser, exporter, geometry, validation
├── MapMaker.Editor     # UI
├── MapMaker.Tests      # Unit and roundtrip tests
