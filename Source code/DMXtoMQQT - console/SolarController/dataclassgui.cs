using System.ComponentModel;
using System.Windows.Input;
using System.Data;
using System.Collections.Generic;
//using System.Windows.Controls;
using MicroMvvm;
//using System.Windows.Media;

namespace uDMXtoMQTT
{
    public class dataclassgui : ObservableObject
    {
        #region construction
        public dataclassgui()
        {
            _data_header = new Sol_data_class();


        }
        #endregion 

        #region members
        Sol_data_class _data_header;

        #endregion

        #region propertiers

         
        public string mqqt_server_ip
        {
            get { return _data_header.mqqt_server_ip; }
            set
            {
                if (_data_header.mqqt_server_ip != value)
                {
                    _data_header.mqqt_server_ip = value;
                    RaisePropertyChanged("mqqt_server_ip");
                }
            }
        }

        public string mqqt_server_port
        {
            get { return _data_header.mqqt_server_port; }
            set
            {
                if (_data_header.mqqt_server_port != value)
                {
                    _data_header.mqqt_server_port = value;
                    RaisePropertyChanged("mqqt_server_port");
                }
            }
        }

        public string mqqt_server_password
        {
            get { return _data_header.mqqt_server_password; }
            set
            {
                if (_data_header.mqqt_server_password != value)
                {
                    _data_header.mqqt_server_password = value;
                    RaisePropertyChanged("mqqt_server_password");
                }
            }
        }

        public string mqqt_server_user
        {
            get { return _data_header.mqqt_server_user; }
            set
            {
                if (_data_header.mqqt_server_user != value)
                {
                    _data_header.mqqt_server_user = value;
                    RaisePropertyChanged("mqqt_server_user");
                }
            }
        }

        public string mqqt_device_id
        {
            get { return _data_header.mqqt_device_id; }
            set
            {
                if (_data_header.mqqt_device_id != value)
                {
                    _data_header.mqqt_device_id = value;
                    RaisePropertyChanged("mqqt_device_id");
                }
            }
        }

        public int number_of_channels
        {
            get { return _data_header.number_of_channels; }
            set
            {
                if (_data_header.number_of_channels != value)
                {
                    _data_header.number_of_channels = value;
                    RaisePropertyChanged("number_of_channels");
                }
            }
        }


        public string[] channel_name
        {
            get { return _data_header.channel_name; }
            set
            {
                if (_data_header.channel_name != value)
                {
                    _data_header.channel_name = value;
                    RaisePropertyChanged("channel_name");
                }
            }

        }

        public string[] subscribe_control_topic
        {
            get { return _data_header.subscribe_control_topic; }
            set
            {
                if (_data_header.subscribe_control_topic != value)
                {
                    _data_header.subscribe_control_topic = value;
                    RaisePropertyChanged("subscribe_control_topic");
                }
            }

        }

        public string[] led_type
        {
            get { return _data_header.led_type; }
            set
            {
                if (_data_header.led_type != value)
                {
                    _data_header.led_type = value;
                    RaisePropertyChanged("led_type");
                }
            }

        }

        public string[] publish_topic
        {
            get { return _data_header.publish_topic; }
            set
            {
                if (_data_header.publish_topic != value)
                {
                    _data_header.publish_topic = value;
                    RaisePropertyChanged("publish_topic");
                }
            }

        }

        public byte[] current_led_value
        {
            get { return _data_header.current_led_value; }
            set
            {
                if (_data_header.current_led_value != value)
                {
                    _data_header.current_led_value = value;
                    RaisePropertyChanged("current_led_value");
                }
            }

        }

        public int gui_current_channel_selected
        {
            get { return _data_header.gui_current_channel_selected; }
            set
            {
                if (_data_header.gui_current_channel_selected != value)
                {
                    _data_header.gui_current_channel_selected = value;
                    RaisePropertyChanged("gui_current_channel_selected");
                }
            }

        }

        public string gui_channel_name
        {
            get { return _data_header.gui_channel_name; }
            set
            {
                if (_data_header.gui_channel_name != value)
                {
                    _data_header.gui_channel_name = value;
                    RaisePropertyChanged("gui_channel_name");
                }
            }

        }

        public string gui_subscribe_control_topic
        {
            get { return _data_header.gui_subscribe_control_topic; }
            set
            {
                if (_data_header.gui_subscribe_control_topic != value)
                {
                    _data_header.gui_subscribe_control_topic = value;
                    RaisePropertyChanged("gui_subscribe_control_topic");
                }
            }

        }

        public string gui_publish_topic
        {
            get { return _data_header.gui_publish_topic; }
            set
            {
                if (_data_header.gui_publish_topic != value)
                {
                    _data_header.gui_publish_topic = value;
                    RaisePropertyChanged("gui_publish_topic");
                }
            }

        }

        public string gui_led_type
        {
            get { return _data_header.gui_led_type; }
            set
            {
                if (_data_header.gui_led_type != value)
                {
                    _data_header.gui_led_type = value;
                    RaisePropertyChanged("gui_led_type");
                }
            }

        }

        public string gui_current_led_value
        {
            get { return _data_header.gui_current_led_value; }
            set
            {
                if (_data_header.gui_current_led_value != value)
                {
                    _data_header.gui_current_led_value = value;
                    RaisePropertyChanged("gui_current_led_value");
                }
            }

        }

        //contains samplerate list used in combo box, and later written to the datalogger
        public List<string> Led_types
        {
            get { return _data_header.Led_types; }
            set
            {
                _data_header.Led_types = value;
                RaisePropertyChanged("Led_types");
            }
        }

        public string gui_group_message_label
        {
            get { return _data_header.gui_group_message_label; }
            set
            {
                if (_data_header.gui_group_message_label != value)
                {
                    _data_header.gui_group_message_label = value;
                    RaisePropertyChanged("gui_group_message_label");
                }
            }

        }

        public byte[] Qos_level
        {
            get { return _data_header.Qos_level; }
            set
            {
                if (_data_header.Qos_level != value)
                {
                    _data_header.Qos_level = value;
                    RaisePropertyChanged("Qos_level");
                }
            }

        }

        public string[] assembled_control_topic
        {
            get { return _data_header.assembled_control_topic; }
            set
            {
                if (_data_header.assembled_control_topic != value)
                {
                    _data_header.assembled_control_topic = value;
                    RaisePropertyChanged("assembled_control_topic");
                }
            }

        }

        public string[] assembled_publish_topic
        {
            get { return _data_header.assembled_publish_topic; }
            set
            {
                if (_data_header.assembled_publish_topic != value)
                {
                    _data_header.assembled_publish_topic = value;
                    RaisePropertyChanged("assembled_publish_topic");
                }
            }

        }

        public byte[] assembled_qos_level
        {
            get { return _data_header.assembled_qos_level; }
            set
            {
                if (_data_header.assembled_qos_level != value)
                {
                    _data_header.assembled_qos_level = value;
                    RaisePropertyChanged("assembled_qos_level");
                }
            }

        }

        public int Number_of_assembled_topics
        {
            get { return _data_header.Number_of_assembled_topics; }
            set
            {
                if (_data_header.Number_of_assembled_topics != value)
                {
                    _data_header.Number_of_assembled_topics = value;
                    RaisePropertyChanged("Number_of_assembled_topics");
                }
            }

        }

        public int[] ch_numbers_assembled_topics
        {
            get { return _data_header.ch_numbers_assembled_topics; }
            set
            {
                if (_data_header.ch_numbers_assembled_topics != value)
                {
                    _data_header.ch_numbers_assembled_topics = value;
                    RaisePropertyChanged("ch_numbers_assembled_topics");
                }
            }

        }

        public string[] led_type_assembled_topics
        {
            get { return _data_header.led_type_assembled_topics; }
            set
            {
                if (_data_header.led_type_assembled_topics != value)
                {
                    _data_header.led_type_assembled_topics = value;
                    RaisePropertyChanged("led_type_assembled_topics");
                }
            }

        }
        public string[] led_effect_assembled_topics
        {
            get { return _data_header.led_effect_assembled_topics; }
            set
            {
                if (_data_header.led_effect_assembled_topics != value)
                {
                    _data_header.led_effect_assembled_topics = value;
                    RaisePropertyChanged("led_effect_assembled_topics");
                }
            }

        }

        public byte[] current_led_value_new_target
        {
            get { return _data_header.current_led_value_new_target; }
            set
            {
                if (_data_header.current_led_value_new_target != value)
                {
                    _data_header.current_led_value_new_target = value;
                    RaisePropertyChanged("current_led_value_new_target");
                }
            }

        }

        public bool[] controller_state
        {
            get { return _data_header.controller_state; }
            set
            {
                if (_data_header.controller_state != value)
                {
                    _data_header.controller_state = value;
                    RaisePropertyChanged("controller_state");
                }
            }

        }

        public string textbox1_status_messages
        {
            get { return _data_header.textbox1_status_messages; }
            set
            {
                if (_data_header.textbox1_status_messages != value)
                {
                    _data_header.textbox1_status_messages = value;
                    RaisePropertyChanged("textbox1_status_messages");
                }
            }

        }

        public byte[] brightness_controller
        {
            get { return _data_header.brightness_controller; }
            set
            {
                if (_data_header.brightness_controller != value)
                {
                    _data_header.brightness_controller = value;
                    RaisePropertyChanged("brightness_controller");
                }
            }

        }

        public int[] brightness_counter
        {
            get { return _data_header.brightness_counter; }
            set
            {
                if (_data_header.brightness_counter != value)
                {
                    _data_header.brightness_counter = value;
                    RaisePropertyChanged("brightness_counter");
                }
            }

        }

        public byte[] brightness_whitemax_mixed_rgb
        {
            get { return _data_header.brightness_whitemax_mixed_rgb; }
            set
            {
                if (_data_header.brightness_whitemax_mixed_rgb != value)
                {
                    _data_header.brightness_whitemax_mixed_rgb = value;
                    RaisePropertyChanged("brightness_whitemax_mixed_rgb");
                }
            }

        }

        public string[] effect
        {
            get { return _data_header.effect; }
            set
            {
                if (_data_header.effect != value)
                {
                    _data_header.effect = value;
                    RaisePropertyChanged("effect");
                }
            }

        }
        public int fade_stepsize
        {
            get { return _data_header.fade_stepsize; }
            set
            {
                if (_data_header.fade_stepsize != value)
                {
                    _data_header.fade_stepsize = value;
                    RaisePropertyChanged("fade_stepsize");
                }
            }

        }

        public byte[] fade_target
        {
            get { return _data_header.fade_target; }
            set
            {
                if (_data_header.fade_target != value)
                {
                    _data_header.fade_target = value;
                    RaisePropertyChanged("fade_target");
                }
            }

        }

        public string gui_full_control_topic
        {
            get { return _data_header.gui_full_control_topic; }
            set
            {
                if (_data_header.gui_full_control_topic != value)
                {
                    _data_header.gui_full_control_topic = value;
                    RaisePropertyChanged("gui_full_control_topic");
                }
            }

        }

        public string gui_full_publish_topic
        {
            get { return _data_header.gui_full_publish_topic; }
            set
            {
                if (_data_header.gui_full_publish_topic != value)
                {
                    _data_header.gui_full_publish_topic = value;
                    RaisePropertyChanged("gui_full_publish_topic");
                }
            }

        }

        public int[] tics_to_next_pulse
        {
            get { return _data_header.tics_to_next_pulse; }
            set
            {
                if (_data_header.tics_to_next_pulse != value)
                {
                    _data_header.tics_to_next_pulse = value;
                    RaisePropertyChanged("tics_to_next_pulse");
                }
            }

        }

        public int[] controller_tic_start
        {
            get { return _data_header.controller_tic_start; }
            set
            {
                if (_data_header.controller_tic_start != value)
                {
                    _data_header.controller_tic_start = value;
                    RaisePropertyChanged("controller_tic_start");
                }
            }

        }

        public int controller_tic_counter
        {
            get { return _data_header.controller_tic_counter; }
            set
            {
                if (_data_header.controller_tic_counter != value)
                {
                    _data_header.controller_tic_counter = value;
                    RaisePropertyChanged("controller_tic_start");
                }
            }

        }

        public byte[] colors_red
        {
            get { return _data_header.colors_red; }
            set
            {
                if (_data_header.colors_red != value)
                {
                    _data_header.colors_red = value;
                    RaisePropertyChanged("colors_red");
                }
            }

        }

        public byte[] colors_green
        {
            get { return _data_header.colors_green; }
            set
            {
                if (_data_header.colors_green != value)
                {
                    _data_header.colors_green = value;
                    RaisePropertyChanged("colors_green");
                }
            }

        }

        public byte[] colors_blue
        {
            get { return _data_header.colors_blue; }
            set
            {
                if (_data_header.colors_blue != value)
                {
                    _data_header.colors_blue = value;
                    RaisePropertyChanged("colors_blue");
                }
            }

        }

        public string[] colors_name
        {
            get { return _data_header.colors_name; }
            set
            {
                if (_data_header.colors_name != value)
                {
                    _data_header.colors_name = value;
                    RaisePropertyChanged("colors_name");
                }
            }

        }

        public int[] colormorph_speed
        {
            get { return _data_header.colormorph_speed; }
            set
            {
                if (_data_header.colormorph_speed != value)
                {
                    _data_header.colormorph_speed = value;
                    RaisePropertyChanged("colormorph_speed");
                }
            }

        }

        public int[] last_random_color
        {
            get { return _data_header.last_random_color; }
            set
            {
                if (_data_header.last_random_color != value)
                {
                    _data_header.last_random_color = value;
                    RaisePropertyChanged("last_random_color");
                }
            }

        }

        public byte[] colors_white
        {
            get { return _data_header.colors_white; }
            set
            {
                if (_data_header.colors_white != value)
                {
                    _data_header.colors_white = value;
                    RaisePropertyChanged("colors_white");
                }
            }

        }

        public byte[] colors_white2
        {
            get { return _data_header.colors_white2; }
            set
            {
                if (_data_header.colors_white2 != value)
                {
                    _data_header.colors_white2 = value;
                    RaisePropertyChanged("colors_white2");
                }
            }

        }

        public byte[,] colors_red_file
        {
            get { return _data_header.colors_red_file; }
            set
            {
                if (_data_header.colors_red_file != value)
                {
                    _data_header.colors_red_file = value;
                    RaisePropertyChanged("colors_red_file");
                }
            }

        }
        public byte[,] colors_green_file
        {
            get { return _data_header.colors_green_file; }
            set
            {
                if (_data_header.colors_green_file != value)
                {
                    _data_header.colors_green_file = value;
                    RaisePropertyChanged("colors_green_file");
                }
            }

        }
        public byte[,] colors_blue_file
        {
            get { return _data_header.colors_blue_file; }
            set
            {
                if (_data_header.colors_blue_file != value)
                {
                    _data_header.colors_blue_file = value;
                    RaisePropertyChanged("colors_blue_file");
                }
            }

        }
        public byte[,] colors_white_file
        {
            get { return _data_header.colors_white_file; }
            set
            {
                if (_data_header.colors_white_file != value)
                {
                    _data_header.colors_white_file = value;
                    RaisePropertyChanged("colors_white_file");
                }
            }

        }

        

        public string[,] colors_name_file
        {
            get { return _data_header.colors_name_file; }
            set
            {
                if (_data_header.colors_name_file != value)
                {
                    _data_header.colors_name_file = value;
                    RaisePropertyChanged("colors_name_file");
                }
            }

        }

        public byte[,] colors_white2_file
        {
            get { return _data_header.colors_white2_file; }
            set
            {
                if (_data_header.colors_white2_file != value)
                {
                    _data_header.colors_white2_file = value;
                    RaisePropertyChanged("colors_white2_file");
                }
            }

        }

        public bool[] colormorph_random_color
        {
            get { return _data_header.colormorph_random_color; }
            set
            {
                if (_data_header.colormorph_random_color != value)
                {
                    _data_header.colormorph_random_color = value;
                    RaisePropertyChanged("colormorph_random_color");
                }
            }

        }

        public byte[] colormorph_single_color
        {
            get { return _data_header.colormorph_single_color; }
            set
            {
                if (_data_header.colormorph_single_color != value)
                {
                    _data_header.colormorph_single_color = value;
                    RaisePropertyChanged("colormorph_single_color");
                }
            }

        }

        public int colormorph_fade_stepsize
        {
            get { return _data_header.colormorph_fade_stepsize; }
            set
            {
                if (_data_header.colormorph_fade_stepsize != value)
                {
                    _data_header.colormorph_fade_stepsize = value;
                    RaisePropertyChanged("colormorph_fade_stepsize");
                }
            }

        }

        public List<string> gui_colors_name_ch_current
        {
            get { return _data_header.gui_colors_name_ch_current; }
            set
            {
                _data_header.gui_colors_name_ch_current = value;
                RaisePropertyChanged("gui_colors_name_ch_current");
            }
        }

        public List<string> gui_colors_red_ch_current
        {
            get { return _data_header.gui_colors_red_ch_current; }
            set
            {
                _data_header.gui_colors_red_ch_current = value;
                RaisePropertyChanged("gui_colors_red_ch_current");
            }
        }

        public List<string> gui_colors_blue_ch_current
        {
            get { return _data_header.gui_colors_blue_ch_current; }
            set
            {
                _data_header.gui_colors_blue_ch_current = value;
                RaisePropertyChanged("gui_colors_blue_ch_current");
            }
        }

        public List<string> gui_colors_green_ch_current
        {
            get { return _data_header.gui_colors_green_ch_current; }
            set
            {
                _data_header.gui_colors_green_ch_current = value;
                RaisePropertyChanged("gui_colors_green_ch_current");
            }
        }

        public List<string> gui_colors_warm_white_ch_current
        {
            get { return _data_header.gui_colors_warm_white_ch_current; }
            set
            {
                _data_header.gui_colors_warm_white_ch_current = value;
                RaisePropertyChanged("gui_colors_warm_white_ch_current");
            }
        }

        public List<string> gui_colors_cold_white_ch_current
        {
            get { return _data_header.gui_colors_cold_white_ch_current; }
            set
            {
                _data_header.gui_colors_cold_white_ch_current = value;
                RaisePropertyChanged("gui_colors_cold_white_ch_current");
            }
        }

        public string gui_rectangle_color
        {
            get { return _data_header.gui_rectangle_color; }
            set
            {
                _data_header.gui_rectangle_color = value;
                RaisePropertyChanged("gui_rectangle_color");
            }
        }

        public string gui_textbox_color_name
        {
            get { return _data_header.gui_textbox_color_name; }
            set
            {
                _data_header.gui_textbox_color_name = value;
                RaisePropertyChanged("gui_textbox_color_name");
            }
        }

        

        

        public bool gui_unsaved_data
        {
            get { return _data_header.gui_unsaved_data; }
            set
            {
                _data_header.gui_unsaved_data = value;
                RaisePropertyChanged("gui_unsaved_data");
            }
        }

        public bool gui_execute_config_save
        {
            get { return _data_header.gui_execute_config_save; }
            set
            {
                _data_header.gui_execute_config_save = value;
                RaisePropertyChanged("gui_execute_config_save");
            }
        }

        public bool[] led_change_in_progress
        {
            get { return _data_header.led_change_in_progress; }
            set
            {
                _data_header.led_change_in_progress = value;
                RaisePropertyChanged("led_change_in_progress");
            }
        }

    }
        #endregion
}


