#!/bin/bash
if [ -f "/app/default.cfg" ]; then rm /app/default.cfg; fi
python3 /app/generate_config.py

# RETTET: Vi starter nu dll-filen gennem dotnet runtime
exec dotnet uDMXtoMQTT.dll
