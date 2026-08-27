# 🎛️ Home Assistant Hybrid uDMX & Art-Net to MQTT Gateway

> ⚠️ **EARLY RELEASE / BETA:** This project is currently in an early release stage. While fully functional in our test environments, you may still encounter bugs or unexpected behavior. Use with caution and feel free to report issues!

This Home Assistant App acts as a hybrid gateway between a physical uDMX USB dongle, network-based Art-Net nodes, and your MQTT network. It translates incoming MQTT commands into DMX light signals in real-time and features dynamic channel management via automatic XML configuration.

The application is completely **cross-platform (universal)** and runs seamlessly on both `ARM64` (e.g., Raspberry Pi) and `x86_64` (Intel/AMD) architectures using a framework-dependent .NET 9 environment.

---

## 🧠 Thoughts Behind the Project

The main goal of this project is to provide the **ability to use cheap DMX LED controllers with Home Assistant**, allowing for budget-friendly smart home lighting integrations without needing expensive professional hardware.

### ⚠️ Technical uDMX Limitations & Performance Tuning
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

## ✨ Features & Supported Color Modes

The gateway bridge supports advanced dynamic effects through dedicated **`colormodes`** that can be triggered via MQTT:

- **`fade`**: Smoothly fades transitions between different light scenes.
- **`cmorph`** *(Color Morph)*: Fades continuously between a predefined list of colors configured on the fixture. This mode can be set to transition either **randomly** or **sequentially** with individual loop speeds per fixture.
- **`pulse`**: Generates a pulsing effect with randomized brightness intensity using your currently selected color.
- **`none`**: Clears and deselects any active effect, returning the fixture to standard static control.

### Core Architecture Features:
- **Hybrid Hardware Engine:** Transmits data to physical uDMX hardware and streams network Art-Net broadcast/unicast packets simultaneously.
- **Thread-Safe Core:** Fully protected using mutex locking to eliminate race conditions or stalls between incoming MQTT packets and the high-speed DMX render loop.
- **Art-Net Disconnected Fallback:** Built on an asynchronous network socket layer (`SendAsync`). If no physical Art-Net node is present on your network, packets drop harmlessly without causing thread blocks or slowing down the uDMX loop timing.
- **RGBGROUP Sub-channel Support:** Smart handling of multi-channel fixtures (RGB, RGBW, RGBWW) where the master channel automatically links and configures the subsequent sub-channels.

---

## 🛠️ Installation & Setup

1. In Home Assistant, navigate to **Settings** -> **Apps** -> **App Store**.
2. Click the three dots in the top-right corner and select **Repositories**.
3. Paste your GitHub repository URL and click **Add**.
4. Find **uDMX to MQTT Gateway** in the store, click it, and press **Install**.

### ⚠️ Critical Step: Disable Protected Mode
Before starting the app, you **must disable Protected Mode**, otherwise the application will be blocked from accessing the host's USB controller layer, and your physical uDMX dongle will fail to open:
1. Go to the **uDMX to MQTT Gateway** app page in Home Assistant.
2. In the **Info** tab, locate the **Protected mode** *(Beskyttet tilstand)* toggle.
3. Turn the toggle **OFF**.
4. Click **Start** to run the app.

---

## ⚙️ Configuration via Home Assistant UI

Configure your gateway options directly in the "Configuration" tab of the App. Below is an example configuration supporting the optional Art-Net network attributes:

```yaml
mqtt_server_ip: "192.168.1.51"
mqtt_server_port: 1883
mqtt_server_user: "mqtttoudmx"
mqtt_server_password: "your_secure_password"
fade_stepsize: 10
colormorph_fade_stepsize: 10
artnet_target_ip: "255.255.255.255"
artnet_enabled: false
artnet_universe: 0
fixtures:
  - name: "conservatory_cabinet"
    start_channel: 0
    type: "RGBW"
    colormorph_random_color: true
    colormorph_speed: 25
  - name: "conservatory_spot"
    start_channel: 16
    type: "RGB"
    colormorph_random_color: false
    colormorph_speed: 100
```

### Supported Fixture Types (`type`):
- `Single` / `Dimmer` / `W` (1 DMX channel)
- `RGB` (3 DMX channels)
- `RGBW` (4 DMX channels)
- `RGBWW` (5 DMX channels)

---

## 📐 XML Channel Logic (RGBGROUP)

When the app starts, the internal Python script dynamically calculates the total channel requirement and compiles the `default.cfg` XML structure.

If you define an **`RGBW` light on channel 0**, the script automatically reserves the next three slots (channels 1, 2, and 3) as helper channels. In the generated XML file, it translates to:

- **Channel 0 (Master):** Gets assigned the type `RGBW`, custom color arrays, MQTT control topics, and specific colormorph speeds.
- **Channels 1, 2, and 3 (Sub-channels):** Automatically created with the type `RGBGROUP0`, linking them directly to master channel 0 so the C# application knows they form a single hardware unit.

---

## 💡 Hardware Procurement & Installation Advice

- **Noise Sensitivity:** Keep the physical uDMX cable isolated and completely **away from high-voltage AC power cables**. The underlying differential line chips are very sensitive to EM noise, which can cause erratic light flickering. Kept shielded, the dongle is incredibly stable.
- **Chipset Compatibility:** This gateway is built specifically for **Native USB Microcontrollers (True uDMX)**, which usually run an open-source firmware flashed onto an **Atmel AVR ATmega8** or **ATmega88** microcontroller. It talks directly to your computer using native USB commands instead of pretending to be a serial COM port.
- **FTDI Incompatibility:** DMX dongles built around FTDI chips (like standard Enttec Open DMX cables) **will not work** with this app, as they rely on serial port bit-banging rather than native USB endpoint transfers.
- **Sourcing:** Search marketplaces like AliExpress or eBay specifically for keywords like: `"uDMX 512 controller"`.

---

## 📄 License
This project is developed open-source for personal use in Home Assistant smart-home automation environments.
