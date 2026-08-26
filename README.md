# uDMX to MQTT Gateway Add-on for Home Assistant

This Home Assistant Add-on acts as a gateway between a physical uDMX USB dongle and your MQTT network. It translates incoming MQTT commands into DMX light signals in real-time and features dynamic channel management via XML configuration.

The application is completely **cross-platform (universal)** and runs seamlessly on both ARM64 (e.g., Raspberry Pi) and x86_64 (Intel/AMD) architectures.

## 🧠 Thoughts Behind the Project

The main goal of this project is to provide the **ability to use cheap DMX LED controllers with Home Assistant**, allowing for budget-friendly smart home lighting integrations without needing expensive professional hardware.

### ⚠️ Technical Limitations & Performance Tuning
Because this setup utilizes a very affordable, entry-level uDMX dongle, there are certain hardware limitations you should be aware of:
- **No Channel Ranges:** Due to device limitations, the application cannot use `setchannel` ranges. It updates individual channels directly.
- **Memory Constraints:** The dongle handles data processing via limited internal memory resources.
- **Channel vs. Frame Rate (FPS) Trade-off:** 
  - If you need smooth **44 FPS** transitions (ideal for fast color fades), the hardware limit is approximately **25 channels**.
  - If you can accept a lower frame rate of **22 FPS**, you can expand your setup to around **50 channels**.

### 📊 Real-Time FPS Monitoring
To help you find the sweet spot for your hardware setup, **the application explicitly outputs the current effect render FPS directly to the log**. By monitoring the live log in the Home Assistant UI, you can instantly see if you are hitting the technical limit of your uDMX dongle or if your current channel pool allows for higher performance.

### 🎭 Beyond LEDs: Stage Lights & Moving Heads
While designed for LED fixtures, this gateway also allows you to **connect directly to professional stage lights**. For example, you can easily control the physical pan/tilt movements of a moving head lamp by configuring those specific moving channels as a standard `White` (`W` / `Single`) channel in Home Assistant. This lets you adjust the position values just like a dimmer slider!

---

## 🚀 Features & Supported Color Modes

The gateway bridge supports advanced dynamic effects through dedicated **`colormodes`** that can be triggered via MQTT:

- **`fade`**: Smoothly fades transitions between different light scenes.
- **`colormorph`**: Fades continuously between a predefined list of colors configured on the fixture. This mode can be set to transition either **randomly** or **sequentially**.
- **`pulse`**: Generates a pulsing effect with randomized brightness intensity using your currently selected color.
- **`none`**: Clears and deselects any active effect, returning the fixture to standard static control.

### Core Architecture Features:
- **Universal Architecture:** Runs effortlessly on any hardware platform using a framework-dependent .NET 9 environment.
- **Dynamic DMX Configuration:** Automatically generates the internal `default.cfg` XML structure based on your Home Assistant UI options using an integrated Python script.
- **RGBGROUP Sub-channel Support:** Smart handling of multi-channel fixtures (RGB, RGBW, RGBWW) where the master channel automatically links and configures the subsequent sub-channels.

---

## 🛠️ Installation & Setup

1. In Home Assistant, navigate to **Settings** -> **Add-ons** -> **Add-on Store**.
2. Click the three dots in the top-right corner and select **Repositories**.
3. Paste your GitHub repository URL (e.g., `https://github.com`) and click **Add**.
4. Find **uDMX to MQTT Gateway** in the store, click it, and press **Install**.

### ⚠️ Critical Step: Disable Protected Mode
Before starting the add-on, you **must disable Protected Mode**, otherwise the application will be blocked from accessing the host's USB controller and your uDMX dongle will not connect:
1. Go to the **uDMX to MQTT Gateway** add-on page in Home Assistant.
2. In the **Info** tab, locate the **Protected mode** (Beskyttet tilstand) toggle.
3. Turn the toggle **OFF**.
4. Click **Start** to run the add-on.

## ⚙️ Configuration via Home Assistant UI

Configure your gateway options directly in the "Configuration" tab of the Add-on. Below is an example configuration:

```yaml
mqtt_server_ip: "192.168.1.51"
mqtt_server_port: 1883
mqtt_server_user: "mqtttoudmx"
mqtt_server_password: "your_secure_password"
fade_stepsize: 10
colormorph_fade_stepsize: 10
fixtures:
  - name: "conservatory_cabinet"
    start_channel: 0
    type: "RGBW"
  - name: "conservatory_spot"
    start_channel: 16
    type: "RGB"
```

### Supported Fixture Types (`type`):
- `Single` / `Dimmer` / `W` (1 DMX channel)
- `RGB` (3 DMX channels)
- `RGBW` (4 DMX channels)
- `RGBWW` (5 DMX channels)

## 📐 XML Channel Logic (RGBGROUP)

When the add-on starts, the internal Python script dynamically calculates the total channel requirement and compiles the `default.cfg` file.

If you define a fixture like an **`RGBW` light on channel 0**, the script automatically reserves the next three slots (channels 1, 2, and 3) as helper channels. In the generated XML file, it translates to:

- **Channel 0 (Master):** Gets assigned the type `RGBW`, custom color arrays, MQTT control topics (`rgbw/control/`), and colormorph settings.
- **Channels 1, 2, and 3 (Sub-channels):** Automatically created with the type `RGBGROUP0`, linking them directly to master channel 0 so the C# application knows they belong together.

## 💻 Local Testing on Windows (Visual Studio)

You can easily run and debug this application locally on your Windows PC. It will connect to your live Home Assistant Raspberry Pi MQTT broker over the network for real-time testing:

1. Set the solution platform dropdown in the top toolbar of Visual Studio to **`Any CPU`**.
2. Run the project in **Debug** or **Release** mode to test MQTT logic and console outputs.
3. Every time you compile a **Release build**, the project automatically outputs a clean, cross-platform package to your `bin/Release/net9.0/publish/` folder. This includes all required `.dll` dependencies and the critical `uDMXtoMQTT.runtimeconfig.json` file. Only the contents of this publish folder need to be copied to your Raspberry Pi.

## 📄 License
This project is developed for personal use in Home Assistant smart-home environments.

## advice
Keep the udmx cable away from powercables, its very sensitive to noise. It can cause weird light behaviour. othervise the dongle is very stable.

Native USB Microcontrollers (True uDMX)The Chip: Usually an Atmel AVR Atmega8 or Atmega88 microcontroller.
 How it works: The chip runs the open-source uDMX firmware. 
 It talks directly to your computer using native USB commands instead of pretending to be a serial COM port.

The udmx dmx dongles, are often the cheap clones. dmx dongles with FTDI chips, wont work.
search aliexpress : uDMX 512 controller