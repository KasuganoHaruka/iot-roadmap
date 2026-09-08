# Six Months into Industrial IoT

**Languages:** [English](README.md) · [中文](README.zh.md)

A personal, 24-week plan to move from ten years of general C# work into an
industrial-IoT role — together with the code scaffold the plan is built around.

This is study material, not a library. Nothing here is meant to be depended on.

## The code is unfinished on purpose

Fourteen methods in `src/` throw `NotImplementedException`. They are not defects
and they are not TODOs someone forgot: **they are the exercises.** The tests are
written first, and implementing each method in the order the roadmap gives is
the syllabus.

```
$ cd src && dotnet test
Failed!  - Failed:    19, Passed:     5, Skipped:     0, Total:    24
```

A failing suite is the expected state of this repository. It is finished when
that line reads 24 passed.

| Week | What you implement | Where |
|---|---|---|
| **W2** | CRC16, bitwise and then table-driven | `Iiot.Drivers.Modbus/Crc16.cs` |
| **W3** | Address parsing, value coding (word order), batch-read planning, PDU building, response parsing, writes | `Iiot.Drivers.Modbus/` |
| **W7** | Store-and-forward buffer on SQLite — append, take pending, mark sent, prune, count | `Iiot.EdgeGateway/Buffering/` |

## Layout

```
roadmap.{en,zh}.md     The plan: six milestones, M1-M6, over 24 weeks
weekly-checklist.md    Week-by-week tasks, with a column for hours actually spent
hardware.md            What to buy (¥1500-3000) and what to avoid on the used market
interview-qa.md        Question bank with answer outlines — to be spoken aloud, not read
progress.md            The 30-minute self-check at the end of each month
src/                   The solution the exercises live in
```

`roadmap.ja.md` also exists. It is **no longer updated** and English wins where
the two disagree.

## The solution

Three projects and one test project, all `net10.0`:

- **`Iiot.Abstractions`** — the driver contract (`IDeviceDriver`, `TagDefinition`,
  `TagValue`, `Quality`). Everything else depends on this and nothing else.
- **`Iiot.Drivers.Modbus`** — a Modbus library written from scratch. Deliberately
  *not* NModbus: writing it is the point. `IModbusTransport` isolates the only
  thing RTU and TCP actually disagree about — framing and checksums — so the
  driver above it is written once; the concrete transports are still to be
  written. Word order for 32-bit values is configurable (ABCD / CDAB / BADC /
  DCBA), and the read planner merges scattered addresses into the fewest
  requests.
- **`Iiot.EdgeGateway`** — a Worker Service that polls devices, buffers readings
  to SQLite when the network is down, and uploads over MQTT. Publishes as a
  single file for `linux-arm64` and runs under systemd on a Raspberry Pi;
  `deploy/` carries the unit file and the udev rules that pin USB serial
  adapters by serial number.

## Building

Requires the **.NET 10 SDK**. `src/global.json` pins it to 10.0.400 with
`rollForward: latestFeature`, so a newer major SDK on the machine will not
silently take over the build.

```sh
cd src
dotnet build
dotnet test
```

Targeting .NET 10 rather than 8 is not about performance: **.NET 8 leaves
support on 2026-11-10**, and starting a six-month project on a framework that
goes EOL two months in is a needless thing to explain in an interview.

## Status

Started 2026-09. Target: employed in the field by 2027-03.

All six months are still marked not-started in [`progress.md`](progress.md).
That file is the gate: each month ends with a 30-minute self-check, and the bar
is being able to explain the material at a whiteboard, not having read it.
