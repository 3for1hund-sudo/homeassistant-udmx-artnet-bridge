using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using MicroMvvm;
using System.Collections.ObjectModel;


namespace uDMXtoMQTT
{
     
    class sol_objects
    {
        #region members
        ObservableCollection<dataclassgui> _data_object = new ObservableCollection<dataclassgui>();
       
        #endregion 

        public ObservableCollection<dataclassgui> data_object
        {
            get { return _data_object; }
            set { _data_object = value;}
        }

        public sol_objects()
        {
           // _data_object.Add(new dataclassgui { data_header = new header_decoded { } });
                     
        }

       

    }
}
