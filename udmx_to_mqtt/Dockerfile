FROM mcr.microsoft.com/dotnet/runtime:9.0-bookworm-slim

# 1. Installer libusb, udev-support og PYTHON 3
RUN apt-get update && apt-get install -y libusb-1.0-0 libusb-1.0-0-dev udev python3 && rm -rf /var/lib/apt/lists/*

# 2. Opret den interne mappe til selve programmet
WORKDIR /app

# 3. Kopier ALLE filer fra din addon-mappe (inkl. uDMXtoMQTT.dll og run.sh) ind i /app
COPY . .

# 4. DYNAMISK LOKALISERING AF LIBUSB (Virker fejlfrit på både aarch64 og x86_64)
RUN LIBUSB_PATH=$(find /usr/lib -name "libusb-1.0.so" | head -n 1) && cp "$LIBUSB_PATH" /app/libusb-1.0.so

# 5. RET WINDOWS LINJESKIFT (Fjerner \r automatisk fra run.sh under build)
RUN sed -i 's/\r$//' run.sh

# 6. Giv fuld kørselstilladelse til opstartsscriptet
RUN chmod +x run.sh

# 7. Skift startmetode (ENTRYPOINT) til at køre run.sh
ENTRYPOINT [ "/bin/bash", "./run.sh" ]
