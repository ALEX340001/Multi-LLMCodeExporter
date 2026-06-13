#!/bin/bash
cd /home/alex340001/Документы/projects/project-csharp/Multi-LLMCodeExporter_11-06-26_InLinux/Multi-LLMCodeExporter
dotnet run --project src/LLMCodeExporter.CLI/LLMCodeExporter.CLI.csproj -- "$@"