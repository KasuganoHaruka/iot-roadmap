# 半年转型工业物联网

**语言：** [English](README.md) · [中文](README.zh.md)

一份把十年通用 C# 经验转向工业物联网岗位的 24 周个人计划，以及计划所依托的代码骨架。

这是学习材料，不是可复用的库。这里没有任何东西是给人依赖的。

## 代码是**故意**没写完的

`src/` 里有十四个方法抛 `NotImplementedException`。它们不是缺陷，也不是谁忘了填的
TODO —— **它们就是练习题本身**。测试是先写好的，按路线图给的顺序把每个方法实现掉，
就是这门课的全部内容。

```
$ cd src && dotnet test
Failed!  - Failed:    19, Passed:     5, Skipped:     0, Total:    24
```

测试挂着才是这个仓库的正常状态。等这一行变成 24 个全过，它就算完成了。

| 周次 | 要实现什么 | 位置 |
|---|---|---|
| **W2** | CRC16，先逐位再查表 | `Iiot.Drivers.Modbus/Crc16.cs` |
| **W3** | 地址解析、数值编解码（字序）、批量读规划、PDU 构造、响应解析、写操作 | `Iiot.Drivers.Modbus/` |
| **W7** | 基于 SQLite 的断网续传缓冲 —— 追加、取待发、标记已发、清理、计数 | `Iiot.EdgeGateway/Buffering/` |

## 目录

```
roadmap.{en,zh}.md     计划本体：M1-M6 六个里程碑，共 24 周
weekly-checklist.md    逐周任务清单，带「实际投入小时」一栏
hardware.md            买什么（预算 1500-3000 元），以及二手市场上要避开什么
interview-qa.md        面试题库与答案要点 —— 要出声讲，不要默读
progress.md            每月末 30 分钟的闸门自测
src/                   练习题所在的解决方案
```

另有 `roadmap.ja.md`。它**不再更新**，与英文版冲突时以英文为准。

## 解决方案结构

三个项目加一个测试项目，均为 `net10.0`：

- **`Iiot.Abstractions`** —— 驱动契约（`IDeviceDriver`、`TagDefinition`、`TagValue`、
  `Quality`）。其余项目都依赖它，而它不依赖任何东西。
- **`Iiot.Drivers.Modbus`** —— 从零手写的 Modbus 库。**刻意不用 NModbus**，因为写这个
  过程才是重点。`IModbusTransport` 把 RTU 与 TCP 唯一真正不同的地方 —— 帧格式与校验 ——
  隔离出去，上层驱动因此只需写一遍；具体的传输实现尚待编写。32 位数值的字序可配置
  （ABCD / CDAB / BADC / DCBA），批量读规划器负责把离散地址合并成最少的请求数。
- **`Iiot.EdgeGateway`** —— Worker Service：轮询设备、断网时把测点缓冲进 SQLite、
  恢复后经 MQTT 上传。可发布为 `linux-arm64` 单文件，在树莓派上以 systemd 服务运行；
  `deploy/` 里放着 unit 文件和按序列号固定 USB 串口设备名的 udev 规则。

## 构建

需要 **.NET 10 SDK**。`src/global.json` 把它锁定在 10.0.400、`rollForward: latestFeature`，
这样机器上装了更高大版本的 SDK 也不会悄悄接管构建。

```sh
cd src
dotnet build
dotnet test
```

选 .NET 10 而不是 8，理由不是性能：**.NET 8 于 2026-11-10 停止支持**。一个半年期的项目，
起步两个月就用上了 EOL 的框架，是面试时没必要去解释的事。

## 状态

2026-09 起步，目标 2027-03 前入职。

六个月目前在 [`progress.md`](progress.md) 里全部标着「未开始」。那份文件就是闸门：
每月末做一次 30 分钟自测，标准是**能对着白板讲出来**，不是「看过」。
