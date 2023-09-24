// COPY

// PAST


cd %~dp0..\Unity\Assets\_Project\Generated Things\GRPC

protoc --proto_path=./Protos --csharp_out=./CSharp --grcp_out=./CSharp --plugin=protoc-gen-grcp=%~dp0/grpc-protoc_windows_x64-1.57.0-dev/grpc_csharp_plugin.exe ./Protos/main.proto

cmd /k