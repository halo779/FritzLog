using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FritzboxLog
{
    public class RelationalDouble
    {
        double value;
        DateTime recordTime;

        public RelationalDouble(double value, DateTime time)
        {
            this.value = value;
            recordTime = time;
        }

        public RelationalDouble(double value)
        {
            this.value = value;
            recordTime = DateTime.Now;
        }
    }
}
