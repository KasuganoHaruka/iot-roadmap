# Six-Month Roadmap into Industrial IoT · Chengdu, ¥15–20k/month

> Start: 2026-09 · Target: 2027-03
> Background: 10 years of C# development; serial / TCP / Modbus data acquisition several years ago (now rusty)
> Pace: employed full-time, 10–15 hours per week (~280–360 hours over six months)

---

## 0. Strategic Call

### Why Industrial IoT and not something else

| Direction | Reuse of existing experience | Feasibility of ¥15–20k in six months |
|---|---|---|
| **Industrial IoT / SCADA & HMI software** | **80%+** (C#, serial, Modbus, TCP all directly usable) | **High** |
| IoT cloud backend (Java/Go) | 30% (only general engineering skills carry over) | Low — you'd have to learn a new language ecosystem *and* distributed systems, then compete with CS-trained Java developers |
| Embedded firmware (C/RTOS) | 10% | Very low — effectively starting from zero |
| Consumer smart hardware | 40% | Low-to-medium — jobs concentrate in Shenzhen, few in Chengdu |

Six months is only enough to go from "can use it" to "can own it" in **one** direction. So pick the one with the highest reuse.

### Honest salary calibration for Chengdu

- **¥12–15k**: can write HMI software, knows Modbus, can contribute to a project. With your background, one month of refreshing gets you here.
- **¥15–20k (the target)**: can independently own a data-acquisition system from selection through deployment + understands PLCs and OPC UA + can travel to customer sites for commissioning + can articulate architectural trade-offs.
- **¥20k+**: add domain depth (semiconductor / new energy / defense) or team leadership. Not a six-month goal.

⚠️ **Ten years of experience cuts both ways.** Interviewers will judge you as a technical lead. Implementation details alone won't pass — you must be able to discuss architecture, the pitfalls you've hit, and why you chose A over B. This matters far more than learning one more framework.

**Fallback**: if six months falls short, take a ¥12–15k role, accumulate one year of real project work, then move. Total elapsed time is still far better than switching to Java or embedded.

---

## 1. Scope of the Stack

### Must learn (core differentiators)

**Protocol layer** — this is what separates you from "a C# developer who writes CRUD"
- Modbus RTU / TCP — at the byte level, not "can call a library"
- Siemens S7comm — DB block read/write, optimized vs. non-optimized blocks
- **OPC UA** — address space, subscriptions, certificate security. The de facto standard for industrial interoperability
- MQTT 3.1.1 / 5 — QoS semantics, topic design, shared subscriptions

**Edge layer**
- Linux: systemd services, udev rules to pin serial device names, `ip` / `ss` / `tcpdump` troubleshooting
- Docker / docker compose
- .NET 10: Worker Service, `linux-arm64` publishing, `System.Threading.Channels`, Polly, Serilog
- SQLite local buffering → store-and-forward across network outages

**Platform layer**
- TDengine (the default time-series database in Chinese industry) / InfluxDB
- Grafana dashboards and alerting
- EMQX (MQTT broker)
- ThingsBoard (to internalize the thing-model / rule-chain / device-shadow abstractions)
- Alibaba Cloud IoT **or** Huawei Cloud IoTDA — pick **one**, get it working end to end

**HMI / application layer**
- WPF + CommunityToolkit.Mvvm (mainstream for industrial HMI)
- ScottPlot / LiveCharts2 real-time charts (high-volume rendering)
- SignalR for real-time push
- Blazor Server for web dashboards (optional)

**Domain knowledge** — both integrators and end customers will ask
- ISA-95 levels: L0 field devices → L1 control → L2 supervision (SCADA) → L3 manufacturing operations (MES) → L4 enterprise (ERP)
- What PLC / HMI / SCADA / DCS / MES each actually own, and where the boundaries are
- OEE, utilization, takt time, work orders, traceability
- IEC 62443 zones and conduits (conceptual fluency is enough)

### Explicitly out of scope (negative ROI within six months)

C firmware, RTOS, PCB design, LoRa/NB-IoT physical layer, deep Kubernetes, the Java/Go ecosystem, Kafka internals.

> You only need to **explain where these fit and when they apply** in an interview — no hands-on work required.
> Example: "NB-IoT suits low-frequency reporting on battery power over wide areas — water and gas meters. Inside a plant, wired plus WiFi is sufficient; there's no reason to bring a carrier network into it."

---

## 2. The Six-Month Plan

### M1 · Weeks 1–4: Protocol refresher, Modbus mastered

| Week | Content |
|---|---|
| **W1** | Assemble the hardware. RS-232 vs. RS-485 electrical differences, termination resistors (120Ω, only at both ends), single-point shield grounding, common-ground issues. C# `SerialPort` pitfalls: `ReadTimeout`, fragmented `DataReceived` events, `BytesToRead` and buffering. TCP partial reads and message framing. |
| **W2** | Work through the Modbus spec byte by byte: function codes 01/02/03/04/05/06/0F/10, the CRC16 algorithm (table-driven), exception responses (function code + 0x80). Compare a Modbus slave simulator against Wireshark captures, byte by byte. |
| **W3** | **Implement your own Modbus library** (skip NModbus for now): RTU and TCP modes, timeout and retry, batching that **merges scattered addresses into the fewest possible requests**, and configurable 32-bit float word order (ABCD / CDAB / BADC / DCBA). |
| **W4** | A multi-device polling scheduler: priority queue, slow-device isolation, exponential backoff on reconnect, link-quality statistics (success rate / mean latency / timeout count). Put 2–3 real slaves on an actual RS-485 bus. |

**Deliverables**
- `Iiot.Drivers.Modbus` library with unit tests (CRC, word order, frame encode/decode)
- A field-troubleshooting handbook for Modbus, written by you — this becomes interview material

**Gate check**: with the spec closed, write out the request and response byte layout for function code 03 on paper, and explain why the CRC is transmitted low byte first.

---

### M2 · Weeks 5–8: MQTT and a Linux edge gateway

| Week | Content |
|---|---|
| **W5** | Install Ubuntu Server on the Raspberry Pi. Publish .NET 10 as a single-file `linux-arm64` binary running as a systemd service (start on boot, restart on crash). udev rules pinning USB serial adapters by serial number (`/dev/ttyModbus0`). Troubleshoot with `ip a`, `ss -tunlp`, `tcpdump -i eth0 port 502`. |
| **W6** | MQTT fundamentals: QoS 0/1/2 semantics and their costs, retained messages, LWT (last will for offline detection), clean session / session expiry, shared subscriptions (`$share/group/topic`). Deploy EMQX via Docker. MQTTnet client. **Topic design conventions** (`iiot/{site}/{line}/{device}/telemetry`). |
| **W7** | Turn the M1 drivers into an edge service: acquire → buffer in SQLite → publish over MQTT → **automatic store-and-forward after an outage** → purge on ACK. Backpressure and a memory ceiling (bounded `Channel` plus a drop policy). |
| **W8** | Harden it: Polly retry and circuit breaking, a health endpoint, graceful shutdown (`IHostApplicationLifetime`), hot configuration reload (`IOptionsMonitor`), rolling structured logs with Serilog, remote configuration pushed over MQTT. |

**Deliverables**
- `Iiot.EdgeGateway` running 24/7 on the Raspberry Pi
- **A screen recording of the outage test — unplug the cable, then watch the backlog drain.** Playing this in an interview beats ten sentences of explanation.

**Gate check**: explain that QoS 1 can deliver duplicates, and how the application handles idempotency (message ID + a monotonic sequence number from the device + a deduplication window on the platform).

---

### M3 · Weeks 9–12: PLC and OPC UA (**the decisive month**)

| Week | Content |
|---|---|
| **W9** | PLC basics: wiring (24V, common terminals for I/O), installing the programming software, reading ladder logic, DB blocks, data types and byte order (**PLCs are big-endian**). Write your own start/stop + counter + timer logic. |
| **W10** | Read and write DB blocks with S7.Net Plus / Sharp7. **Optimized vs. non-optimized blocks** (optimized blocks have no absolute addresses — disable optimization or use symbolic access). Absolute address arithmetic (`DB1.DBD0`). Commission the loop: a button in your HMI drives a PLC output; the PLC's counter comes back to the HMI. |
| **W11** | OPC UA in depth: browse the address space in UaExpert, the four NodeId types, Subscription / MonitoredItem / PublishingInterval / SamplingInterval / **deadband**, queue and discard policies, certificate trust (copy the client cert into the server's `trusted/certs`), security policies (None / Basic256Sha256), user tokens. Write a client against OPC Foundation's `UA-.NETStandard`. |
| **W12** | **Build your own OPC UA server**, modeling the Modbus data from M1 into an information model and exposing it. Understand where OPC UA PubSub / over MQTT fits. |

**Deliverables**
- `Iiot.Drivers.S7` and `Iiot.Drivers.OpcUa`, unified with Modbus behind an `IDeviceDriver` abstraction
- A physical demo rig you can show: PLC + transmitters + Raspberry Pi

**Gate check**: contrast where OPC UA and MQTT each apply, and explain why the field pattern is so often "**OPC UA to acquire → MQTT to the cloud**".

> ⚠️ **This month is the line between ¥15k and ¥20k.** If you fall behind, protect this month and cut ThingsBoard and the cloud platform from M4.

---

### M4 · Weeks 13–16: Data platform and visualization

| Week | Content |
|---|---|
| **W13** | Deploy and model TDengine: super tables / sub-tables / tags, one sub-table per device, write-throughput testing, downsampling queries (`INTERVAL`), retention (`KEEP`). Compare against InfluxDB on ecosystem, Chinese-language support, and SQL friendliness. |
| **W14** | Grafana on top of TDengine: live dashboards and alert rules, template variables for switching between devices, compression and hot/cold tiering. |
| **W15** | Deploy ThingsBoard: device onboarding → thing model → rule chain → dashboard. Internalize the **attributes / telemetry / RPC / device shadow** abstraction — it's the shared vocabulary of every IoT platform. |
| **W16** | Pick **one** of Alibaba Cloud IoT or Huawei Cloud IoTDA and get it working: thing-model definition, device shadow, OTA updates, rule engine forwarding to your own HTTP service. The free tier is enough. |

**Deliverable**: a `docker-compose.yml` (EMQX + TDengine + Grafana) that brings the whole stack up in one command, with the end-to-end path working.

**Gate check**: design a storage model that sustains **500 devices × 20 tags × 1s**, and compute on the spot the daily row count, raw volume, compressed volume, and one-year disk footprint.
> Worked example: 500 × 20 × 86400 = 864 million rows/day. With a super-table design at roughly 20 bytes per row and 10:1 compression, that's about 1.7 GB/day → roughly 620 GB per year. Conclusion: downsampling and tiered retention are mandatory (raw for 30 days, one-minute aggregates for a year).

---

### M5 · Weeks 17–20: The integration project (the centerpiece of your résumé)

**Project: Shop-floor equipment acquisition and monitoring system**

```
Field devices (Modbus RTU/TCP · Siemens S7 · OPC UA)
   │  IDeviceDriver abstraction + tag-list configuration (Excel / JSON import)
   ▼
Edge gateway (Raspberry Pi · .NET 10)
   polling scheduler → SQLite buffer → store-and-forward → link-quality stats
   │  MQTT (EMQX) + thing-model JSON
   ▼
Ingestion service (.NET) → TDengine
   └─ Alarm engine (threshold / duration / severity / debounce / recovery notice)
   │
   ├─ WPF HMI (live charts · tag monitor · manual writes · alarm list · history)
   └─ Grafana dashboard
```

**The details that earn the offer** — interviewers will dig here
- OEE calculation (availability × performance × quality)
- Per-device uptime and link-quality statistics — one glance shows which device keeps dropping frames
- Hot-reloadable tag lists (change configuration without stopping acquisition)
- **A one-click 200-device simulator** for load testing — proof you thought about scale
- Timestamp strategy: edge time vs. platform time, and what to do when a field device's clock is wrong

| Week | Content |
|---|---|
| W17–18 | Edge layer and platform layer connected end to end |
| W19 | WPF HMI |
| W20 | Load testing, architecture diagram, README, deployment scripts, **a 3–5 minute demo video** |

---

### M6 · Weeks 21–24: The job search

| Week | Content |
|---|---|
| **W21** | Rewrite the résumé. Reframe ten years of C# as "**industrial software / device connectivity**". Convert every role into STAR form with quantified numbers. Lead with both the old foundation (hands-on serial and Modbus) and the new project (a complete acquisition system). |
| **W22** | Rehearse the question bank out loud (see `interview-qa.md`). Practice **drawing architecture on a whiteboard** — with ten years of experience, the odds of being asked to draw are close to 100%. |
| **W23** | Apply: 15–20 companies in each category. **Start with your second-tier targets** as practice, then approach your first choices a week later. |
| **W24** | Iterate on the debriefs, then negotiate: anchoring, 13th/14th month salary, level, travel allowance, review cycle. |

**Target company categories in Chengdu**
1. **Industrial software / automation integrators** — HMI, SCADA, MES, acquisition platforms. The densest concentration of C# roles, though often with heavy travel.
2. **In-house smart-manufacturing / IT teams at manufacturers** — electronics, equipment, new energy, defense institutes. Stable and low-travel, but they weigh domain understanding and delivery experience more heavily.
3. **Energy and municipal utilities** — power, water, gas. Heavy on Modbus / DL-T645 / national protocol standards, with plenty of legacy C#. Your old experience is directly valuable here.
4. **IoT platform product companies** — apply as a supplement.

---

## 3. Resources

**Specifications and documentation** (first-hand sources, highest priority)

| Resource | Link |
|---|---|
| Modbus Application Protocol spec (PDF, no sign-up) | <https://www.modbus.org/file/secure/modbusprotocolspecification.pdf> |
| Modbus over Serial Line spec (PDF) | <https://www.modbus.org/file/secure/modbusoverserial.pdf> |
| Modbus specification index | <https://www.modbus.org/modbus-specifications> |
| OPC Foundation `UA-.NETStandard` (see `Samples/`) | <https://github.com/OPCFoundation/UA-.NETStandard> |
| MQTTnet | <https://github.com/dotnet/MQTTnet> |
| EMQX documentation | <https://docs.emqx.com/zh/emqx/latest/> |
| TDengine documentation | <https://docs.taosdata.com/> |
| HiveMQ *MQTT Essentials* — the clearest QoS explanation anywhere | <https://www.hivemq.com/mqtt-essentials/> |
| Snap7 — practise S7comm without real hardware | <https://snap7.sourceforge.net/> |
| ThingsBoard documentation | <https://thingsboard.io/docs/> |
| Grafana documentation | <https://grafana.com/docs/> |
| Polly / Serilog | <https://github.com/App-vNext/Polly> · <https://serilog.net/> |

**Tools**

| Tool | For | Link |
|---|---|---|
| Wireshark | Capturing Modbus TCP and MQTT | <https://www.wireshark.org/download.html> |
| diagslave | Free command-line Modbus slave simulator | <https://www.modbusdriver.com/diagslave.html> |
| Modbus Poll / Slave | The Windows GUI everyone in the field uses (trial) | <https://www.modbustools.com/> |
| pymodbus | Scriptable — easy to provoke exception responses | <https://github.com/pymodbus-dev/pymodbus> |
| crccalc | Check your CRC-16/MODBUS against a reference | <https://crccalc.com/> |
| UaExpert | The generic OPC UA client; required for M3 | <https://www.unified-automation.com/products/development-tools/uaexpert.html> |
| Prosys OPC UA Browser | A free alternative to UaExpert for browsing an address space | <https://www.prosysopc.com/products/opc-ua-browser/> |
| UA-.NETStandard-Samples | The official reference client and server code | <https://github.com/OPCFoundation/UA-.NETStandard-Samples> |
| docker compose | One command for EMQX + TDengine + Grafana | <https://docs.docker.com/compose/> |

> Every link above was checked and resolves. The two exceptions are **Snap7**
> (SourceForge) and **UaExpert** (Unified Automation), which answer 403 to an
> automated request — that is their bot protection, not a broken link; both open
> fine in a browser.

**Books**
- *OPC Unified Architecture* (Mahnke / Leitner / Damm) — the authoritative OPC UA text; the first four chapters are enough
- The programming manual for whichever PLC you buy (Siemens S7-1200 or a domestic equivalent)
- *Designing Data-Intensive Applications*, chapter 11 (stream processing) — for time-series data and message delivery semantics

**Communities**
- Chinese C# HMI-development communities (cnblogs is the most active for this niche)
- Bilibili: Siemens' official channel, HMI development tutorials
- Zhihu topics on industrial automation and the industrial internet

---

## 4. Verification: a 30-minute self-check at the end of each month

The bar is "**can you explain it at a whiteboard**", not "have you read about it".

| Milestone | Passing standard |
|---|---|
| End of M1 | Write out a Modbus FC03 frame by hand and explain every byte; read two real devices on a physical RS-485 bus |
| End of M2 | The Pi service runs 72 hours without crashing; after a 10-minute cable pull, the backlog drains with zero data loss |
| End of M3 | UaExpert browses your own OPC UA server; S7 DB read/write works; you can articulate OPC UA vs. MQTT |
| End of M4 | The full stack comes up with one docker-compose command; Grafana updates live; you can size storage on demand |
| End of M5 | README, architecture diagram and demo video complete; the 200-device load test passes |
| End of M6 | At least three full technical interviews completed; an offer in hand, or a concrete list of what's still missing |

---

## 5. Contingencies

| Risk | Response |
|---|---|
| Work eats the schedule; only 8h/week left | Minimum viable version: keep M1 / M2 / M3 / M5, cut ThingsBoard and the cloud platform from M4. Timeline slips to 7–8 months. |
| M3 (PLC + OPC UA) runs late | Immediately borrow two weeks from M4. **PLC and OPC UA are non-negotiable** — they are the ¥15k/¥20k dividing line. |
| Hardware arrives late | Start weeks 1–2 on a Modbus slave simulator and PLCSIM. Don't sit idle waiting. |
| You can learn it but can't explain it | From M1 onward, publish one technical blog post per month. Writing is the best rehearsal for speaking, and it doubles as a portfolio. |
| Six months in, still short of ¥15k | Take the ¥12–15k role and move again after a year of real project work. **Don't stay put over a ¥2k gap.** |
