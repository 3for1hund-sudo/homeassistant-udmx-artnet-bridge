import json

# 1. Læs Home Assistant UI indstillinger
with open("/data/options.json", "r") as f:
    options = json.load(f)

fixtures_list = options.get("fixtures", [])
assigned_types = {fix["start_channel"]: fix for fix in fixtures_list}

type_offsets = {"RGB": 2, "RGBW": 3, "RGBWW": 4}

# DYNAMISK LOGIK: Beregn højeste påkrævede kanal ud fra dine fixtures
highest_channel_needed = 17 

for fix in fixtures_list:
    start_ch = fix["start_channel"]
    fix_type = fix["type"]
    
    needed_for_this = start_ch + 1
    if fix_type in type_offsets:
        needed_for_this += type_offsets[fix_type]
        
    if needed_for_this > highest_channel_needed:
        highest_channel_needed = needed_for_this

total_channels = min(highest_channel_needed, 512)

# Faste arraysæt fra backup
def_red = "0,255,255,0,0,255,0,255,192,128,128,128,0,128,0,0"
def_green = "0,255,0,255,0,255,255,0,192,128,0,128,128,0,128,0"
def_blue = "0,255,0,0,255,0,255,255,192,128,0,0,0,128,128,128"
def_white = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0"
def_names = "black,white,red,lime,blue,yellow,cyan,magenta,silver,gray,maroon,olive,green,purple,teal,navy"

ch0_red = "0,255,255,0,255,0,0,255,192,128,128,128,0,128,0,0"
ch0_green = "0,255,0,255,255,0,255,0,192,128,0,128,128,0,128,0"
ch0_blue = "0,255,0,0,0,255,255,255,192,128,0,0,0,128,128,128"
ch0_white = "0,0,0,0,96,0,0,0,0,0,0,0,0,0,0,0"
ch0_names = "black,white,red,lime,yellow1,blue,cyan,magenta,silver,gray,maroon,olive,green,purple,teal,navy"

w_white = "10,20,30,40,50,60,70,80,80,70,60,50,40,30,20,0"
w_names = "white0,white1,white2,white3,white4,white5,white6,white7,white8,white9,white10,white11,white12,white13,white14,white15"


def get_channel_colors(fixture_data, is_ch0=False):
    user_colors = fixture_data.get("colors", [])
    ref_r = (ch0_red if is_ch0 else def_red).split(",")
    ref_g = (ch0_green if is_ch0 else def_green).split(",")
    ref_b = (ch0_blue if is_ch0 else def_blue).split(",")
    ref_w = (ch0_white if is_ch0 else def_white).split(",")
    ref_n = (ch0_names if is_ch0 else def_names).split(",")
    
    if not user_colors:
        return ",".join(ref_r), ",".join(ref_g), ",".join(ref_b), ",".join(ref_w), ",".join(ref_n)
        
    out_r, out_g, out_b, out_w, out_n = [], [], [], [], []
    
    for idx, col in enumerate(user_colors[:16]):
        out_n.append(str(col.get("name", ref_n[idx])))
        out_r.append(str(col.get("r", ref_r[idx])))
        out_g.append(str(col.get("g", ref_g[idx])))
        out_b.append(str(col.get("b", ref_b[idx])))
        out_w.append(str(col.get("w", ref_w[idx])))
        
    for i in range(len(user_colors), 16):
        out_r.append(ref_r[i])
        out_g.append(ref_g[i])
        out_b.append(ref_b[i])
        out_w.append(ref_w[i])
        out_n.append(ref_n[i])
        
    return ",".join(out_r), ",".join(out_g), ",".join(out_b), ",".join(out_w), ",".join(out_n)

sub_channels_queue = []
current_parent = 0

setup_xml = f"""<setup>
        <mqqt_server_ip>{options.get('mqtt_server_ip', '192.168.1.51')}</mqqt_server_ip>

        <mqqt_server_port>{options.get('mqtt_server_port', 1883)}</mqqt_server_port>

        <mqqt_server_password>{options.get('mqtt_server_password', 'YourPassword')}</mqqt_server_password>

        <mqqt_server_user>{options.get('mqtt_server_user', 'homeassist user')}</mqqt_server_user>

        <number_of_channels>{total_channels}</number_of_channels>

        <mqqt_device_id>MQQTUDMX01</mqqt_device_id>

        <fade_stepsize>{options.get('fade_stepsize', 10)}</fade_stepsize>

        <colormorph_fade_stepsize>{options.get('colormorph_fade_stepsize', 10)}</colormorph_fade_stepsize>

        <artnet_target_ip>{options.get('artnet_target_ip', '255.255.255.255')}</artnet_target_ip>

        <artnet_enabled>{str(options.get('artnet_enabled', False))}</artnet_enabled>

        <artnet_universe>{options.get('artnet_universe', 0)}</artnet_universe>

</setup>

<channels>"""

for i in range(total_channels):
    
    if sub_channels_queue:
        parent_ch = sub_channels_queue.pop(0)
        setup_xml += f"""
<ch{i}>
        <channel_name>no_name_ch{i}</channel_name>
        <subscribe_control_topic>kitchen/led/floor/</subscribe_control_topic>
        <publish_topic>kitchen/led/floor/currentvalue/</publish_topic>
        <led_type>RGBGROUP{parent_ch}</led_type>
        <qos_level>0</qos_level>
</ch{i}>"""

    elif i in assigned_types:
        f = assigned_types[i]
        t = f["type"]
        name = f.get("name", f"no_name_ch{i}")
        current_parent = i
        
        if t == "RGB":
            sub_topic = "rgb/control/"
            pub_topic = "rgbw/publish/"
        elif t in ["RGBW", "RGBWW"]:
            sub_topic = "rgbw/control/"
            pub_topic = "rgbw/publish/"
        else:
            sub_topic = "kitchen/led/floor/"
            pub_topic = "rgbw/publish/"
        
        if t in ["Single", "Dimmer", "W"]:
            # Dynamisk indlæsning af random/speed for hvide kanaler (Med defaults)
            rand_bool = str(f.get("colormorph_random_color", False))
            speed_val = str(f.get("colormorph_speed", 100))
            
            setup_xml += f"""
<ch{i}>
        <channel_name>{name}</channel_name>
        <subscribe_control_topic>{sub_topic}</subscribe_control_topic>
        <publish_topic>{pub_topic}</publish_topic>
        <led_type>W</led_type>
        <qos_level>0</qos_level>
        <colormorph_white>{w_white}</colormorph_white>
        <colors_name_file>{w_names}</colors_name_file>
        <colormorph_random_color>{rand_bool}</colormorph_random_color>
        <colormorph_speed>{speed_val}</colormorph_speed>

</ch{i}>"""
        else:
            is_ch0 = i == 0
            
            # HENT DYNAMISKE VÆRDIER FRA HA UI: Hvis de ikke findes, brug de gamle kanalspecifikke defaults
            default_rand = "True" if is_ch0 else "False"
            default_speed = "25" if is_ch0 else "100"
            
            # f.get() læser værdien hvis brugeren har sat den i UI'en
            user_rand = f.get("colormorph_random_color")
            user_speed = f.get("colormorph_speed")
            
            rand_bool = str(user_rand) if user_rand is not None else default_rand
            speed_val = str(user_speed) if user_speed is not None else default_speed
            
            r_array, g_array, b_array, w_array, n_array = get_channel_colors(f, is_ch0=is_ch0)
            
            setup_xml += f"""
<ch{i}>
        <channel_name>{name}</channel_name>
        <subscribe_control_topic>{sub_topic}</subscribe_control_topic>
        <publish_topic>{pub_topic}</publish_topic>
        <led_type>{t}</led_type>
        <qos_level>0</qos_level>
        <colormorph_red>{r_array}</colormorph_red>
        <colormorph_green>{g_array}</colormorph_green>
        <colormorph_blue>{b_array}</colormorph_blue>
        <colormorph_white>{w_array}</colormorph_white>
        <colors_name_file>{n_array}</colors_name_file>
        <colormorph_random_color>{rand_bool}</colormorph_random_color>
        <colormorph_speed>{speed_val}</colormorph_speed>

</ch{i}>"""
            
            if t in type_offsets:
                for _ in range(type_offsets[t]):
                    sub_channels_queue.append(i)

    else:
        setup_xml += f"""
<ch{i}>
        <channel_name>no_name_ch{i}</channel_name>
        <subscribe_control_topic>kitchen/led/floor/</subscribe_control_topic>
        <publish_topic>kitchen/led/floor/currentvalue/</publish_topic>
        <led_type>RGBGROUP{current_parent}</led_type>
        <qos_level>0</qos_level>
</ch{i}>"""

setup_xml += "\n</channels>"

with open("/app/default.cfg", "w", encoding="utf-8") as f_out:
    f_out.write(setup_xml.strip())
    
print(f"default.cfg blev oprettet med {total_channels} kanaler og fuld individuel colormorph support!")
