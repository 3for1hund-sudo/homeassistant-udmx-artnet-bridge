using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;


namespace uDMXtoMQTT
{
    public class Sol_data_class
    {
        #region Members
        // header info read from unit



        public string file_mode_path;



        public string mqqt_server_ip = "192.168.1.51";
        public string mqqt_server_port = "1883";
        public string mqqt_server_password = "Dummypass";
        public string mqqt_server_user = "mqtttoudmx";

        public int number_of_channels = 20;
        public string mqqt_device_id = "MQQTUDMX01";

        // ART-NET KONFIGURATION (Styres dynamisk via dmx_collection)
        public string artnet_target_ip = "255.255.255.255"; // Netværks-broadcast eller fast IP på dit Art-Net node
        public bool artnet_enabled = true;                   // Slå Art-Net output globalt til/fra
        public byte artnet_universe = 0;                     // Det fysiske Art-Net univers (Typisk 0)



        public string[] channel_name = new string[512];
        public string[] subscribe_control_topic = new string[512];
        public string[] publish_topic = new string[512];
        public string[] led_type = new string[512];
        // contains the current 0-255 led value currently set on the controller.
        public byte[] current_led_value = new byte[512];        
        public byte[] Qos_level = new byte[512];


        //set by json packet recieved routine, when new command has been recieved
        public bool[] led_change_in_progress = new bool[512];




        public string[] assembled_control_topic = new string[512];
        public string[] assembled_publish_topic = new string[512];

        public byte[] assembled_qos_level = new byte[512];
        public int Number_of_assembled_topics = 0;
        public int[] ch_numbers_assembled_topics = new int[512];
        public string[] led_type_assembled_topics = new string[512];
        public string[] led_effect_assembled_topics = new string[512];

        // sets the value controller code, should ajust the channels to
        public byte[] current_led_value_new_target = new byte[512];
        // is led band in on or off state
        public bool[] controller_state = new bool[512];

        // contains new brightnes % recived, from mqtt
        public byte[] brightness_controller = new byte[512];

        public int[] brightness_counter = new int[512];
        //original values, for light, before any brightness adjustment. so we know what 100% is.
        public byte[] brightness_whitemax_mixed_rgb = new byte[512];
        //contains if channel should use an effect
        public string[] effect = new string[512];
        // step size light is adjusted, during fade
        public int fade_stepsize = 10;
        //where channel should fade to
        public byte[] fade_target = new byte[512];

        // controller tics until next change
        public int[] tics_to_next_pulse = new int[512];
        // used by effects, to time, when to change effect
        public int[] controller_tic_start = new int[512];
        public int controller_tic_counter = 0;

        //########################################################################
        // effects

        // enables random color
        public bool[] colormorph_random_color = new bool[512];

        //standard main colors
        public byte[] colors_red = { 0, 255, 255, 0, 0, 255, 0, 255, 192, 128, 128, 128, 0, 128, 0, 0 };
        public byte[] colors_green = { 0, 255, 0, 255, 0, 255, 255, 0, 192, 128, 0, 128, 128, 0, 128, 0 };
        public byte[] colors_blue = { 0, 255, 0, 0, 255, 0, 255, 255, 192, 128, 0, 0, 0, 128, 128, 128 };
        public byte[] colors_white = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public byte[] colors_white2 = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        public string[] colors_name = { "black", "white", "red", "lime", "blue", "yellow", "cyan", "magenta", "silver", "gray", "maroon", "olive", "green", "purple", "teal", "navy" };

        // simple ramp sequence, for single color polymorph. default values written
        public byte[] colormorph_single_color = { 10, 20, 30, 40, 50, 60, 70, 80, 80, 70, 60, 50, 40, 30, 20, 10 };

        //contains individiual channels, colormorph color map
        public byte[,] colors_red_file = new byte[512,16];
        public byte[,] colors_green_file = new byte[512,16];
        public byte[,] colors_blue_file = new byte[512,16];
        public byte[,] colors_white_file = new byte[512,16];
        public byte[,] colors_white2_file = new byte[512, 16];
        public string[,] colors_name_file = new string[512, 16];      

        

        // time delay, until morphing to next color
        public int[] colormorph_speed = new int[512];        
        // holds last color morphed to. its so we dont random the same color twice
        public int[] last_random_color = new int[512];
        //holds stepsize colormorph, fade to next.
        public int colormorph_fade_stepsize = 10;

        //############################################################
        // gui variables

        
     
        public List<string> Led_types = new List<string>() { "W", "R", "G", "B", "RGB", "RGBW", "RGBWw" };
    
        public string textbox1_status_messages = "";


        #endregion


    }


}
