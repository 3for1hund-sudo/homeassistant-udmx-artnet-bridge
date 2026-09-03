## 1.1.8
- Fixed single fixture color, not turning on. program identified led type incorrectly.
- corrected, so program detects single fixture, and use brigthness value, for led. instead of w color.
- Change json return packet routine, color return is not needed, for single color. only brightness.
- Tested RGBWW, fixed issue with json packet decoding. should be working proper now, except issue with python script and colormorph.

## 1.1.7
- Updated MQTT broker backend scripts.
- Added support for custom color variables in `generate_config.py`.

## 1.1.6
- Artnet support added

## 1.1.0
- Initial beta release bridging uDMX and Art-Net nodes.
