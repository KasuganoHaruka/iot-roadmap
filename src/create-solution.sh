#!/usr/bin/env bash
# 生成解决方案文件并把所有项目加进去。
# 手写 .sln 容易出错,所以用 dotnet CLI 生成。
set -euo pipefail
cd "$(dirname "$0")"

dotnet new sln -n Iiot --force
dotnet sln add Iiot.Abstractions/Iiot.Abstractions.csproj
dotnet sln add Iiot.Drivers.Modbus/Iiot.Drivers.Modbus.csproj
dotnet sln add Iiot.EdgeGateway/Iiot.EdgeGateway.csproj
dotnet sln add tests/Iiot.Drivers.Modbus.Tests/Iiot.Drivers.Modbus.Tests.csproj

echo
echo "完成。接下来:"
echo "  dotnet build"
echo "  dotnet test          # 现在会红,W2/W3 的任务就是让它变绿"
