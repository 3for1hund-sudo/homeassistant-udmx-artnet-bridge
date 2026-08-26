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



        public string mqqt_server_ip = "127.0.0.1";
        public string mqqt_server_port = "5000";
        public string mqqt_server_password = "231081922048#";
        public string mqqt_server_user = "kolonihave";

        public int number_of_channels = 511;
        public string mqqt_device_id = "MQQTUDMX01";




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

        
        public string gui_rectangle_color = "";
        public int gui_current_channel_selected = 0;
        public string gui_channel_name = "";
        public string gui_subscribe_control_topic;
        public string gui_publish_topic;
        public string gui_led_type;
        public string gui_current_led_value;
        public List<string> Led_types = new List<string>() { "W", "R", "G", "B", "RGB", "RGBW", "RGBWw" };
        public string gui_group_message_label = "";
        public string textbox1_status_messages = "";

        // full topic labels shown in gui, which user can copy to memory, using button
        public string gui_full_control_topic = "null";
        public string gui_full_publish_topic = "null";


        // list of colors for selected effect
        public List<string> gui_colors_name_ch_current = new List<string>();
        public List<string> gui_colors_red_ch_current = new List<string>();
        public List<string> gui_colors_blue_ch_current = new List<string>();
        public List<string> gui_colors_green_ch_current = new List<string>();
        public List<string> gui_colors_warm_white_ch_current = new List<string>();
        public List<string> gui_colors_cold_white_ch_current = new List<string>();
    
        // variables for creating colors, with the canvas, for the list.
      
        public string gui_textbox_color_name = "none";

        //unsaved data bit.
        public bool gui_unsaved_data = false;
        public bool gui_execute_config_save = false;

        #endregion


    }


}
