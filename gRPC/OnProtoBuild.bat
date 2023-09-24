xcopy %~dp0Server\GrpcTest\Protos\main.proto "%~dp0..\Unity\Assets\_Project\Generated Things\GRPC\Protos\." /Y

@echo off
setlocal enableextensions disabledelayedexpansion

set "search=%GRPCServer"
set "replace=%GRPCClient"

set "textFile=%~dp0..\Unity\Assets\_Project\Generated Things\GRPC\Protos\main.proto"

for /f "delims=" %%i in ('type "%textFile%" ^| find /v /n "" ^& break ^> "%textFile%"') do (
    set "line=%%i"
    setlocal enabledelayedexpansion
    set "line=!line:*]=!"
    if defined line set "line=!line:%search%=%replace%!"
    >>"%textFile%" echo(!line!
    endlocal
)

cd %~dp0..\Unity\Assets\_Project\Generated Things\GRPC

protoc --proto_path=./Protos --csharp_out=./CSharp --grcp_out=./CSharp --plugin=protoc-gen-grcp=%~dp0/grpc-protoc_windows_x64-1.57.0-dev/grpc_csharp_plugin.exe ./Protos/main.proto
